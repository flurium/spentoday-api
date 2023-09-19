using Lib;

namespace Backend.Features.Domains;

public record DomainVerification(string Type, string Domain, string Value);
public record ProjectDomain(string Name, string ApexName, bool Verified, List<DomainVerification>? Verification);
public record DomainConfiguration(bool Misconfigured);

public class VercelDomainApi
{
    private readonly HttpClient client;
    private readonly string projectId;
    private readonly string teamId;

    public VercelDomainApi(HttpClient client, string token, string projectId, string teamId)
    {
        this.projectId = projectId;
        this.teamId = teamId;

        this.client = client;
        this.client.BaseAddress = new Uri("https://api.vercel.com/");
        this.client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    /// <summary>
    /// Send request to Vercel to add domain to project.
    /// </summary>
    public async Task<ProjectDomain?> AddDomain(string domain)
    {
        var body = $@"{{""name"":""{domain}""}}";
        var route = $"/v10/projects/{projectId}/domains?teamId={teamId}";

        var response = await Http.JsonPost(client, route, body);
        if (response == null) return null;

        var data = await Http.JsonResponse<ProjectDomain>(response);
        return data;
    }

    /// <summary>
    /// Send request to Vercel API to get inforamation about domain in project.
    /// Field Verified means domain doesn't conflict with Vercel.
    /// If not Verified then TXT dns record should be added to domain.
    /// </summary>
    public async Task<ProjectDomain?> GetProjectDomain(string domain)
    {
        var route = $"/v9/projects/{projectId}/domains/{domain}?teamId={teamId}";
        var response = await Http.Get(client, route);
        if (response == null || !response.IsSuccessStatusCode) return null;

        var data = await Http.JsonResponse<ProjectDomain>(response);
        return data;
    }

    /// <summary>
    /// Send request to Vercel API to get domain DNS configuration.
    /// If Misconfigured equals True, then CNAME or A dns record should be added to verify domain.
    /// </summary>
    public async Task<DomainConfiguration?> GetDomainConfiguration(string domain)
    {
        var route = $"/v6/domains/{domain}/config?teamId={teamId}";
        var response = await Http.Get(client, route);
        if (response == null || !response.IsSuccessStatusCode) { return null; }

        var data = await Http.JsonResponse<DomainConfiguration>(response);
        return data;
    }

    /// <summary>
    /// Send request to Vercel API to verify domain TXT dns record
    /// if domain is already in another Vercel account.
    /// </summary>
    public async Task<bool?> VerifyDomain(string domain)
    {
        var route = $"/v9/projects/{projectId}/domains/{domain}/verify?teamId={teamId}";
        var response = await Http.Post(client, route);

        if (response == null) return null;
        // Console.WriteLine(await response.Content.ReadAsStringAsync());
        return response.IsSuccessStatusCode;
    }

    /// <returns>True if removed, otherwise false.</returns>
    public async Task<bool> RemoveDomain(string domain)
    {
        var route = $"/v9/projects/{projectId}/domains/{domain}?teamId={teamId}";
        var response = await Http.Delete(client, route);
        if (response == null) return false;
        return response.IsSuccessStatusCode;
    }
}