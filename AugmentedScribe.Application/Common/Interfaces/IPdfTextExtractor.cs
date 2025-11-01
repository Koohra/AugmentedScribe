namespace AugmentedScribe.Application.Common.Interfaces;

public interface IPdfTextExtractor
{
    Task<string> ExtractTextAsync(Stream pdfStream);
}