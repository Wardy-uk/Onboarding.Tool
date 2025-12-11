using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace Onboarding.Tool.Server.Helpers.Files;

public static class PdfHelper
{
    public static byte[] GenerateLetterheadPdf(byte[] imageBytes)
    {
        using var outputStream = new MemoryStream();

        WriterProperties writerProperties = new WriterProperties().SetCompressionLevel(CompressionConstants.NO_COMPRESSION);
        PdfWriter writer = new PdfWriter(outputStream, writerProperties);
        PdfDocument pdfDocument = new PdfDocument(writer);
        Document document = new Document(pdfDocument);

        Image image = new Image(ImageDataFactory.Create(imageBytes));

        pdfDocument.AddNewPage();
        Rectangle pageSize = pdfDocument.GetFirstPage().GetPageSize();

        float x = pageSize.GetWidth() - image.GetImageWidth() - 50f;
        float y = pageSize.GetHeight() - image.GetImageHeight() - 30f;

        image.SetFixedPosition(x, y);
        document.Add(image);
        document.Close();

        return outputStream.ToArray();
    }
}
