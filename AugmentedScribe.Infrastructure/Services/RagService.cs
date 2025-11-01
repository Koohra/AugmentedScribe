using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Domain.Entities;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Text;

namespace AugmentedScribe.Infrastructure.Services;

public sealed class RagService : IRagService
{
    private readonly ILogger<RagService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _geminiApiKey;
    private readonly string _embeddingModelId;
    private readonly string _chromaDbEndpoint;

    public RagService(
        ILogger<RagService> logger,
        IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;

        _geminiApiKey = configuration["GoogleGemini:ApiKey"] ??
                        throw new InvalidOperationException(
                            "GoogleGemini:ApiKey is not configured in appsettings.Development.json");

        _embeddingModelId = configuration["GoogleGemini:EmbeddingModelId"] ??
                            throw new InvalidOperationException(
                                "GoogleGemini:EmbeddingModelId is not configured.");

        var chromaDbUrl = configuration["ChromaDb:Url"] ??
                          throw new InvalidOperationException("ChromaDb:Url is not configured.");

        var chromaDbPort = configuration["ChromaDb:Port"] ?? "8000";

        _chromaDbEndpoint = $"{chromaDbUrl}:{chromaDbPort}";

        if (_geminiApiKey.StartsWith("SUA_API_KEY"))
        {
            throw new InvalidOperationException(
                "GoogleGemini:ApiKey is still set to the placeholder value. Please provide a valid API key.");
        }
    }

    public async Task GenerateEmbeddingsAsync(
        Book book,
        string text,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting RAG process for BookId: {BookId}...", book.Id);

        try
        {
            var memory = await CreateMemoryAsync();

#pragma warning disable SKEXP0050
            var lines = TextChunker.SplitPlainTextLines(text, maxTokensPerLine: 128);
            var chunks = TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerParagraph: 1024);
#pragma warning restore SKEXP0050

            _logger.LogInformation(
                "Text chunked into {ChunkCount} parts for BookId: {BookId}.",
                chunks.Count, book.Id);

            var collectionName = $"campaign-{book.CampaignId}";

            for (var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var chunkId = $"book-{book.Id}-chunk-{i}";

                await memory.SaveInformationAsync(
                    collection: collectionName,
                    text: chunk,
                    id: chunkId,
                    description: $"Source BookId: {book.Id}",
                    cancellationToken: cancellationToken);
            }

            _logger.LogInformation(
                "Successfully generated and stored {ChunkCount} embeddings in ChromaDB collection '{CollectionName}' for BookId: {BookId}.",
                chunks.Count, collectionName, book.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embeddings for BookId: {BookId}.", book.Id);
            throw;
        }
    }

#pragma warning disable SKEXP0001
    private async Task<ISemanticTextMemory> CreateMemoryAsync()
#pragma warning restore SKEXP0001
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddGoogleAIEmbeddingGenerator(
            modelId: _embeddingModelId,
            apiKey: _geminiApiKey);

        var kernel = kernelBuilder.Build();

#pragma warning disable SKEXP0020
        var memoryStore = new ChromaMemoryStore(_chromaDbEndpoint, _loggerFactory);
#pragma warning restore SKEXP0020

#pragma warning disable SKEXP0001
        var memory = new SemanticTextMemory(
            memoryStore,
            kernel.GetRequiredService<IEmbeddingGenerator>()
                as IEmbeddingGenerator<string, Embedding<float>>);
#pragma warning restore SKEXP0001

        return memory;
    }
}