using System.Net.Http.Headers;

using Microsoft.AspNetCore.Components.Forms;

namespace Finance.Web;

public class FinanceApi(IHttpClientFactory factory, AuthState auth)
{
    private HttpClient Client(string name)
    {
        var http = factory.CreateClient(name);
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

    private async Task<T> GetAsync<T>(string client, string url) where T : new()
    {
        var response = await Client(client).GetAsync(url);
        await Check(response);
        return await response.Content.ReadFromJsonAsync<T>() ?? new T();
    }

    private async Task PostAsync(string client, string url, object body)
    {
        var response = await Client(client).PostAsJsonAsync(url, body);
        await Check(response);
    }

    public async Task LoginAsync(string email, string password)
    {
        var response = await Client("identity").PostAsJsonAsync("api/auth/login", new { email, password });
        await Check(response);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        auth.Login(result!.AccessToken);
    }

    public Task<List<AccountDto>> GetAccountsAsync()
        => GetAsync<List<AccountDto>>("accounts", "api/accounts");

    public Task CreateAccountAsync(string name, string currency)
        => PostAsync("accounts", "api/accounts", new { name, currency });

    public Task<List<TransactionDto>> GetTransactionsAsync(Guid accountId)
        => GetAsync<List<TransactionDto>>("accounts", $"api/accounts/{accountId}/transactions");

    public Task AddTransactionAsync(Guid accountId, DateOnly date, decimal amount, string description)
        => PostAsync("accounts", $"api/accounts/{accountId}/transactions", new { date, amount, description });

    public async Task DeleteTransactionAsync(Guid accountId, Guid id)
    {
        var response = await Client("accounts").DeleteAsync($"api/transactions/{id}");
        await Check(response);
    }

    public Task<List<string>> GetFormatsAsync()
        => GetAsync<List<string>>("import", "api/imports/formats");

    public Task<List<ImportBatchDto>> GetImportsAsync()
        => GetAsync<List<ImportBatchDto>>("import", "api/imports");

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

        var response = await Client("import").PostAsync("api/imports", content);
        await Check(response);
        return (await response.Content.ReadFromJsonAsync<ImportBatchDto>())!;
    }
}