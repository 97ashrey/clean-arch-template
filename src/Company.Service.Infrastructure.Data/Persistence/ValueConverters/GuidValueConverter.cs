using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Company.Service.Infrastructure.Data.Persistence.ValueConverters;

/// <summary>
/// Converts <see cref="Guid"/> to <see cref="byte[]"/> for storage as <c>binary(16)</c>.
/// Uses big-endian (RFC 4122) byte order — the standard UUID layout — so that version 7
/// time-ordered UUIDs sort correctly in a <c>binary(16)</c> column.
/// </summary>
public class GuidValueConverter : ValueConverter<Guid, byte[]>
{
    public GuidValueConverter()
        : base(
            v => v.ToByteArray(bigEndian: true),
            v => new Guid(v, bigEndian: true))
    {
    }
}
