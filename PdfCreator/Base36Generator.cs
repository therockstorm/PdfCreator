using System;
using System.Text;

namespace PdfCreator
{
    public class Base36Generator
    {
        public const int Length = 6;
        private static readonly Random Random = new Random();
        private static readonly char[] Base36Chars = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public string Execute()
        {
            var sb = new StringBuilder(Length);
            for (var i = 0; i < Length; i++)
                sb.Append(Base36Chars[Random.Next(36)]);
            return sb.ToString();
        }
    }
}