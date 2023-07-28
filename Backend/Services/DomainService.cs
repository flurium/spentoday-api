using Lib;

namespace Backend.Services;

public record DomainServiceSecrets(string Token, string ProjectId, string TeamId);
public record DomainVerification(string Type, string Domain, string Value, string Reason);
public record DomainResponse(bool Verified, List<DomainVerification>? Verification);

public class DomainService
{
    private readonly HttpClient client;
    private readonly string token;
    private readonly string projectId;
    private readonly string teamId;

    public DomainService(HttpClient client, string token, string projectId, string teamId)
    {
        this.token = token;
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

    public async Task<DomainResponse?> VerifyDomain(string domain)
    {
        var route = $"/v9/projects/{projectId}/domains/{domain}/verify?teamId={teamId}";
        var response = await client.PostAsync(route, null);

        var data = await Http.JsonResponse<DomainResponse>(response);
        return data;
    }

    public async Task<bool> RemoveDomainFromShop(string domain)
    {
        try
        {
            var route = $"/v9/projects/{projectId}/domains/{domain}?teamId={teamId}";
            var response = await client.DeleteAsync(route);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}