using System.Text.Json;

namespace Finance.Messaging;

public static class MessageSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static byte[] Serialize<T>(T message)
    {
        return JsonSerializer.SerializeToUtf8Bytes(message, Options);
    }

    public static T Deserialize<T>(ReadOnlyMemory<byte> body)
    {
        var result = JsonSerializer.Deserialize<T>(body.Span, Options);

        if (result == null)
        {
            throw new InvalidOperationException("Message body is missing or null.");
        }

        return result;
    }
}
