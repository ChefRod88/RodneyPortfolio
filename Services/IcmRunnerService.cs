using System;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public class IcmRunnerService : IIcmRunnerService
{
    private readonly IConfiguration _config;
    private readonly IOpenAIClient _openAiClient;
    private readonly ILogger<IcmRunnerService> _logger;

    public IcmRunnerService(
        IConfiguration config,
        IOpenAIClient openAiClient,
        ILogger<IcmRunnerService> logger)
    {
        _config = config;
        _openAiClient = openAiClient;
        _logger = logger;
    }

    public async Task<IcmRunnerResult> RunPipelineAsync(string jobDescription, int? targetStage = null, CancellationToken cancellationToken = default)
    {
        var result = new IcmRunnerResult();
        var workspaceRoot = Path.Combine(Directory.GetCurrentDirectory(), "IcmWorkspace");

        try
        {
            // 0. Ensure folders exist (Step 4: separate reference/ and working/ directories)
            Directory.CreateDirectory(Path.Combine(workspaceRoot, "01_skill_extraction", "reference"));
            Directory.CreateDirectory(Path.Combine(workspaceRoot, "01_skill_extraction", "working"));
            Directory.CreateDirectory(Path.Combine(workspaceRoot, "02_resume_comparison", "reference"));
            Directory.CreateDirectory(Path.Combine(workspaceRoot, "02_resume_comparison", "working"));
            Directory.CreateDirectory(Path.Combine(workspaceRoot, "03_interview_preparation", "reference"));
            Directory.CreateDirectory(Path.Combine(workspaceRoot, "03_interview_preparation", "working"));

            // Read Layer 0 and Layer 1 configs
            var globalIdentity = await SafeReadFileAsync(Path.Combine(workspaceRoot, "_config", "global_identity.md"));
            var taskRouting = await SafeReadFileAsync(Path.Combine(workspaceRoot, "_config", "task_routing.md"));

            bool runS1 = targetStage == null || targetStage == 1;
            bool runS2 = targetStage == null || targetStage == 2;
            bool runS3 = targetStage == null || targetStage == 3;

            // -------------------------------------------------------------
            // STAGE 1: Skill Extraction
            // -------------------------------------------------------------
            if (runS1)
            {
                _logger.LogInformation("ICM Pipeline: Running Stage 1 (Skill Extraction)...");
                var jobDescPath = Path.Combine(workspaceRoot, "01_skill_extraction", "reference", "job_description.txt");
                if (!string.IsNullOrWhiteSpace(jobDescription))
                {
                    await File.WriteAllTextAsync(jobDescPath, jobDescription, cancellationToken);
                }
                
                var jobDesc = await SafeReadFileAsync(jobDescPath);
                var s1Contract = await SafeReadFileAsync(Path.Combine(workspaceRoot, "01_skill_extraction", "CONTEXT.md"));
                
                var s1Prompt = $@"
Identity Context (Layer 0):
{globalIdentity}

Routing Context (Layer 1):
{taskRouting}

Stage Contract (Layer 2):
{s1Contract}

Inputs:
- job_description.txt (Layer 4 Working Input):
{jobDesc}

Please execute the process described in the Stage Contract. Provide only the Markdown response for extracted_requirements.md.";

                var s1Output = await CallOpenAiAsync(s1Prompt, cancellationToken);
                if (string.IsNullOrEmpty(s1Output))
                {
                    throw new InvalidOperationException("Stage 1 execution failed to return an output.");
                }

                var s1OutputPath = Path.Combine(workspaceRoot, "01_skill_extraction", "working", "extracted_requirements.md");
                await File.WriteAllTextAsync(s1OutputPath, s1Output, cancellationToken);
                result.Stage1Output = s1Output;
                result.CompletedStage = 1;
            }

            // -------------------------------------------------------------
            // STAGE 2: Resume Comparison
            // -------------------------------------------------------------
            if (runS2)
            {
                _logger.LogInformation("ICM Pipeline: Running Stage 2 (Resume Comparison)...");
                
                // Step 8: State Handoff (Copy outputs from Stage 1 working/ to Stage 2 reference/)
                var s1SourcePath = Path.Combine(workspaceRoot, "01_skill_extraction", "working", "extracted_requirements.md");
                var s2DestRequirementsPath = Path.Combine(workspaceRoot, "02_resume_comparison", "reference", "extracted_requirements.md");
                
                if (File.Exists(s1SourcePath))
                {
                    File.Copy(s1SourcePath, s2DestRequirementsPath, overwrite: true);
                }

                // Copy RodneyResume reference file to stage reference folder
                var resumeSourcePath = Path.Combine(workspaceRoot, "_config", "skills", "RodneyResume.md");
                var resumeDestPath = Path.Combine(workspaceRoot, "02_resume_comparison", "reference", "RodneyResume.md");
                if (File.Exists(resumeSourcePath))
                {
                    File.Copy(resumeSourcePath, resumeDestPath, overwrite: true);
                }

                var requirementsText = await SafeReadFileAsync(s2DestRequirementsPath);
                var resumeText = await SafeReadFileAsync(resumeDestPath);
                var s2Contract = await SafeReadFileAsync(Path.Combine(workspaceRoot, "02_resume_comparison", "CONTEXT.md"));

                var s2Prompt = $@"
Identity Context (Layer 0):
{globalIdentity}

Routing Context (Layer 1):
{taskRouting}

Stage Contract (Layer 2):
{s2Contract}

Inputs:
- RodneyResume.md (Layer 3 Reference):
{resumeText}

- extracted_requirements.md (Layer 4 Working Input):
{requirementsText}

Please execute the process described in the Stage Contract. Provide only the Markdown response for comparison_results.md.";

                var s2Output = await CallOpenAiAsync(s2Prompt, cancellationToken);
                if (string.IsNullOrEmpty(s2Output))
                {
                    throw new InvalidOperationException("Stage 2 execution failed to return an output.");
                }

                var s2OutputPath = Path.Combine(workspaceRoot, "02_resume_comparison", "working", "comparison_results.md");
                await File.WriteAllTextAsync(s2OutputPath, s2Output, cancellationToken);
                result.Stage2Output = s2Output;
                result.CompletedStage = 2;
            }

            // -------------------------------------------------------------
            // STAGE 3: Interview Preparation
            // -------------------------------------------------------------
            if (runS3)
            {
                _logger.LogInformation("ICM Pipeline: Running Stage 3 (Interview Preparation)...");

                // Step 8: State Handoff (Copy outputs from Stage 2 working/ to Stage 3 reference/)
                var s2SourcePath = Path.Combine(workspaceRoot, "02_resume_comparison", "working", "comparison_results.md");
                var s3DestComparisonPath = Path.Combine(workspaceRoot, "03_interview_preparation", "reference", "comparison_results.md");
                if (File.Exists(s2SourcePath))
                {
                    File.Copy(s2SourcePath, s3DestComparisonPath, overwrite: true);
                }

                // Copy Resume reference to Stage 3 reference folder
                var resumeSourcePath = Path.Combine(workspaceRoot, "_config", "skills", "RodneyResume.md");
                var resumeDestPath = Path.Combine(workspaceRoot, "03_interview_preparation", "reference", "RodneyResume.md");
                if (File.Exists(resumeSourcePath))
                {
                    File.Copy(resumeSourcePath, resumeDestPath, overwrite: true);
                }

                // Copy Voice reference to Stage 3 reference folder
                var voiceSourcePath = Path.Combine(workspaceRoot, "_config", "voice.md");
                var voiceDestPath = Path.Combine(workspaceRoot, "03_interview_preparation", "reference", "voice.md");
                if (File.Exists(voiceSourcePath))
                {
                    File.Copy(voiceSourcePath, voiceDestPath, overwrite: true);
                }

                var comparisonText = await SafeReadFileAsync(s3DestComparisonPath);
                var resumeText = await SafeReadFileAsync(resumeDestPath);
                var voiceText = await SafeReadFileAsync(voiceDestPath);
                var s3Contract = await SafeReadFileAsync(Path.Combine(workspaceRoot, "03_interview_preparation", "CONTEXT.md"));

                var s3Prompt = $@"
Identity Context (Layer 0):
{globalIdentity}

Routing Context (Layer 1):
{taskRouting}

Stage Contract (Layer 2):
{s3Contract}

Inputs:
- voice.md (Layer 3 Reference):
{voiceText}

- RodneyResume.md (Layer 3 Reference):
{resumeText}

- comparison_results.md (Layer 4 Working Input):
{comparisonText}

Please execute the process described in the Stage Contract. Provide the Markdown response for interview_guide.md and append the JSON block containing the structured output at the very end.";

                var s3Output = await CallOpenAiAsync(s3Prompt, cancellationToken);
                if (string.IsNullOrEmpty(s3Output))
                {
                    throw new InvalidOperationException("Stage 3 execution failed to return an output.");
                }

                var s3OutputPath = Path.Combine(workspaceRoot, "03_interview_preparation", "working", "interview_guide.md");
                await File.WriteAllTextAsync(s3OutputPath, s3Output, cancellationToken);
                result.Stage3Output = s3Output;
                result.CompletedStage = 3;

                // Parse the JSON output from Stage 3
                result.FinalAnalysis = ParseFinalAnalysis(s3Output);
            }
            
            // If running a specific stage individually, fetch outputs from disk for other stages to populate the result
            if (targetStage.HasValue)
            {
                if (targetStage != 1)
                {
                    result.Stage1Output = await SafeReadFileAsync(Path.Combine(workspaceRoot, "01_skill_extraction", "working", "extracted_requirements.md"));
                }
                if (targetStage != 2)
                {
                    result.Stage2Output = await SafeReadFileAsync(Path.Combine(workspaceRoot, "02_resume_comparison", "working", "comparison_results.md"));
                }
                if (targetStage != 3)
                {
                    result.Stage3Output = await SafeReadFileAsync(Path.Combine(workspaceRoot, "03_interview_preparation", "working", "interview_guide.md"));
                    if (!string.IsNullOrEmpty(result.Stage3Output))
                    {
                        result.FinalAnalysis = ParseFinalAnalysis(result.Stage3Output);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running ICM Pipeline");
            result.FinalAnalysis = new JobMatchResponse
            {
                MatchScore = 0,
                SkillsAligned = new List<string>(),
                Gaps = new List<string> { $"Pipeline Error: {ex.Message}" },
                TalkingPoints = new List<string>()
            };
        }

        return result;
    }

    private async Task<string> CallOpenAiAsync(string prompt, CancellationToken cancellationToken)
    {
        var model = _config["OpenAI:Model"] ?? "gpt-4o-mini";
        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 2000
        };

        var response = await _openAiClient.PostChatCompletionsAsync(requestBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenAI call in ICM runner failed: {StatusCode}, {Body}", response.StatusCode, errorBody);
            throw new InvalidOperationException($"OpenAI API returned error status code: {response.StatusCode}");
        }

        var json = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken);
        return json?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? string.Empty;
    }

    private static async Task<string> SafeReadFileAsync(string path)
    {
        if (!File.Exists(path))
        {
            return string.Empty;
        }
        return await File.ReadAllTextAsync(path);
    }

    private JobMatchResponse ParseFinalAnalysis(string stage3Output)
    {
        try
        {
            // Attempt to extract JSON from code block
            var match = Regex.Match(stage3Output, @"```json\s*(.*?)\s*```", RegexOptions.Singleline);
            var jsonString = match.Success ? match.Groups[1].Value.Trim() : string.Empty;

            if (string.IsNullOrEmpty(jsonString))
            {
                // Fallback: search for braces
                var startIndex = stage3Output.IndexOf('{');
                var endIndex = stage3Output.LastIndexOf('}');
                if (startIndex >= 0 && endIndex > startIndex)
                {
                    jsonString = stage3Output.Substring(startIndex, endIndex - startIndex + 1);
                }
            }

            if (!string.IsNullOrEmpty(jsonString))
            {
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                var score = 0;
                if (root.TryGetProperty("matchScore", out var scoreEl))
                {
                    score = scoreEl.TryGetInt32(out var s) ? s : 0;
                }

                var skills = GetStringArray(root, "skillsAligned");
                var gaps = GetStringArray(root, "gaps");
                var talkingPoints = GetStringArray(root, "talkingPoints");

                return new JobMatchResponse
                {
                    MatchScore = Math.Clamp(score, 0, 100),
                    SkillsAligned = skills,
                    Gaps = gaps,
                    TalkingPoints = talkingPoints
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse final JSON in ICM Stage 3");
        }

        return new JobMatchResponse
        {
            MatchScore = 0,
            SkillsAligned = new List<string>(),
            Gaps = new List<string> { "Failed to parse the structured comparison results." },
            TalkingPoints = new List<string>()
        };
    }

    private static List<string> GetStringArray(JsonElement root, string propertyName)
    {
        var list = new List<string>();
        if (root.TryGetProperty(propertyName, out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in arr.EnumerateArray())
            {
                var s = item.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    list.Add(s.Trim());
                }
            }
        }
        return list;
    }

    private sealed class OpenAIResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private sealed class Choice
    {
        public Message? Message { get; set; }
    }

    private sealed class Message
    {
        public string? Content { get; set; }
    }
}
