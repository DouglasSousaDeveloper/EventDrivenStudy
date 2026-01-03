using System.Text.Json;

namespace Shared.Serialization;

public static class JsonSerializationExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static byte[] Serialize<T>(this T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, DefaultOptions);
    }

    public static T Deserialize<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, DefaultOptions)!
               ?? throw new InvalidOperationException("Failed to deserialize message.");
    }
}
