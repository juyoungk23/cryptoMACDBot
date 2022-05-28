using NUnit.Framework;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using cryptoMACDBot;

namespace cryptoMACDBotNUnitTests
{
    public class MACDTests
    {

        private MACD macd;

        [SetUp]
        public void Setup()
        {
            List<Candle> candles = new List<Candle>();
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));
            candles.Add(new Candle(DateTime.Now, 30000m, 32000m, 29000m));

            this.macd = new MACD(candles);

            for (int i = 26; i < candles.Count; i++)
            {
                Console.WriteLine(macd.GetMACDblue());
            };

        }


        [Test]
        public void GetLatestMACD()
        {
            double val = macd.GetMACDblue();
            Console.WriteLine(val);
            Assert.IsInstanceOf(typeof(double), val);
 
        }

        [Test]
        public async Task subscribe_with_error()
        {

        }

    }
}
