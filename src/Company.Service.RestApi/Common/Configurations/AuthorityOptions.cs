namespace Company.Service.RestApi.Common.Configurations;

internal class AuthorityOptions
{
    public const string SectionName = "Authority";

    public required string Scope { get; set; }

    public required string Url { get; set; }

    public required bool RequireHttps { get; set; } = true;

    public bool Enabled { get; set; } = true;
}
