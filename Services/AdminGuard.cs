namespace RodneyPortfolio.Services;

/// <summary>
/// Simple helper — call IsAdminAuthenticated() at the top of every admin PageModel.
/// If it returns false, redirect to /Admin/Login.
/// </summary>
public static class AdminGuard
{
    private const string SessionKey = "rc_admin_authenticated";

    public static bool IsAdminAuthenticated(HttpContext context)
    {
        return string.Equals(context.Session.GetString(SessionKey), "true", StringComparison.Ordinal);
    }

    public static void MarkAuthenticated(HttpContext context)
        => context.Session.SetString(SessionKey, "true");

    public static void ClearAuthentication(HttpContext context)
        => context.Session.Remove(SessionKey);
}
