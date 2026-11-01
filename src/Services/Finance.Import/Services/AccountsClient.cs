using System.Net;

namespace Finance.Import;

public class AccountsClient(HttpClient http) : IAccountsClient
{
    public async Task<bool> IsOwnerAsync(Guid userId, Guid accountId)
    {
        var response = await http.GetAsync($"internal/accounts/{accountId}?userId={userId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        response.EnsureSuccessStatusCode();

        return true;
    }

    public async Task<ImportResult> ImportAsync(Guid batchId, Guid userId, Guid accountId, List<NormalizedRow> rows)
    {
        var items = rows
            .Select(r => new ImportItem(r.Date, r.Amount, r.Description, r.Ordinal))
            .ToList();

        var body = new ImportRequest(batchId, userId, accountId, items);

        var response = await http.PostAsJsonAsync("internal/transactions/import", body);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ImportResult>();

        if (result == null)
        {
            throw new HttpRequestException("Accounts returned an empty response");
        }

        return result;
    }
}