using System;
namespace cryptoMACDBot
{
    public class Candle
    {
        public Candle(int minute, decimal open, decimal high, decimal low)
        {
            this.minute = minute;
            this.open = open;
            this.high = high;
            this.low = low;
        }

        public int minute { get; }
        public decimal open { get; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal close { get; set; }

        public void setHigh(decimal high)
        {
            this.high = high;
        }

        public void setLow(decimal low)
        {
            this.low = low;
        }

        public void setClose(decimal close)
        {
            this.close = close;
        }
    }
}
