namespace PositionService.Resources
{
    public class Position
    {

        public string Symbol { get; set; }

        public long Quantity { get; set; }

        public string Name { get; set; }

        public decimal CurrentPrice { get; set; }

        public decimal UnrealizedPnl { get; set; }

        public decimal RealizedPnl { get; set; }

        public decimal TotalPnl { get; set; }

    }
    
}
