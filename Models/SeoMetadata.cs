using System;
using System.Collections.Generic;

namespace RodneyPortfolio.Models
{
    public sealed class SeoMetadata
    {
        public required string Title { get; init; }
        public required string Description { get; init; }

        public string? CanonicalUrl { get; init; }
        public string? Robots { get; init; }

        public string? OpenGraphTitle { get; init; }
        public string? OpenGraphDescription { get; init; }
        public string? OpenGraphType { get; init; }
        public string? OpenGraphUrl { get; init; }
        public string? OpenGraphImage { get; init; }
        public string? OpenGraphImageAlt { get; init; }

        public string? TwitterCard { get; init; }
        public string? TwitterTitle { get; init; }
        public string? TwitterDescription { get; init; }
        public string? TwitterImage { get; init; }
        public string? TwitterImageAlt { get; init; }

        public DateTimeOffset? DatePublished { get; init; }
        public DateTimeOffset? DateModified { get; init; }

        public IReadOnlyCollection<string> Keywords { get; init; } = Array.Empty<string>();
        public IReadOnlyCollection<object> StructuredData { get; init; } = Array.Empty<object>();
    }
}
