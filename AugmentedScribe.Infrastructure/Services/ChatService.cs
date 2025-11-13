using System.Text;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Domain.Entities;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Memory;
using Polly;
using Polly.Retry;

namespace AugmentedScribe.Infrastructure.Services;

public sealed class ChatService : IChatService
{
    private readonly ILogger<ChatService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _geminiApiKey;
    private readonly string _embeddingModelId;
    private readonly string _chatModelId;
    private readonly string _chromaDbEndpoint;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ChatService(ILogger<ChatService> logger,
        IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;

        _geminiApiKey = configuration["GoogleGemini:ApiKey"] ??
                        throw new InvalidOperationException("GoogleGemini:ApiKey is not configured.");
        _embeddingModelId = configuration["GoogleGemini:EmbeddingModelId"] ??
                            throw new InvalidOperationException("GoogleGemini:EmbeddingModelId is not configured.");
        _chatModelId = configuration["GoogleGemini:ChatModelId"] ??
                       throw new InvalidOperationException("GoogleGemini:ChatModelId is not configured.");

        var chromaDbUrl = configuration["ChromaDb:Url"] ??
                          throw new InvalidOperationException("ChromaDb:Url is not configured.");
        var chromaDbPort = configuration["ChromaDb:Port"] ?? "8000";
        _chromaDbEndpoint = $"{chromaDbUrl}:{chromaDbPort}";

        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<InvalidOperationException>(ex =>
                ex.InnerException is HttpRequestException ||
                ex.Message.Contains("503") ||
                ex.Message.Contains("Service Unavailable") ||
                ex.Message.Contains("429") ||
                ex.Message.Contains("Too Many Requests"))
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount} após {Delay}s devido a: {Error}",
                        retryCount, timeSpan.TotalSeconds, exception.Message);
                });
    }

    public async Task<string> GenerateResponseAsync(
        Campaign campaign,
        string prompt,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting chat generation for CampaignId: {CampaignId}", campaign.Id);

        try
        {
            var kernel = BuildKernel();
#pragma warning disable SKEXP0001
            var memory = await CreateMemoryAsync(kernel);

            var collectionName = $"campaign-{campaign.Id}";

            _logger.LogInformation("Searching ChromaDB collection '{CollectionName}' for relevant context...",
                collectionName);

            var searchResults = await memory.SearchAsync(
                collection: collectionName,
                query: prompt,
                limit: 20,
                minRelevanceScore: 0.5,
                cancellationToken: cancellationToken
            ).ToListAsync(cancellationToken);

            var contextBuilder = new StringBuilder();
            if (searchResults.Count == 0)
            {
                _logger.LogWarning("No context found in ChromaDB for CampaignId: {CampaignId}", campaign.Id);
                contextBuilder.Append("No context available.");
            }
            else
            {
                foreach (var result in searchResults)
                {
                    contextBuilder.AppendLine(result.Metadata.Text);
                }
            }

            var ragPrompt = CreateRagPrompt(campaign, contextBuilder.ToString());
            var chatFunction = kernel.CreateFunctionFromPrompt(ragPrompt);

            _logger.LogInformation("Invoking Gemini chat model for CampaignId: {CampaignId}", campaign.Id);

            var kernelResult = await _retryPolicy.ExecuteAsync(() =>
                kernel.InvokeAsync(
                    chatFunction,
                    new KernelArguments { { "prompt", prompt } },
                    cancellationToken)
            );

            var response = kernelResult.GetValue<string>()
                           ?? "Sorry, I encountered an error generating a response.";

            _logger.LogInformation("Chat response generated for CampaignId: {CampaignId}", campaign.Id);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate chat response for CampaignId: {CampaignId}", campaign.Id);
            throw new InvalidOperationException("An error occurred while processing the chat request.", ex);
        }
    }

    private Kernel BuildKernel()
    {
        _logger.LogInformation("Building Kernel with ChatModelId: {ModelId}", _chatModelId);
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddGoogleAIGeminiChatCompletion(
            modelId: _chatModelId,
            apiKey: _geminiApiKey);

        return kernelBuilder.Build();
    }


    private async Task<ISemanticTextMemory> CreateMemoryAsync(Kernel kernel)
    {
#pragma warning disable SKEXP0020
        var memoryStore = new ChromaMemoryStore(_chromaDbEndpoint, _loggerFactory);
#pragma warning restore SKEXP0020

        var kernelBuilderEmbeddings = Kernel.CreateBuilder();
        kernelBuilderEmbeddings.Services.AddGoogleAIEmbeddingGenerator(
            modelId: _embeddingModelId,
            apiKey: _geminiApiKey);
        var kernelEmbeddings = kernelBuilderEmbeddings.Build();

#pragma warning disable SKEXP0001
        var memory = new SemanticTextMemory(
            memoryStore,
            kernelEmbeddings.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>());
#pragma warning restore SKEXP0001

        return memory;
    }

    private string CreateRagPrompt(Campaign campaign, string context)
    {
        const string promptPlaceholder = "{{$prompt}}";

        return $$"""
                 You are an expert assistant for the RPG system: {{campaign.System}}.
                 Your name is "Augmented Scribe".
                 You must answer initially with your presentation in the first sentence. 
                 You must answer the user's questions based ONLY on the context provided below.
                 If the answer is not found in the context, you MUST state: "I could not find information about that in the uploaded books."
                 Do not use any external knowledge.

                 [CONTEXT]
                 {{context}}
                 [END CONTEXT]

                 User Question:
                 {{promptPlaceholder}}

                 Assistant Answer:
                 """;
    }
}