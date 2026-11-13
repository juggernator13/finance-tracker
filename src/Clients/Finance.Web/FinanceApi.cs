using System.Net.Http.Headers;

using Microsoft.AspNetCore.Components.Forms;

namespace Finance.Web;

public class FinanceApi(IHttpClientFactory factory, AuthState auth)
{
    private HttpClient Client()
    {
        var http = factory.CreateClient("gateway");
        if (auth.Token != null)
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        }
        return http;
    }

    private static async Task Check(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            throw new Exception($"{(int)response.StatusCode}: {body}");
        }
    }

    private async Task<T> GetAsync<T>(string url) where T : new()
    {
        var response = await Client().GetAsync(url);
        await Check(response);
        return await response.Content.ReadFromJsonAsync<T>() ?? new T();
    }

    private async Task PostAsync(string url, object body)
    {
        var response = await Client().PostAsJsonAsync(url, body);
        await Check(response);
    }

    public async Task LoginAsync(string email, string password)
    {
        var response = await Client().PostAsJsonAsync("api/auth/login", new { email, password });
        await Check(response);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        auth.Login(result!.AccessToken);
    }

    public Task<List<AccountDto>> GetAccountsAsync()
        => GetAsync<List<AccountDto>>("api/accounts");

    public Task CreateAccountAsync(string name, string currency)
        => PostAsync("api/accounts", new { name, currency });

    public Task<List<TransactionDto>> GetTransactionsAsync(Guid accountId)
        => GetAsync<List<TransactionDto>>($"api/accounts/{accountId}/transactions");

    public Task AddTransactionAsync(Guid accountId, DateOnly date, decimal amount, string description)
        => PostAsync($"api/accounts/{accountId}/transactions", new { date, amount, description });

    public async Task DeleteTransactionAsync(Guid accountId, Guid id)
    {
        var response = await Client().DeleteAsync($"api/transactions/{id}");
        await Check(response);
    }

    public Task<List<string>> GetFormatsAsync()
        => GetAsync<List<string>>("api/imports/formats");

    public Task<List<ImportBatchDto>> GetImportsAsync()
        => GetAsync<List<ImportBatchDto>>("api/imports");

    public async Task<ImportBatchDto> UploadAsync(Guid accountId, string? format, IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(accountId.ToString()), "accountId");

        if (!string.IsNullOrWhiteSpace(format))
        {
            content.Add(new StringContent(format), "format");
        }

        using var stream = file.OpenReadStream(5 * 1024 * 1024);
        content.Add(new StreamContent(stream), "file", file.Name);

        var response = await Client().PostAsync("api/imports", content);
        await Check(response);
        return (await response.Content.ReadFromJsonAsync<ImportBatchDto>())!;
    }

    public Task<List<CategoryReportItem>> GetCategoryReportAsync(int year, int month)
        => GetAsync<List<CategoryReportItem>>($"api/reports/by-category?year={year}&month={month}");

    public Task<List<MonthlyReportItem>> GetMonthlyReportAsync(int year)
        => GetAsync<List<MonthlyReportItem>>($"api/reports/monthly?year={year}");
}