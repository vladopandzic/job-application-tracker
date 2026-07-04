namespace Procoding.ApplicationTracker.Application.Core.Abstractions.AiExtraction;

/// <summary>
/// Extracts structured job-application fields from free-form job posting text using an AI model.
/// Implemented in Infrastructure (Gemini), consumed by a command handler. Provider-agnostic — any
/// LLM with structured output can back it. Returns null when unconfigured or extraction fails.
/// </summary>
public interface IJobPostingExtractor
{
    Task<ExtractedJobPosting?> ExtractAsync(string content, CancellationToken cancellationToken = default);
}
