namespace Finance.Import;

public class ImportBatch
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public string Format { get; set; } = "";
    public string Status { get; set; } = "";
    public int TotalRows { get; set; }
    public int Created { get; set; }
    public int Skipped { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
}