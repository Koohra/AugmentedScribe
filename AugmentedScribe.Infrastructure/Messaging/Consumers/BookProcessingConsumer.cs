using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Contracts;
using AugmentedScribe.Domain.Enums;
using Azure.Storage.Blobs;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AugmentedScribe.Infrastructure.Messaging.Consumers;

public sealed class BookProcessingConsumer(
    ILogger<BookProcessingConsumer> logger,
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    IPdfTextExtractor pdfTextExtractor) : IConsumer<BookUploadedEvent>
{
    private readonly ILogger<BookProcessingConsumer>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    private readonly IFileStorageService _fileStorageService =
        fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));

    private readonly IPdfTextExtractor _pdfTextExtractor =
        pdfTextExtractor ?? throw new ArgumentNullException(nameof(pdfTextExtractor));

    public async Task Consume(ConsumeContext<BookUploadedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received Book Uploaded Event for BookId: {BookId}", message.BookId);

        var book = await _unitOfWork.Books.GetBookByIdAsync(message.BookId, context.CancellationToken);
        if (book is null)
        {
            _logger.LogWarning("Book with Id {BookId} not found. Message will be discarded.", message.BookId);
            return;
        }

        if (book.Status != BookStatus.Pending)
        {
            _logger.LogWarning(
                "Book {BookId} is not in Pending status (Current: {Status}). Reprocessing will be skipped.",
                book.Id, book.Status);
            return;
        }

        try
        {
            book.StartProcessing();
            await _unitOfWork.CompleteAsync(context.CancellationToken);
            _logger.LogInformation("Book {BookId} status updated to Processing.", book.Id);

            var blobClient = new BlobClient(new Uri(message.StorageUrl));
            var blobName = blobClient.Name;
            await using var stream = await _fileStorageService.DownloadFileAsync(blobName, context.CancellationToken);
            _logger.LogInformation("PDF for Book {BookId} downloaded.", book.Id);

            var text = await _pdfTextExtractor.ExtractTextAsync(stream);
            _logger.LogInformation("Text extracted from PDF for Book {BookId}. Length: {Length}", book.Id, text.Length);

            //Pipeline RAG...

            book.MarkAsCompleted();
            _logger.LogInformation("Book {BookId} status updated to Completed.", book.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process book {BookId}", book.Id);
            book.MarkAsFailed();
        }
        finally
        {
            await _unitOfWork.CompleteAsync(context.CancellationToken);
        }
    }
}