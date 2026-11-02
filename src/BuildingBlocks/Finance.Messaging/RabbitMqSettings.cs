namespace Finance.Messaging
{
    public class RabbitMqSettings
    {
        public string Host { get; set; } = "localhost";
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}