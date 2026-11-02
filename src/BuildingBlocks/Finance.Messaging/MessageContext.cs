namespace Finance.Messaging;

public record MessageContext(string MessageId, string? CorrelationId);