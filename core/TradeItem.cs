namespace TestTask.core
{
    public class TradeItem
    {
        public string TradeType { get; set; }
        public long TimeStamp { get; set; }
        public string Pair { get; set; }
        public long DealId { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
        public long BuyOrderId { get; set; }
        public long SellOrderId { get; set; }
        public long TransactionTime { get; set; }
        public bool IsMakerOrTaker { get; set; }
        public bool IsMarketOrLimit { get; set; }

    }
}
