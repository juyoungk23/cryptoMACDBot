using System;
using Skender.Stock.Indicators;

namespace cryptoMACDBot
{
    public class Candle : IQuote
    {
        public Candle(DateTime date, decimal open, decimal high, decimal low)
        {
            Date = date;
            Open = open;
            High = high;
            Low = low;
        }

        public DateTime Date;
        public decimal Open { get; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }

        DateTime IQuote.Date => this.Date;
    }
}
