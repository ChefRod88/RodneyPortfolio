using System.Threading;
using System.Threading.Tasks;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public class IcmRunnerResult
{
    public JobMatchResponse FinalAnalysis { get; set; } = new();
    public string Stage1Output { get; set; } = string.Empty;
    public string Stage2Output { get; set; } = string.Empty;
    public string Stage3Output { get; set; } = string.Empty;
    public int CompletedStage { get; set; }
}

public interface IIcmRunnerService
{
    Task<IcmRunnerResult> RunPipelineAsync(string jobDescription, int? targetStage = null, CancellationToken cancellationToken = default);
}
