using System;
using PositionService.Shared;

namespace PositionService.Dtos
{
    public class PositionDto
    {
        public DateTime TradeDate { get; set; }

        public Side Side { get; set; } 

        public string Symbol { get; set; }

        public long Quantity { get; set; }

        public decimal Price { get; set; }
    }
}
