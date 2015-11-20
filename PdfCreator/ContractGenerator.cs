using System;
using System.Collections.Generic;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfCreator
{
    public class ContractGenerator
    {
        private const int Margin = 50;
        private const int SectionPadding = 30;
        private const string DateFormat = "yyyy-MM-dd";
        private const string FontFamily = "Arial";
        private const PageSize PageSize = PdfSharp.PageSize.Letter;
        private static readonly XSolidBrush TextBrush = XBrushes.Black;
        private static readonly double PageWidth = PageSizeConverter.ToSize(PageSize).Width;
        private static readonly double Width = PageWidth - Margin*2;
        private static readonly XImage Logo = XImage.FromFile("logo.png");
        private static readonly XRect LogoRect = new XRect(PageWidth/2 - Logo.PointWidth/2, Margin, Logo.PointWidth, Logo.PointHeight);
        private static readonly XFont Font = new XFont(FontFamily, 11, XFontStyle.Regular);
        private static readonly double BodyRectHeight = Font.GetHeight()*7 + SectionPadding;
        private static readonly XRect AddressRect = new XRect(Margin, LogoRect.Bottom + 5, Width, Font.GetHeight() + SectionPadding);
        private static readonly XRect BodyRectLeft = new XRect(Margin, AddressRect.Bottom, Margin + 45, BodyRectHeight);
        private static readonly XRect BodyRectRight = new XRect(BodyRectLeft.Right, AddressRect.Bottom, PageWidth - BodyRectLeft.Right /*Don't omit long fields*/, BodyRectHeight);
        private static readonly XFont ConditionsFont = new XFont(FontFamily, 8, XFontStyle.Regular);
        private static readonly XRect ConditionsRect = new XRect(Margin, BodyRectLeft.Bottom, Width, ConditionsFont.GetHeight()*2 + SectionPadding);
        private static readonly XRect MoreConditionsRect = new XRect(Margin, ConditionsRect.Bottom, Width, ConditionsFont.GetHeight()*2 + SectionPadding);
        private static readonly XFont SignatureFieldFont = new XFont(FontFamily, 11, XFontStyle.Bold);
        private static readonly XFont SignatureFont = new XFont("Segoe Script", 16, XFontStyle.Regular);
        private static readonly double SignatureFieldHeight = SignatureFieldFont.GetHeight();
        private static readonly double BuyerSignatureY = MoreConditionsRect.Bottom + SectionPadding;
        private static readonly double SellerSignatureY = BuyerSignatureY + SignatureFieldHeight*2 + SignatureFont.GetHeight();
        private static readonly double AdminSignatureY = SellerSignatureY + SignatureFieldHeight*2 + SignatureFont.GetHeight();
        private static readonly Dictionary<Signee, double> SignatureY = new Dictionary<Signee, double>
        {
            {Signee.Buyer, BuyerSignatureY},
            {Signee.Seller, SellerSignatureY},
            {Signee.Admin, AdminSignatureY}
        };

        public string Generate(Trade trade)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PageSize;
            var gfx = XGraphics.FromPdfPage(page);

            GenerateHeader(gfx);
            GenerateBody(gfx, trade);
            GenerateConditions(gfx);
            GenerateSignatureFields(gfx, trade);
            GenerateReferenceId(gfx, trade);

            var filename = @"test_" + trade.CreatedAt.ToString(DateFormat) + ".pdf";
            document.Save(filename);
            return filename;
        }

        public void AddSignature(string filename, string signature, Signee signee)
        {
            var font = SignatureFont;
            var document = PdfReader.Open(filename, PdfDocumentOpenMode.Modify);
            var gfx = XGraphics.FromPdfPage(document.Pages[0]);
            var rect = new XRect(Margin, SignatureY[signee] - SectionPadding, PageWidth - Margin /*Don't omit long fields*/, font.GetHeight());
            CheckWidth(gfx, font, rect, signature);

            CreateTextFormatter(gfx).DrawString(signature, font, TextBrush, rect, XStringFormats.TopLeft);
            document.Save(filename);
        }

        private static void GenerateHeader(XGraphics gfx)
        {
            gfx.DrawImage(Logo, LogoRect.Left, LogoRect.Top);

            var tf = CreateTextFormatter(gfx, XParagraphAlignment.Center);
            tf.DrawString("123 Main Street · Chicago, IL 60611\nsupport@example.com · (555) 555-5555",
                Font, TextBrush, AddressRect, XStringFormats.TopLeft);
        }

        private static void GenerateBody(XGraphics gfx, Trade trade)
        {
            var tf = CreateTextFormatter(gfx);
            tf.DrawString(@"Contract ID:
Date:

Buyer:
Seller:

Price:
Notes:", Font, TextBrush, BodyRectLeft, XStringFormats.TopLeft);

            var font = Font;
            var rect = BodyRectRight;
            CheckWidth(gfx, font, rect, trade.Buyer.CompanyName + "/" + trade.Buyer.FullName);
            CheckWidth(gfx, font, rect, trade.Seller.CompanyName + "/" + trade.Seller.FullName);

            tf.DrawString(string.Format("{0}\n{1}\n\n{2}\n{3}\n\n{4}\n{5}",
                trade.TradeIdDisplay,
                trade.CreatedAt.ToString(DateFormat),
                trade.Buyer.CompanyName + "/" + trade.Buyer.FullName,
                trade.Seller.CompanyName + "/" + trade.Seller.FullName,
                "$150",
                "Candy tastes good."), font, TextBrush, rect, XStringFormats.TopLeft);
        }

        private static void CheckWidth(XGraphics gfx, XFont font, XRect rect, string text)
        {
            if (gfx.MeasureString(text, font).Width > rect.Width)
                throw new Exception("Uh oh.");
        }

        private static void GenerateConditions(XGraphics gfx)
        {
            var tf = CreateTextFormatter(gfx);
            tf.DrawString(@"All conditions apply.", ConditionsFont, TextBrush, ConditionsRect, XStringFormats.TopLeft);
            tf.DrawString("So do these.", ConditionsFont, TextBrush, MoreConditionsRect, XStringFormats.TopLeft);
        }

        private static void GenerateSignatureFields(XGraphics gfx, Trade trade)
        {
            var color = XPens.Black;
            const int x1 = Margin;
            var x2 = x1 + Width;
            var tf = CreateTextFormatter(gfx);

            AddSignatureField(gfx, tf, color, trade.Buyer.CompanyName, x1, x2, SignatureY[Signee.Buyer]);
            AddSignatureField(gfx, tf, color, trade.Seller.CompanyName, x1, x2, SignatureY[Signee.Seller]);
            AddSignatureField(gfx, tf, color, "Acme, LLC", x1, x2, SignatureY[Signee.Admin]);
        }

        private static void AddSignatureField(XGraphics gfx, XTextFormatter tf, XPen color, string companyName, int x1, double x2, double y)
        {
            gfx.DrawLine(color, x1, y, x2, y);
            tf.DrawString(companyName, SignatureFieldFont, TextBrush, new XRect(x1, y, x2, SignatureFieldHeight), XStringFormats.TopLeft);
        }

        private static void GenerateReferenceId(XGraphics gfx, Trade trade)
        {
            var text = "Reference ID: " + trade.TradeId.ToString().ToUpper();
            var tf = CreateTextFormatter(gfx);
            tf.DrawString(
                text,
                ConditionsFont,
                TextBrush,
                new XRect(PageWidth - Margin - gfx.MeasureString(text, ConditionsFont).Width, PageSizeConverter.ToSize(PageSize).Height - Margin - ConditionsFont.GetHeight(), Width, ConditionsFont.GetHeight()),
                XStringFormats.TopLeft);
        }

        private static XTextFormatter CreateTextFormatter(XGraphics gfx, XParagraphAlignment alignment = XParagraphAlignment.Left)
        {
            return new XTextFormatter(gfx) {Alignment = alignment};
        }
    }
}