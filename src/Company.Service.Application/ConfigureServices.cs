using Company.Service.Application.Common.Behaviours;
using Company.Service.Application.Common.Interfaces.UserContext;
using FluentValidation;
using FluentValidation.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Service.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices<TUserProvider>(this IServiceCollection services) where TUserProvider : class, IUserProvider
    {
        services.AddMediator(options =>
        {
            options.Namespace = "Company.Service.Application";
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.GenerateTypesAsInternal = true;
            options.Assemblies = [typeof(IApplicationAssemblyMarker).Assembly];
            options.PipelineBehaviors = [typeof(ResultLoggingPipelineBehaviour<,>), typeof(ExceptionHandlerPipelineBehaviour<,>), typeof(ValidationPipelineBehaviour<,>)];
        });

        // Overrides the default normalization of property names used in validation messages
        // Default behaviour FizBuz -> Fiz Buz, FooBar.FizBuz -> Foo Bar Fiz Buz
        // Override FizBuz -> FizBuz, FooBar.FizBuz -> FooBar.FizBuz
        ValidatorOptions.Global.DisplayNameResolver = (type, member, expression) =>
        {
            if (expression != null)
            {
                var chain = PropertyChain.FromExpression(expression);
                if (chain.Count > 0) return chain.ToString();
            }

            return member?.Name;
        };
        services.AddValidatorsFromAssembly(typeof(IApplicationAssemblyMarker).Assembly, includeInternalTypes: true);
        
        services.AddScoped<IUserProvider, TUserProvider>();

        return services;
    }
}