using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coinbase.Pro.Models;
using Skender.Stock.Indicators;
using Newtonsoft.Json.Linq;

namespace cryptoMACDBot
{
    class Program
    {

        private static Client client;
        public static CoinbaseProWebSocket socket;
        private static int timeframe = 1;
        private static Trade trade = new Trade(0, 0, decimal.MaxValue, decimal.MaxValue, "", CoinbasePro.Services.Orders.Types.OrderSide.Buy, DateTime.Now, 0);
        private static bool in_position = false;
        private static bool in_trade = false;
        public static MACD macd = new MACD(processedCandles);
        private static int lookbackPeriod = 200;
        public static EMA ema = new EMA(processedCandles, lookbackPeriod);

        static async Task Main(string[] args)
        {

            string apiKey = "07661b27d60a86197091a49404aff506";
            string apiSecret = "uEthX/aoXnXu8y9BP6r9Bj6iMcs6of0K45JIDn3fKi2rGqRNXnkeLQmqjOdFLEU8fqR4CeKGr2Ssff76TPQFog==";
            string passphrase = "sysr3fz9sbb";
            client = new Client(apiKey, apiSecret, passphrase);
            client.SetTradeAmount(0.005m);
            client.SetProductPair("ENJ-USD");
            trade.open = false;

            // Live Deployment

            // SetupWebSocket(client);
            // await Connect();



            // Backtesting Code

            CoinbasePro.Services.Products.Types.CandleGranularity granularity = CoinbasePro.Services.Products.Types.CandleGranularity.Hour1;
            List<Candle> allTheCandles = await client.GetHistoricalData(DateTime.Parse("Jan 1, 2022"), DateTime.Now, granularity);
            //allTheCandles.Reverse();
            Console.WriteLine(allTheCandles.Count);

            foreach (var candle in allTheCandles)
            {
                processedCandles.Add(candle);
                Backtest(candle);

            }

            Console.WriteLine(client.GetWinLossPercentage());

        }

        public static void SetupWebSocket(Client client)
        {
            socket = new CoinbaseProWebSocket(new WebSocketConfig
            {
                UseTimeApi = true,
                ApiKey = client.apiKey,
                Secret = client.apiSecret,
                Passphrase = client.passphrase,
                SocketUri = "wss://ws-feed.exchange.coinbase.com"
            });
        }

        public static async Task Connect()
        {
            var result = await socket.ConnectAsync();
            if (!result.Success)
            { throw new Exception("Connection error."); }

            var sub = new Subscription
            {
                ProductIds =
                  {
                     client.productPair
                  },
                Channels =
                  {
                     "level2",
                     "heartbeat",
                     JObject.FromObject(
                        new Channel
                           {
                              Name = "ticker_batch",
                              ProductIds = {client.productPair}
                           })
                  }
            };

            await socket.SubscribeAsync(sub);

            socket.RawSocket.MessageReceived += Make_Candles;
            socket.RawSocket.MessageReceived += Check_Signals;

            while (socket.RawSocket.LastActiveTime.Second - DateTime.Now.Second < 30)
            {
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }

        private static void Check_Signals(object sender, WebSocket4Net.MessageReceivedEventArgs e)
        {

            if (processedCandles.Count >= 2 && processedCandles.Count >= lookbackPeriod && processedCandles.Count > macd.slowPeriods)
            {

                // check stoploss

                Candle candle = processedCandles.Last();
                decimal price = candle.Close;

                if (trade != null)
                {
                    client.CheckStoploss(trade, price);
                    client.CheckProfits(trade, price);
                }

                // update indicators
                macd.candles = processedCandles;
                ema.candles = processedCandles;

                double ratio = (double)Math.Abs((decimal) macd.GetMACDblue() / price * 100);

                if (price > ema.GetLatestEMA()) // only buy
                {
                    if (macd.GetMACDblue() < 0 && macd.GetMACDorange() < 0)
                    {
                        if (macd.GetMACDblue() > macd.GetMACDorange())
                        {
                            if (!in_position && !in_trade)
                            {
                                List<decimal> targets = client.SetTargets(ratio, true, price);
                                trade = new Trade(client.tradeQuantity, stoploss: targets[0],
                                    target: targets[1], mainTarget: targets[2], client.productPair,
                                    orderSide: CoinbasePro.Services.Orders.Types.OrderSide.Buy,
                                    DateTime.Now, priceAtTrade: price);
                                // client.PlaceLongOrder(trade);
                                in_position = true;
                                in_trade = true;

                                // profits = calculateExpectedProfits();
                                Console.WriteLine("BUY!");
                            }
                        }
                    }
                }

                if (price < ema.GetLatestEMA()) // only sell
                {
                    if (macd.GetMACDblue() > 0 && macd.GetMACDorange() > 0)
                    {
                        if (macd.GetMACDblue() < macd.GetMACDorange())
                        {
                            if (in_position && !in_trade)
                            {
                                List<decimal> targets = client.SetTargets(ratio, false, price);
                                trade = new Trade(client.tradeQuantity, stoploss: targets[0],
                                    target: targets[1], mainTarget: targets[2], client.productPair,
                                    orderSide: CoinbasePro.Services.Orders.Types.OrderSide.Sell,
                                    DateTime.Now, priceAtTrade: price);
                                // client.PlaceShortOrder(trade);
                                in_position = true;
                                in_trade = true;

                                // profits = calculateExpectedProfits();
                                Console.WriteLine("SELL!");
                            }
                        }
                    }
                }
            }
        }

        // Backtest
        private static void Backtest(Candle candle)
        {
            if (processedCandles.Count >= 2 && processedCandles.Count >= lookbackPeriod && processedCandles.Count > macd.slowPeriods)
            {
                decimal price = candle.Close;

                if (trade.open)
                {
                    client.CheckStoploss(trade, price);
                    client.CheckProfits(trade, price);
                }

                // update indicators
                macd.Update(processedCandles);
                ema.Update(processedCandles);


                double ratio = (double)Math.Abs( (decimal) macd.GetMACDblue() / price * 100);

                
                if (price > ema.GetLatestEMA()) // only buy
                {

                    if (macd.GetMACDblue() < 0 && macd.GetMACDorange() < 0)
                    {
                        
                        if (macd.GetMACDblue() > macd.GetMACDorange())
                        {
                            //Console.WriteLine($"going up, count: {processedCandles.Count}");
                            if (!trade.inPosition && !trade.open)
                            {

                                List<decimal> targets = client.SetTargets(ratio, true, price);
                                trade = new Trade(client.tradeQuantity, stoploss: targets[0],
                                    target: targets[1], mainTarget: targets[2], client.productPair,
                                    orderSide: CoinbasePro.Services.Orders.Types.OrderSide.Buy,
                                    DateTime.Now, priceAtTrade: price);
                                // client.PlaceLongOrder(trade);
                                trade.open = true;
                                trade.inPosition = true;

                                // Console.WriteLine($"BUY! price: {price}, count: {processedCandles.Count}, stoploss: {targets[0]}, target: {targets[1]}, main: {targets[2]}");

                            }
                        }
                    }
                }

                if (price < ema.GetLatestEMA()) // only sell
                {
                    if (macd.GetMACDblue() > 0 && macd.GetMACDorange() > 0)
                    {
                        if (macd.GetMACDblue() < macd.GetMACDorange())
                        {
                            // Console.WriteLine($"going down, count: {processedCandles.Count}");
                            if (trade.inPosition && !trade.open)
                            {
                                List<decimal> targets = client.SetTargets(ratio, false, price);
                                trade = new Trade(client.tradeQuantity, stoploss: targets[0],
                                    target: targets[1], mainTarget: targets[2], client.productPair,
                                    orderSide: CoinbasePro.Services.Orders.Types.OrderSide.Sell,
                                    DateTime.Now, priceAtTrade: price);
                                // client.PlaceShortOrder(trade);
                                trade.open = true;
                                trade.inPosition = false;

                                // Console.WriteLine($"SELL! price: {price}, count: {processedCandles.Count}, stoploss: {targets[0]}, target: {targets[1]}, main: {targets[2]}");
                            }
                        }
                    }
                }
            }
        }
        // Candle Maker

        private static Dictionary<string, bool> minutesProcessed = new Dictionary<string, bool>();
        private static List<Candle> minuteCandlesticks = new List<Candle>();
        private static List<Candle> processedCandles = new List<Candle>();
        private static TickerEvent currentTick;
        private static TickerEvent previousTick;

        private static void Make_Candles(object sender, WebSocket4Net.MessageReceivedEventArgs e)
        {

            if (WebSocketHelper.TryParse(e.Message, out var msg))
            {
                if (msg is TickerEvent tick)
                {
                    previousTick = currentTick;
                    currentTick = tick;
                    DateTimeOffset date = tick.Time;
                    date = date.ToLocalTime();
                    string formattedTime = $"{date.Month}/{date.Day}/{date.Year} {date.Hour}:{date.Minute}";

                    // Create New Candle
                    if (!minutesProcessed.ContainsKey(formattedTime))
                    {
                        minutesProcessed[formattedTime] = true;
                        Candle minuteCandle = new Candle(date.DateTime, tick.Price, tick.Price, tick.Price);
                        minuteCandlesticks.Add(minuteCandle);


                        // New Candle
                        if (minuteCandlesticks.Count >= 2)
                        {

                            Candle finishedCandle = minuteCandlesticks[minuteCandlesticks.Count - 2];
                            finishedCandle.Close = previousTick.Price;

                            if (finishedCandle.Date.Minute % timeframe == 0)
                            {
                                processedCandles.Add(finishedCandle);
                                Console.WriteLine($"New Candle - {finishedCandle.Open}, {finishedCandle.High}, {finishedCandle.Low}, {finishedCandle.Close}");

                            }
                        }
                    }

                    // if 2 or more candles, 
                    if (minuteCandlesticks.Count >= 2)
                    {
                        // Currently developing Candle
                        Candle currentCandle = minuteCandlesticks[minuteCandlesticks.Count - 1];

                        if (currentTick.Price > currentCandle.High)
                        { currentCandle.High = currentTick.Price; }

                        if (currentTick.Price < currentCandle.Low)
                        { currentCandle.Low = currentTick.Price; }
                    }
                }
            }
        }
    }
}
