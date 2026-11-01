using Microsoft.EntityFrameworkCore;

namespace Finance.Import.Tests;

public static class TestHelpers
{
    public const string Bank1Header = "Дата операции;Описание;Сумма";
    public const string Bank2Header = "Date,Description,Debit,Credit";

    public const string Bank1Csv =
        "Дата операции;Описание;Сумма\n20.10.2026;Пятёрочка;-1 250,50\n21.10.2026;Кофе;-150,00\n21.10.2026;Кофе;-150,00";

    public const string Bank2Csv =
        "Date,Description,Debit,Credit\n2026-10-20,\"Amazon, Inc.\",35.99,\n2026-10-22,Salary,,2500.00";

    public static ImportDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ImportDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ImportDbContext(options);
    }
}