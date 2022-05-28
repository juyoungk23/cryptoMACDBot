using System;
namespace cryptoMACDBot
{
	public class Trade
	{
	
        public Trade(decimal amount, double stoploss, double target, double mainTarget, string productPair, CoinbasePro.Services.Orders.Types.OrderSide orderSide, DateTime date, decimal priceAtTrade)
        {
            this.date = date;
            this.amount = amount;
            this.stoploss = stoploss;
            this.target = target;
            this.mainTarget = mainTarget;
            this.productPair = productPair;
            this.orderSide = orderSide;
            this.priceAtTrade = priceAtTrade;
        }

        public DateTime date { get; set; }

		public decimal amount { get; set; }

		public double stoploss { get; set; }

		public double target { get; set; }

		public double mainTarget { get; set; }

		public bool open = true;

        public CoinbasePro.Services.Orders.Types.OrderSide orderSide;

        public string productPair { get; set; }

        public bool stoplossHit = false;

        public bool targetHit = false;

        public bool mainTargetHit = false;

        public decimal priceAtTrade;

    }
}

