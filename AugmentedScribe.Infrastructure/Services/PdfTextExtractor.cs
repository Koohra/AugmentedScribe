using System.Text;
using AugmentedScribe.Application.Common.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace AugmentedScribe.Infrastructure.Services;

public sealed class PdfTextExtractorService : IPdfTextExtractor
{
    public async Task<string> ExtractTextAsync(Stream pdfStream)
    {
        return await Task.Run(() =>
        {
            var textBuilder = new StringBuilder();
            using (var reader = new PdfReader(pdfStream))
            {
                using (var doc = new PdfDocument(reader))
                {
                    var numPage = doc.GetNumberOfPages();
                    for (var i = 1; i <= numPage; i++)
                    {
                        var page = doc.GetPage(i);
                        var strategy = new LocationTextExtractionStrategy();
                        var text = PdfTextExtractor.GetTextFromPage(page, strategy);
                        textBuilder.Append(text);
                    }
                }
            }

            return textBuilder.ToString();
        });
    }
}