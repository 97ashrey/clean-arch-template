using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;
using Company.Service.Application;
using Company.Service.Infrastructure;
using Company.Service.RestApi.Common.Authorization;
using Company.Service.RestApi.Common.Configurations;
using Company.Service.RestApi.Common.Filters;
using Company.Service.RestApi.Common.Middleware;
using Company.Service.RestApi.Common.UserContext;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddApplicationServices<ApplicationUserProvider>();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<ExceptionHandlerFilter>();
});

builder.Services.AddApiVersioning(options =>
{
    // api/v1/....
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
})
.AddMvc(options =>
{
    options.Conventions.Add(new VersionByNamespaceConvention());
})
.AddOpenApi();

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
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();
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
app.UseSwaggerUI(options =>
{
    foreach (var description in app.DescribeApiVersions().Reverse())
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    }
});
#endif

app.UseHttpsRedirection();

app.UseHandleUnauthorized();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () =>
{
    return Results.Ok("Ok");
})
.ExcludeFromDescription();

if (authorityOptions!.Enabled)
{
    app.MapControllers();
}
else
{
    app.MapControllers().AllowAnonymous();
}

app.Run();

internal class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        // Generate a separate Swagger document for each discovered API version
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "ServiceDomainPlaceholder Service API",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated ? "This API version is deprecated." : "Active API version."
            });
        }

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
}