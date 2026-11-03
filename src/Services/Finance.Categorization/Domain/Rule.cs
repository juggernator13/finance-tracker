namespace Finance.Categorization;

public class Rule
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid CategoryId { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public bool isRegex { get; set; }
    public int Priority { get; set; }
}