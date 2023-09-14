using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;

namespace Backend.Features.Domains;

public class DomainService
{
    /// <summary>
    /// Get domain status from project domain.
    /// Don't do any side effects, like sync with db.
    /// </summary>
    /// <param name="dbDomain">Domain from database</param>
    /// <param name="projectDomain">Project domain from Vercel API</param>
    public static DomainStatus ProjectDomainStatus(ShopDomain dbDomain, ProjectDomain? projectDomain)
    {
        if (projectDomain == null) return DomainStatus.NoStatus(dbDomain.Domain);
        if (dbDomain.Verified) return DomainStatus.Verified(dbDomain.Domain);

        if (projectDomain.Verification != null && projectDomain.Verification.Count > 0)
        {
            return DomainStatus.NotVerified(dbDomain.Domain, projectDomain.Verification.First());
        }
        return DomainStatus.NoStatus(dbDomain.Domain);
    }

    /// <summary>
    /// Get domain status if domain is misconfigured.
    /// A or CNAME record in verification field.
    /// Don't do any side effects, like sync with db.
    /// </summary>
    /// <param name="dbDomain">Domain from database</param>
    /// <param name="projectDomain">Project domain from Vercel API</param>
    public static DomainStatus DomainConfigurationStatus(string domain, ProjectDomain? projectDomain)
    {
        if (projectDomain == null) return DomainStatus.NoStatus(domain);
        if (projectDomain.Name == projectDomain.ApexName)
        {
            return DomainStatus.NotVerified(domain, new("A", "@", "76.76.21.21"));
        }

        var prefix = projectDomain.Name.Substring(0, projectDomain.Name.LastIndexOf(projectDomain.ApexName) - 1);
        return DomainStatus.NotVerified(domain, new("CNAME", prefix, "cname.vercel-dns.com."));
    }

    public static async Task<bool> SyncDomainVerification(Db db, ShopDomain dbDomain, bool verified)
    {
        if (dbDomain.Verified == verified) return true;
        dbDomain.Verified = verified;
        return await db.Save();
    }

    /// <summary>
    /// Get domain status for single domain.
    /// Have a side effect: syncing verification+misconfiguration with database.
    /// </summary>
    public static async Task<DomainStatus> GetStatusAndSync(VercelDomainApi api, Db db, ShopDomain dbDomain)
    {
        var projectDomain = await api.GetProjectDomain(dbDomain.Domain);
        if (projectDomain == null) return DomainStatus.NoStatus(dbDomain.Domain);

        if (!projectDomain.Verified)
        {
            var synced = await SyncDomainVerification(db, dbDomain, false);
            if (!synced) return DomainStatus.NoStatus(dbDomain.Domain);

            // requires Vercel TXT verification
            var verification = projectDomain.Verification?.FirstOrDefault();
            if (verification == null) return DomainStatus.NoStatus(dbDomain.Domain);

            return DomainStatus.NotVerified(dbDomain.Domain, verification);
        }

        var domainConfiguration = await api.GetDomainConfiguration(dbDomain.Domain);
        if (domainConfiguration == null) return DomainStatus.NoStatus(dbDomain.Domain);

        if (domainConfiguration.Misconfigured)
        {
            var synced = await SyncDomainVerification(db, dbDomain, false);
            if (!synced) return DomainStatus.NoStatus(dbDomain.Domain);

            return DomainConfigurationStatus(dbDomain.Domain, projectDomain);
        }

        { // smart move
            var synced = await SyncDomainVerification(db, dbDomain, true);
            if (!synced) return DomainStatus.NoStatus(dbDomain.Domain);
        }

        return DomainStatus.Verified(dbDomain.Domain);
    }
}