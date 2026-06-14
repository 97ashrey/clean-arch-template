namespace Company.Service.AppHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var rabbitmq = builder.AddRabbitMQ(
                "RabbitMq",
                userName: builder.AddParameter("rabbitmq-username", "admin", secret: true),
                password: builder.AddParameter("rabbitmq-password", "admin", secret: true)
            )
            .WithManagementPlugin();

        // See documentation on https://github.com/navikt/mock-oauth2-server
        var identityServer = builder.AddContainer("identity-server", "ghcr.io/navikt/mock-oauth2-server", "3.0.0")
            .WithBindMount("./oauth2-mock", "/etc/oauth2-mock")
            .WithHttpEndpoint(port: 52150, targetPort: 8080, name: "http")
            .WithEnvironment("JSON_CONFIG_PATH", "/etc/oauth2-mock/config.json");

        var sqlServer = builder.AddSqlServer(
                "serviceDomainPlaceholder-service-sql-server",
                port: 52160,
                password: builder.AddParameter("db-password", "P@5sword", secret: true))
            .WithLifetime(ContainerLifetime.Persistent)
            .WithImageTag("2022-CU25-ubuntu-22.04");

        var sqlDatabase = sqlServer.WithVolume("serviceDomainPlaceholderDb-sql-data", "/var/opt/mssql")
            .AddDatabase("ServiceDomainPlaceholderDb");

        var dbDeploy = builder.AddProject<Projects.Company_Service_DbDeploy>("serviceDomainPlaceholder-service-dbdeploy")
            .WaitFor(sqlDatabase)
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables["ConnectionStrings__ServiceDomainPlaceholderDb"] = $"{sqlServer.Resource.GetConnectionStringAsync().Result};Initial Catalog={sqlDatabase.Resource.DatabaseName}";
            });

        var serviceRestApi = builder.AddProject<Projects.Company_Service_RestApi>("serviceDomainPlaceholder-service-rest-api")
               .WaitFor(sqlDatabase)
               .WaitForCompletion(dbDeploy)
               .WaitFor(rabbitmq)
               .WithReference(rabbitmq, "MessagingBus")
               .WithEnvironment(context =>
               {
                   context.EnvironmentVariables["Authority__Enabled"] = "false";
                   context.EnvironmentVariables["Authority__RequireHttps"] = "false";
                   context.EnvironmentVariables["Authority__Url"] = identityServer.GetEndpoint("http").Url + "/connect";

                   context.EnvironmentVariables["ConnectionStrings__ServiceDomainPlaceholderDb"] = $"{sqlServer.Resource.GetConnectionStringAsync().Result};Initial Catalog={sqlDatabase.Resource.DatabaseName}";
               });

        builder.Build().Run();
    }
}