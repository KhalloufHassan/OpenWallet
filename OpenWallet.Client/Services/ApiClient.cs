using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using OpenWallet.Shared.DTOs;
using OpenWallet.Shared.Models;

namespace OpenWallet.Client.Services;

public class ApiClient(HttpClient http)
{
    public async Task<SetupStatusDto> GetSetupStatusAsync() =>
        await http.GetFromJsonAsync<SetupStatusDto>("api/setup/status") ?? new();

    public async Task<string?> CompleteSetupAsync(SetupDto dto)
    {
        HttpResponseMessage r = await http.PostAsJsonAsync("api/setup", dto);
        if (r.IsSuccessStatusCode) return null;
        string body = await r.Content.ReadAsStringAsync();
        try
        {
            System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out System.Text.Json.JsonElement el))
                return el.GetString() ?? "Setup failed.";
        }
        catch { }
        return "Setup failed. It may have already been completed.";
    }

    public async Task<AuthMeDto?> GetMeAsync()
    {
        HttpResponseMessage r = await http.GetAsync("api/auth/me");
        if (r.StatusCode == HttpStatusCode.Unauthorized) return null;
        return await r.Content.ReadFromJsonAsync<AuthMeDto>();
    }

    public async Task<LoginResultDto> LoginAsync(LoginDto dto)
    {
        HttpResponseMessage r = await http.PostAsJsonAsync("api/auth/login", dto);
        return await r.Content.ReadFromJsonAsync<LoginResultDto>() ?? new();
    }

    public async Task<LoginResultDto> VerifyTotpAsync(VerifyTotpDto dto)
    {
        HttpResponseMessage r = await http.PostAsJsonAsync("api/auth/totp/verify", dto);
        return await r.Content.ReadFromJsonAsync<LoginResultDto>() ?? new();
    }

    public async Task<SecurityStatusDto> GetSecurityStatusAsync() =>
        await http.GetFromJsonAsync<SecurityStatusDto>("api/auth/security") ?? new();

    public async Task<TotpSetupDto> SetupTotpAsync()
    {
        HttpResponseMessage r = await http.PostAsync("api/auth/totp/setup", null);
        return await r.Content.ReadFromJsonAsync<TotpSetupDto>() ?? new();
    }

    public async Task<bool> EnableTotpAsync(VerifyTotpDto dto)
    {
        HttpResponseMessage r = await http.PostAsJsonAsync("api/auth/totp/enable", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DisableTotpAsync()
    {
        HttpResponseMessage r = await http.PostAsync("api/auth/totp/disable", null);
        return r.IsSuccessStatusCode;
    }

    public async Task<string> GetPasskeyRegisterOptionsAsync(RegisterPasskeyNameDto dto)
    {
        HttpResponseMessage r = await http.PostAsJsonAsync("api/auth/passkey/register/options", dto);
        return await r.Content.ReadAsStringAsync();
    }

    public async Task<(PasskeyInfoDto? Info, string? Error)> CompletePasskeyRegisterAsync(string credentialJson)
    {
        using StringContent content = new(credentialJson, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage r = await http.PostAsync("api/auth/passkey/register/complete", content);
        if (!r.IsSuccessStatusCode)
        {
            string body = await r.Content.ReadAsStringAsync();
            return (null, body);
        }
        PasskeyInfoDto? info = await r.Content.ReadFromJsonAsync<PasskeyInfoDto>();
        return (info, null);
    }

    public async Task<bool> DeletePasskeyAsync(int id)
    {
        HttpResponseMessage r = await http.DeleteAsync($"api/auth/passkey/{id}");
        return r.IsSuccessStatusCode;
    }

    public async Task<string?> GetPasskeyLoginOptionsAsync(bool platform = false)
    {
        string url = platform ? "api/auth/passkey/login/options?platform=true" : "api/auth/passkey/login/options";
        HttpResponseMessage r = await http.PostAsync(url, null);
        if (r.StatusCode == System.Net.HttpStatusCode.NoContent) return null;
        return await r.Content.ReadAsStringAsync();
    }

    public async Task<LoginResultDto> CompletePasskeyLoginAsync(string assertionJson)
    {
        using StringContent content = new(assertionJson, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage r = await http.PostAsync("api/auth/passkey/login/complete", content);
        return await r.Content.ReadFromJsonAsync<LoginResultDto>() ?? new();
    }

    public async Task LogoutAsync() =>
        await http.PostAsync("api/auth/logout", null);

    public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
    {
        HttpResponseMessage r = await http.PostAsJsonAsync("api/auth/change-password", dto);
        return r.IsSuccessStatusCode;
    }


    public async Task<List<AccountDto>> GetAccountsAsync() =>
        await http.GetFromJsonAsync<List<AccountDto>>("api/accounts") ?? [];

    public async Task<AccountDto> CreateAccountAsync(CreateAccountDto dto) =>
        (await http.PostAsJsonAsync("api/accounts", dto)).EnsureSuccessStatusCode()
            is var r ? await r.Content.ReadFromJsonAsync<AccountDto>() ?? new() : new();

    public async Task<AccountDto> UpdateAccountAsync(int id, UpdateAccountDto dto) =>
        await (await http.PutAsJsonAsync($"api/accounts/{id}", dto)).Content.ReadFromJsonAsync<AccountDto>() ?? new();

    public async Task DeleteAccountAsync(int id) =>
        (await http.DeleteAsync($"api/accounts/{id}")).EnsureSuccessStatusCode();

    public async Task<List<CategoryDto>> GetCategoriesAsync() =>
        await http.GetFromJsonAsync<List<CategoryDto>>("api/categories") ?? [];

    public async Task<List<CategoryDto>> GetCategoriesFlatAsync() =>
        await http.GetFromJsonAsync<List<CategoryDto>>("api/categories/flat") ?? [];

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto) =>
        await (await http.PostAsJsonAsync("api/categories", dto)).Content.ReadFromJsonAsync<CategoryDto>() ?? new();

    public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto) =>
        await (await http.PutAsJsonAsync($"api/categories/{id}", dto)).Content.ReadFromJsonAsync<CategoryDto>() ?? new();

    public async Task DeleteCategoryAsync(int id) =>
        (await http.DeleteAsync($"api/categories/{id}")).EnsureSuccessStatusCode();

    public async Task<List<StoreDto>> GetStoresAsync() =>
        await http.GetFromJsonAsync<List<StoreDto>>("api/stores") ?? [];

    public async Task<StoreDto> CreateStoreAsync(CreateStoreDto dto) =>
        await (await http.PostAsJsonAsync("api/stores", dto)).Content.ReadFromJsonAsync<StoreDto>() ?? new();

    public async Task<StoreDto> UpdateStoreAsync(int id, CreateStoreDto dto) =>
        await (await http.PutAsJsonAsync($"api/stores/{id}", dto)).Content.ReadFromJsonAsync<StoreDto>() ?? new();

    public async Task DeleteStoreAsync(int id) =>
        (await http.DeleteAsync($"api/stores/{id}")).EnsureSuccessStatusCode();

    public async Task<List<TagDto>> GetTagsAsync() =>
        await http.GetFromJsonAsync<List<TagDto>>("api/tags") ?? [];

    public async Task<TagDto> CreateTagAsync(CreateTagDto dto) =>
        await (await http.PostAsJsonAsync("api/tags", dto)).Content.ReadFromJsonAsync<TagDto>() ?? new();

    public async Task<TagDto> UpdateTagAsync(int id, CreateTagDto dto) =>
        await (await http.PutAsJsonAsync($"api/tags/{id}", dto)).Content.ReadFromJsonAsync<TagDto>() ?? new();

    public async Task DeleteTagAsync(int id) =>
        (await http.DeleteAsync($"api/tags/{id}")).EnsureSuccessStatusCode();

    public async Task<RecordsPageResult> GetRecordsAsync(RecordFilterDto filter)
    {
        string query = BuildRecordQuery(filter);
        return await http.GetFromJsonAsync<RecordsPageResult>($"api/records?{query}") ?? new();
    }

    public async Task<RecordDto> CreateRecordAsync(CreateRecordDto dto) =>
        await (await http.PostAsJsonAsync("api/records", dto)).Content.ReadFromJsonAsync<RecordDto>() ?? new();

    public async Task<(RecordDto Outgoing, RecordDto Incoming)> CreateTransferAsync(CreateTransferDto dto)
    {
        HttpResponseMessage response = await http.PostAsJsonAsync("api/records/transfer", dto);
        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body);
        }
        TransferResult? result = await response.Content.ReadFromJsonAsync<TransferResult>();
        return (result?.Outgoing ?? new(), result?.Incoming ?? new());
    }

    public async Task<(RecordDto Outgoing, RecordDto Incoming)> UpdateTransferAsync(int id, CreateTransferDto dto)
    {
        HttpResponseMessage response = await http.PutAsJsonAsync($"api/records/transfer/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body);
        }
        TransferResult? result = await response.Content.ReadFromJsonAsync<TransferResult>();
        return (result?.Outgoing ?? new(), result?.Incoming ?? new());
    }

    public async Task<RecordDto> UpdateRecordAsync(int id, UpdateRecordDto dto) =>
        await (await http.PutAsJsonAsync($"api/records/{id}", dto)).Content.ReadFromJsonAsync<RecordDto>() ?? new();

    public async Task DeleteRecordAsync(int id) =>
        (await http.DeleteAsync($"api/records/{id}")).EnsureSuccessStatusCode();

    public async Task<AttachmentDto?> UploadAttachmentAsync(int recordId, IBrowserFile file)
    {
        using MultipartFormDataContent content = new();
        using StreamContent fileContent = new(file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.Name);
        HttpResponseMessage r = await http.PostAsync($"api/records/{recordId}/attachments", content);
        if (!r.IsSuccessStatusCode) return null;
        return await r.Content.ReadFromJsonAsync<AttachmentDto>();
    }

    public async Task DeleteAttachmentAsync(int recordId, int attachmentId) =>
        (await http.DeleteAsync($"api/records/{recordId}/attachments/{attachmentId}")).EnsureSuccessStatusCode();

    public async Task<List<TemplateDto>> GetTemplatesAsync() =>
        await http.GetFromJsonAsync<List<TemplateDto>>("api/templates") ?? [];

    public async Task<TemplateDto?> GetTemplateAsync(int id)
    {
        HttpResponseMessage r = await http.GetAsync($"api/templates/{id}");
        if (!r.IsSuccessStatusCode) return null;
        return await r.Content.ReadFromJsonAsync<TemplateDto>();
    }

    public async Task<TemplateDto> CreateTemplateAsync(CreateTemplateDto dto) =>
        await (await http.PostAsJsonAsync("api/templates", dto)).Content.ReadFromJsonAsync<TemplateDto>() ?? new();

    public async Task<TemplateDto> UpdateTemplateAsync(int id, UpdateTemplateDto dto) =>
        await (await http.PutAsJsonAsync($"api/templates/{id}", dto)).Content.ReadFromJsonAsync<TemplateDto>() ?? new();

    public async Task DeleteTemplateAsync(int id) =>
        (await http.DeleteAsync($"api/templates/{id}")).EnsureSuccessStatusCode();

    public async Task<List<DebtDto>> GetDebtsAsync() =>
        await http.GetFromJsonAsync<List<DebtDto>>("api/debts") ?? [];

    public async Task<DebtDto> CreateDebtAsync(CreateDebtDto dto) =>
        await (await http.PostAsJsonAsync("api/debts", dto)).Content.ReadFromJsonAsync<DebtDto>() ?? new();

    public async Task<DebtDto> UpdateDebtAsync(int id, UpdateDebtDto dto) =>
        await (await http.PutAsJsonAsync($"api/debts/{id}", dto)).Content.ReadFromJsonAsync<DebtDto>() ?? new();

    public async Task DeleteDebtAsync(int id) =>
        (await http.DeleteAsync($"api/debts/{id}")).EnsureSuccessStatusCode();

    public async Task LinkDebtRecordAsync(int debtId, int recordId) =>
        (await http.PostAsync($"api/debts/{debtId}/records/{recordId}", null)).EnsureSuccessStatusCode();

    public async Task UnlinkDebtRecordAsync(int debtId, int recordId) =>
        (await http.DeleteAsync($"api/debts/{debtId}/records/{recordId}")).EnsureSuccessStatusCode();

    public async Task<DashboardDto> GetDashboardAsync(DateTime? from = null, DateTime? to = null)
    {
        string query = string.Empty;
        if (from.HasValue) query += $"from={from.Value:O}&";
        if (to.HasValue) query += $"to={to.Value:O}";
        return await http.GetFromJsonAsync<DashboardDto>($"api/stats/dashboard?{query}") ?? new();
    }

    public async Task<List<CategoryExpenseDto>> GetExpensesByCategoryAsync(DateTime from, DateTime to) =>
        await http.GetFromJsonAsync<List<CategoryExpenseDto>>(
            $"api/stats/expenses-by-category?from={from:O}&to={to:O}") ?? [];

    public async Task<List<TagExpenseDto>> GetExpensesByTagAsync(DateTime from, DateTime to) =>
        await http.GetFromJsonAsync<List<TagExpenseDto>>(
            $"api/stats/expenses-by-tag?from={from:O}&to={to:O}") ?? [];

    public async Task<List<BalanceTrendDto>> GetBalanceTrendAsync(DateTime? from = null, DateTime? to = null)
    {
        string query = string.Empty;
        if (from.HasValue) query += $"from={from.Value:O}&";
        if (to.HasValue)   query += $"to={to.Value:O}";
        return await http.GetFromJsonAsync<List<BalanceTrendDto>>($"api/stats/balance-trend?{query}") ?? [];
    }

    private static string BuildRecordQuery(RecordFilterDto f)
    {
        List<string> parts = [];
        if (f.AccountId.HasValue) parts.Add($"accountId={f.AccountId}");
        if (f.CategoryId.HasValue) parts.Add($"categoryId={f.CategoryId}");
        if (f.StoreId.HasValue) parts.Add($"storeId={f.StoreId}");
        if (f.Type.HasValue) parts.Add($"type={f.Type}");
        if (f.FromDate.HasValue) parts.Add($"fromDate={f.FromDate.Value:O}");
        if (f.ToDate.HasValue) parts.Add($"toDate={f.ToDate.Value:O}");
        if (!string.IsNullOrWhiteSpace(f.Search)) parts.Add($"search={Uri.EscapeDataString(f.Search)}");
        foreach (int tagId in f.TagIds) parts.Add($"tagIds={tagId}");
        parts.Add($"page={f.Page}");
        parts.Add($"pageSize={f.PageSize}");
        return string.Join("&", parts);
    }
}

public class RecordsPageResult
{
    public List<RecordDto> Records { get; set; } = [];
    public int Total { get; set; }
}

public class TransferResult
{
    public RecordDto Outgoing { get; set; } = new();
    public RecordDto Incoming { get; set; } = new();
}
