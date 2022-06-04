using NUnit.Framework;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using cryptoMACDBot;

namespace cryptoMACDBotNUnitTests
{
    public class StrategyTests
    {
        private MACD macd;
        private Client client;

        [SetUp]
        public void Setup()
        {
            string apiKey = "07661b27d60a86197091a49404aff506";
            string apiSecret = "uEthX/aoXnXu8y9BP6r9Bj6iMcs6of0K45JIDn3fKi2rGqRNXnkeLQmqjOdFLEU8fqR4CeKGr2Ssff76TPQFog==";
            string passphrase = "sysr3fz9sbb";
            client = new Client(apiKey, apiSecret, passphrase);
            client.SetTradeAmount(0.005m);
            client.SetProductPair("BTC-USD");

            List<Candle> candles = new List<Candle>();
            List<double[]> data = new List<double[]>();
            double[] arr1 = { 30251.25, 30251.25, 30251.25, 30251.25 };
            double[] arr2 = { 30254.8, 30254.8, 30225.53, 30225.53 };
            double[] arr3 = { 30210.02, 30210.02, 30165.89, 30172.8 };
            double[] arr4 = { 30164.49, 30196.57, 30164.49, 30180.17 };
            double[] arr5 = { 30196.47, 30203.43, 30186.59, 30186.8 };
            double[] arr6 = { 30209.1, 30216.26, 30199.18, 30209.85 };
            double[] arr7 = { 30198.79, 30198.79, 30162.12, 30168.7 };
            double[] arr8 = { 30179.62, 30203.52, 30179.62, 30202.08 };
            double[] arr9 = { 30195.85, 30264.5, 30195.85, 30260.69 };
            double[] arr10 = { 30275, 30285.16, 30275, 30282.71 };
            double[] arr11 = { 30258.57, 30258.57, 30258.57, 30255.89 };
            double[] arr12 = { 30275.48, 30275.83, 30237.7, 30248.32 };
            double[] arr13 = { 30245.56, 30259.04, 30227.12, 30259.04 };

            data.Add(arr1); data.Add(arr2); data.Add(arr3); data.Add(arr4); data.Add(arr5);
            data.Add(arr6); data.Add(arr7); data.Add(arr8); data.Add(arr9); data.Add(arr10);
            data.Add(arr11); data.Add(arr12); data.Add(arr13);

            for (int i = 0; i < data.Count; i++)
            {
                Candle candle = new Candle(DateTime.Now, (decimal) data[i][0], (decimal)data[i][1], (decimal)data[i][2]);
                candle.Close = (decimal)data[i][3];
                candles.Add(candle);
            }
            
            macd = new MACD(candles);
        }

        [Test]
        public async Task GetHistoricalData()
        {

            var data = await client.GetHistoricalData(DateTime.Parse("Jan 1, 2020"), DateTime.Now, CoinbasePro.Services.Products.Types.CandleGranularity.Hour24);
            data.Reverse();
            // close on jan 1, 2020: 7174

            Console.WriteLine(data.Count);
            Console.WriteLine(data[0].Close) ;
            Assert.IsTrue(true);
        }

        

    }
}
