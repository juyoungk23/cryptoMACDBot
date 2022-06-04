using System;
namespace cryptoMACDBot
{
	public class Trade
	{
	
        public Trade(decimal amount, decimal stoploss, decimal target, decimal mainTarget, string productPair, CoinbasePro.Services.Orders.Types.OrderSide orderSide, DateTime date, decimal priceAtTrade)
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

        public bool open = true;

        public bool inPosition = false;

        public DateTime date { get; set; }

		public decimal amount { get; set; }

		public decimal stoploss { get; set; }

		public decimal target { get; set; }

		public decimal mainTarget { get; set; }

        public CoinbasePro.Services.Orders.Types.OrderSide orderSide;

        public string productPair { get; set; }

        public decimal priceAtTrade;

        public bool stoplossHit = false;

        public bool targetHit = false;

        public bool mainTargetHit = false;





        //public int findTradeFactor()
        //{
        //    if (orderSide.Equals(CoinbasePro.Services.Orders.Types.OrderSide.Sell))
        //    {
        //        return -1;
        //    }

        //    return 1;
        //}

    }
}

