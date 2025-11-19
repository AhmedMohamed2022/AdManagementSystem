using iTextSharp.text.pdf;
using iTextSharp.text;
namespace AdManagementSystem.Services
{

    public interface IPdfService
    {
        byte[] GenerateArabicPdf(string title, string bodyText);
    }

    public class PdfService : IPdfService
    {
        private readonly string _fontPath;

        public PdfService(IConfiguration config)
        {
            _fontPath = config["PDF:FontPath"];
        }

        public byte[] GenerateArabicPdf(string title, string bodyText)
        {
            using var ms = new MemoryStream();

            // إنشاء مستند PDF
            var document = new Document(PageSize.A4, 40, 40, 40, 40);
            var writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            // تحميل الخط
            var bf = BaseFont.CreateFont(_fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(bf, 16,Font.NORMAL,BaseColor.BLACK);
            var bold = new Font(bf, 20, Font.BOLD, BaseColor.BLACK);

            // الاتجاه من اليمين لليسار
            var titleParagraph = new Paragraph(title, bold)
            {
                Alignment = Element.ALIGN_RIGHT
            };

            var bodyParagraph = new Paragraph(bodyText, font)
            {
                Alignment = Element.ALIGN_RIGHT
            };

            // إضافة المحتوى
            document.Add(titleParagraph);
            document.Add(new Paragraph("\n"));
            document.Add(bodyParagraph);

            // إنهاء وتصدير
            document.Close();
            return ms.ToArray();
        }
    }

}
