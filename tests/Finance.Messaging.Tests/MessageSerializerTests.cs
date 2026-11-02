using System.Text;

namespace Finance.Messaging.Tests;

public record Ping(Guid Id, string Text);
public record Payment(DateOnly Date, decimal Amount);

public class MessageSerializerTests
{
    [Fact]
    public void Ping_SurvivesRoundTrip()
    {
        var original = new Ping(Guid.NewGuid(), "Привет");

        var bytes = MessageSerializer.Serialize(original);
        var result = MessageSerializer.Deserialize<Ping>(bytes);

        Assert.Equal(original, result);
    }

    [Fact]
    public void DateAndDecimal_AreKept()
    {
        var original = new Payment(new DateOnly(2026, 10, 20), -1250.50m);

        var result = MessageSerializer.Deserialize<Payment>(MessageSerializer.Serialize(original));

        Assert.Equal(new DateOnly(2026, 10, 20), result.Date);
        Assert.Equal(-1250.50m, result.Amount);
    }

    [Fact]
    public void CamelCaseJson_IsRead()
    {
        string json = "{\"id\":\"11111111-1111-1111-1111-111111111111\",\"text\":\"hi\"}";

        var result = MessageSerializer.Deserialize<Ping>(Encoding.UTF8.GetBytes(json));

        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), result.Id);
        Assert.Equal("hi", result.Text);
    }

    [Fact]
    public void InvalidJson_Throws()
    {
        var bytes = Encoding.UTF8.GetBytes("not json");

        Assert.ThrowsAny<Exception>(() => MessageSerializer.Deserialize<Ping>(bytes));
    }

    [Fact]
    public void NullJson_ThrowsInvalidOperation()
    {
        var bytes = Encoding.UTF8.GetBytes("null");

        Assert.Throws<InvalidOperationException>(() => MessageSerializer.Deserialize<Ping>(bytes));
    }
}