using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Service for analyzing job description compatibility with Rodney's resume.
/// </summary>
public interface IJobMatchService
{
    /// <summary>
    /// Analyzes a job description against Rodney's resume and returns match score, skills alignment, gaps, and talking points.
    /// </summary>
    Task<JobMatchResponse> AnalyzeAsync(string jobDescription, CancellationToken cancellationToken = default);
}
