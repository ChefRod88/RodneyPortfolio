using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace RodneyPortfolio.Services
{
    public interface ICanonicalUrlService
    {
        string GetCanonicalUrl(HttpContext context, string? overridePath = null);
    }

    public class CanonicalUrlService : ICanonicalUrlService
    {
        private const string CanonicalDomain = "https://www.rodneyachery.com";

        public string GetCanonicalUrl(HttpContext context, string? overridePath = null)
        {
            var path = overridePath ?? context.Request.Path.Value ?? "/";
            
            // Normalize trailing slash
            if (path.Length > 1 && path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            
            // Normalize casing according to routing policy (if any, typically lowercase or match exact razor route)
            // For now, we will trust the path provided or keep as is.
            // Exclude tracking parameters and fragments (they are not part of Path)

            return $"{CanonicalDomain}{path}";
        }
    }
}
