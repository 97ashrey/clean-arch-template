using Microsoft.Extensions.Configuration;

namespace Company.Service.AppHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);
        var config = builder.Configuration;

        var rabbitmq = builder.AddRabbitMQ(
                "RabbitMq",
                userName: builder.AddParameter("rabbitmq-username", secret: true),
                password: builder.AddParameter("rabbitmq-password", secret: true)
            )
            .WithManagementPlugin();

        // See documentation on https://github.com/navikt/mock-oauth2-server
        var identityServerTag = config.GetValue("ContainerImages:IdentityServer", "3.0.0");
        var identityServerPort = config.GetValue("Ports:IdentityServer", 52150);

        var identityServer = builder.AddContainer("identity-server", "ghcr.io/navikt/mock-oauth2-server", identityServerTag)
            .WithBindMount("./oauth2-mock", "/etc/oauth2-mock")
            .WithHttpEndpoint(port: identityServerPort, targetPort: 8080, name: "http")
            .WithEnvironment("JSON_CONFIG_PATH", "/etc/oauth2-mock/config.json");

        var sqlServerTag = config.GetValue("ContainerImages:SqlServer", "2022-CU25-ubuntu-22.04");
        var sqlServerPort = config.GetValue("Ports:SqlServer", 52160);
        var sqlVolumeName = config.GetValue("Volumes:SqlServerData", "serviceDomainPlaceholderDb-sql-data");

        var sqlServer = builder.AddSqlServer(
                "serviceDomainPlaceholder-service-sql-server",
                port: sqlServerPort,
                password: builder.AddParameter("db-password", secret: true))
            .WithLifetime(ContainerLifetime.Persistent)
            .WithImageTag(sqlServerTag);

        var sqlDatabase = sqlServer.WithVolume(sqlVolumeName, "/var/opt/mssql")
            .AddDatabase("ServiceDomainPlaceholderDb");

        var dbDeploy = builder.AddProject<Projects.Company_Service_DbDeploy>("serviceDomainPlaceholder-service-dbdeploy")
            .WaitFor(sqlDatabase)
            .WithReference(sqlDatabase);

        var authorityEnabled = config.GetValue("Authority:Enabled", false);
        var authorityRequireHttps = config.GetValue("Authority:RequireHttps", false);

        var serviceRestApi = builder.AddProject<Projects.Company_Service_RestApi>("serviceDomainPlaceholder-service-rest-api")
               .WaitFor(sqlDatabase)
               .WaitForCompletion(dbDeploy)
               .WaitFor(rabbitmq)
               .WaitFor(identityServer)
               .WithReference(rabbitmq, "MessagingBus")
               .WithReference(sqlDatabase)
               .WithEnvironment(context =>
               {
                   context.EnvironmentVariables["Authority__Enabled"] = authorityEnabled.ToString().ToLowerInvariant();
                   context.EnvironmentVariables["Authority__RequireHttps"] = authorityRequireHttps.ToString().ToLowerInvariant();
                   context.EnvironmentVariables["Authority__Url"] = identityServer.GetEndpoint("http").Url + "/connect";
               });

        builder.Build().Run();
    }
}
