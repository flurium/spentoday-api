using Lib;

namespace Backend.Config;

/// <summary>
/// Get all secrets from environment variables to static fields.
/// </summary>
public static class Secrets
{
    public static readonly string JWT_ISSUER;
    public static readonly string JWT_AUDIENCE;
    public static readonly string JWT_SECRET;

    public static readonly string RESEND_API_KEY;
    public static readonly string BREVO_API_KEY;
    public static readonly string SENDGRID_API_KEY;

    public static readonly string STORJ_ACCESS_KEY;
    public static readonly string STORJ_SECRET_KEY;
    public static readonly string STORJ_ENDPOINT;
    public static readonly string STORJ_PUBLIC_KEY;

    public static readonly string VERCEL_TOKEN;
    public static readonly string VERCEL_PROJECT_ID;
    public static readonly string VERCEL_TEAM_ID;

    public static readonly string DB_CONNECTION_STRING;

    static Secrets()
    {
        JWT_ISSUER = Env.GetRequired("JWT_ISSUER");
        JWT_AUDIENCE = Env.GetRequired("JWT_AUDIENCE");
        JWT_SECRET = Env.GetRequired("JWT_SECRET");

        RESEND_API_KEY = Env.GetRequired("RESEND_API_KEY");
        BREVO_API_KEY = Env.GetRequired("BREVO_API_KEY");
        SENDGRID_API_KEY = Env.GetRequired("SENDGRID_API_KEY");

        STORJ_ACCESS_KEY = Env.GetRequired("STORJ_ACCESS_KEY");
        STORJ_SECRET_KEY = Env.GetRequired("STORJ_SECRET_KEY");
        STORJ_ENDPOINT = Env.GetRequired("STORJ_ENDPOINT");
        STORJ_PUBLIC_KEY = Env.GetRequired("STORJ_PUBLIC_KEY");

        VERCEL_TOKEN = Env.GetRequired("VERCEL_TOKEN");
        VERCEL_PROJECT_ID = Env.GetRequired("VERCEL_PROJECT_ID");
        VERCEL_TEAM_ID = Env.GetRequired("VERCEL_TEAM_ID");

        DB_CONNECTION_STRING = Env.GetRequired("DB_CONNECTION_STRING");
    }
}