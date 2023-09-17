namespace Backend.Features.Domains;

public class DomainStatus
{
    public string Domain { get; }
    public string Status { get; }
    public DomainVerification? Verification { get; }

    private DomainStatus(string domain, string status, DomainVerification? verification = null)
    {
        Domain = domain;
        Status = status;
        Verification = verification;
    }

    public static DomainStatus NoStatus(string domain) => new(domain, "no-status");

    public static DomainStatus Taken(string domain) => new(domain, "taken");

    public static DomainStatus Verified(string domain) => new(domain, "verified");

    public static DomainStatus NotVerified(string domain, DomainVerification verification)
    {
        return new(domain, "not-verified", verification);
    }

    /// <summary>
    /// Send requests to Vercel Domain API and transform into Domain Status.
    /// </summary>
    //public static async Task<DomainStatus> RequestStatus(VercelDomainApi api, string domain)
    //{
    //}
}