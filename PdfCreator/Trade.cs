using System;

namespace PdfCreator
{
    public class Trade
    {
        public Guid TradeId { get; set; }
        public string TradeIdDisplay { get; set; }
        public DateTime CreatedAt { get; set; }
        public User Buyer { get; set; }
        public User Seller { get; set; }
    }
}