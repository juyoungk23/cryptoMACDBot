using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coinbase.Pro.Models;
using Skender.Stock.Indicators;
using WebSocket4Net;

namespace cryptoMACDBot
{
    public class MACD
    {
        public MACD(CandleMaker candleMaker)
        {
            this.candleMaker = candleMaker;
            processedCandles = candleMaker.processedCandles;
            candleMaker.addEventHandler(randomMethod);
        }

        private void randomMethod(object sender, WebSocket4Net.MessageReceivedEventArgs e)
        {


            if (WebSocketHelper.TryParse(e.Message, out var msg))
            {
                if (msg is TickerEvent tick)
                {
                }

            }
        }



        public void getMACD()
        {
            Console.WriteLine("I am here");
            processedCandles.GetMacd(12, 26);
        }


        public List<decimal> MACDblue;
        public List<decimal> MACDorange;


        public CandleMaker candleMaker;
        public IEnumerable<Candle> processedCandles { get; set; }
        
    }
}

