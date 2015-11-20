using System;
using System.Diagnostics;
using System.Drawing.Text;
using System.Threading.Tasks;

namespace PdfCreator
{
    internal class Program
    {
        private static readonly Trade Trade = new Trade
        {
            TradeId = Guid.NewGuid(),
            TradeIdDisplay = new Base36Generator().Execute(),
            CreatedAt = DateTime.UtcNow,
            Buyer = new User {Id = "test1", FullName = "John Doe", CompanyName = "Walmart"},
            Seller = new User {Id = "test2", FullName = "Richard Roe", CompanyName = "Best Buy"}
        };

        private static void Main()
        {
            var filepath = GenerateContract();
            Process.Start(filepath);

            // To test file upload to S3:
            //  - Make sure AWS Toolkit for Visual Studio is configured
            //  - Set the bucket name in ContractService
            //  - Uncomment the following line and comment out the two above
            //MainAsync().Wait();

            // Uncomment the following line to see which font families are available on your machine
            //var x = GetFontFamilyList();
        }

        private static async Task MainAsync()
        {
            var filepath = GenerateContract();
            await new ContractService().UploadAsync(new Contract {TradeId = Trade.TradeId, Filepath = filepath});
            Process.Start(filepath);
        }

        private static string GenerateContract()
        {
            var generator = new ContractGenerator();
            var filepath = generator.Generate(Trade);
            generator.AddSignature(filepath, "John Doe", Signee.Buyer);
            generator.AddSignature(filepath, "Richard Roe", Signee.Seller);
            generator.AddSignature(filepath, "Brett Esbaum", Signee.Admin);
            return filepath;
        }

        private static string GetFontFamilyList()
        {
            var familyList = "";
            var fontFamilies = new InstalledFontCollection().Families;
            var count = fontFamilies.Length;
            for (var j = 0; j < count; ++j)
            {
                var familyName = fontFamilies[j].Name;
                familyList = familyList + familyName;
                familyList = familyList + ",  ";
            }
            return familyList;
        }
    }
}