using Company.Service.Application;
using Company.Service.RestApi.Common.Authorization;
using Company.Service.RestApi.Common.Configurations;
using Company.Service.RestApi.Common.Filters;
using Company.Service.RestApi.Common.UserContext;
using Company.Service.Infrastructure;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry;
using Company.Service.RestApi.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddApplicationServices<ApplicationUserProvider>();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<ExceptionHandlerFilter>();
});

#if !DEBUG
    builder.Logging.ClearProviders();
#endif

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

var otel = builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
    });

if (builder.Configuration.GetValue<bool>("ExportTelemetry"))
{
    otel.UseOtlpExporter();
}

#if DEBUG
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SetupSwagger);
#endif

var authorityOptions = builder.Configuration.GetSection(AuthorityOptions.SectionName).Get<AuthorityOptions>();

if (authorityOptions!.Enabled)
{
    builder.Services.AddAuthentication("token")
        .AddJwtBearer("token", options =>
        {
            options.Authority = authorityOptions!.Url;

            options.RequireHttpsMetadata = authorityOptions.RequireHttps;

            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.ValidTypes = ["at+jwt", "jwt", "JWT"];
        });
}

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicyName.ApiCaller, policy =>
    {
        policy.RequireClaim("scope", authorityOptions!.Scope);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
#if DEBUG
app.UseSwagger();
app.UseSwaggerUI();
#endif

app.UseHttpsRedirection();

app.UseHandleUnauthorized();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () =>
{
    return Results.Ok("Ok");
});

if (authorityOptions!.Enabled)
{
    app.MapControllers();
}
else
{
    app.MapControllers().AllowAnonymous();
}

app.Run();

static void SetupSwagger(SwaggerGenOptions options)
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ServiceDomainPlaceholder Service API",
        Version = "v1",
        // TODO: Add API description.
        Description = ""
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });

    options.AddSecurityRequirement(document =>
    {
        OpenApiSecuritySchemeReference? schemeRef = new("Bearer");
        OpenApiSecurityRequirement? requirement = new()
        {
            [schemeRef] = []
        };
        return requirement;
    });
}