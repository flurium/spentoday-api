using Lib;

namespace Backend.Services;

public record DomainVerification(string Type, string Domain, string Value);
public record DomainResponse(bool Verified, List<DomainVerification>? Verification);

public class DomainService
{
    private readonly HttpClient client;
    private readonly string projectId;
    private readonly string teamId;

    public DomainService(HttpClient client, string token, string projectId, string teamId)
    {
        this.projectId = projectId;
        this.teamId = teamId;

        this.client = client;
        this.client.BaseAddress = new Uri("https://api.vercel.com/");
        this.client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    public async Task<DomainResponse?> AddDomainToShop(string domain)
    {
        var body = $@"{{""name"":""{domain}""}}";
        var route = $"/v10/projects/{projectId}/domains?teamId={teamId}";

        var response = await Http.JsonPost(client, route, body);
        if (response == null) return null;

        var data = await Http.JsonResponse<DomainResponse>(response);
        return data;
    }

    public async Task<DomainResponse?> GetDomainInfo(string domain)
    {
        var route = $"/v9/projects/{projectId}/domains/{domain}?teamId={teamId}";
        var response = await Http.Get(client, route);
        if (response == null || !response.IsSuccessStatusCode) return null;

        var data = await Http.JsonResponse<DomainResponse>(response);
        return data;
    }

    /// <returns>True if verified, otherwise false.</returns>
    public async Task<bool> VerifyDomain(string domain)
    {
        var route = $"/v9/projects/{projectId}/domains/{domain}/verify?teamId={teamId}";
        var response = await Http.Post(client, route);
        if (response == null) return false;
        return response.IsSuccessStatusCode;
    }

    /// <returns>True if removed, otherwise false.</returns>
    public async Task<bool> RemoveDomainFromShop(string domain)
    {
        var route = $"/v9/projects/{projectId}/domains/{domain}?teamId={teamId}";
        var response = await Http.Delete(client, route);
        if (response == null) return false;
        return response.IsSuccessStatusCode;
    }
}