using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coinbase.Pro.Models;
using Newtonsoft.Json.Linq;

namespace cryptoMACDBot
{
    class Program
    {

        // Todo:
        // 1. Figure out how to update the MACD indicator as the data is streaming live

        private static Client client;
        public static CoinbaseProWebSocket socket;
        private static string productPair = "BTC-USD";
        private static int timeframe = 1;

        static async Task Main(string[] args)
        {

            string apiKey = "07661b27d60a86197091a49404aff506";
            string apiSecret = "uEthX/aoXnXu8y9BP6r9Bj6iMcs6of0K45JIDn3fKi2rGqRNXnkeLQmqjOdFLEU8fqR4CeKGr2Ssff76TPQFog==";
            string passphrase = "sysr3fz9sbb";
            client = new Client(apiKey, apiSecret, passphrase);
            SetupWebSocket(client);

            // before I connect, I need to add all the indicators I want to listen to


            // Connect to websocket
            await Connect();

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
            {
                throw new Exception("Connection error.");
            }

            var sub = new Subscription
            {
                ProductIds =
                  {
                     productPair
                  },
                Channels =
                  {
                     "level2",
                     "heartbeat",
                     JObject.FromObject(
                        new Channel
                           {
                              Name = "ticker_batch",
                              ProductIds = {productPair}
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

                    // "2022-05-21T17:40:36.836840Z"
                    DateTimeOffset date = tick.Time;
                    date = date.ToLocalTime();
                    string formattedTime = $"{date.Month}/{date.Day}/{date.Year} {date.Hour}:{date.Minute}";

                    if (!minutesProcessed.ContainsKey(formattedTime))
                    {

                        minutesProcessed[formattedTime] = true;


                        Candle minuteCandle = new Candle(date.DateTime, tick.Price, tick.Price, tick.Price);
                        minuteCandlesticks.Add(minuteCandle);

                        if (minuteCandle.Date.Minute % timeframe == 0)
                        {
                            processedCandles.Add(minuteCandle);

                            Console.WriteLine("New Candle");
                        }

                    }

                    // if 2 or more candles, 
                    if (minuteCandlesticks.Count >= 2)
                    {
                        minuteCandlesticks[minuteCandlesticks.Count - 2].Close = previousTick.Price;
                        Candle currentCandle = minuteCandlesticks[minuteCandlesticks.Count - 1];

                        if (currentTick.Price > currentCandle.High)
                        {
                            currentCandle.High = currentTick.Price;
                        }

                        if (currentTick.Price < currentCandle.Low)
                        {
                            currentCandle.Low = currentTick.Price;
                        }
                    }
                }
            }
        }

        private static Trade trade = null;
        private static decimal tradeQuantity = 0.005m;
        private static bool in_position = false;
        private static bool in_trade = false;
        private static MACD macd = new MACD(processedCandles);
        private static int lookbackPeriod = 10;
        private static EMA ema = new EMA(processedCandles, lookbackPeriod);

        private static void Check_Signals(object sender, WebSocket4Net.MessageReceivedEventArgs e)
        {

            if (processedCandles.Count >= 2 && processedCandles.Count >= lookbackPeriod && processedCandles.Count >= macd.longPeriod)
            {

                // check stoploss
                client.Check_Stoploss(trade, processedCandles.Last().Close);

                // handle confirming profits

                decimal price = processedCandles.Last().Close;
                double ratio = (double) Math.Abs(((decimal) macd.GetMACDblue()) / processedCandles.Last().Close * 100);

                if (processedCandles.Last().Close > ema.GetLatestEMA()) // only buy
                {
                    if (macd.GetMACDblue() < 0 && macd.GetMACDorange() < 0)
                    {
                        if (macd.GetMACDblue() > macd.GetMACDorange())
                        {
                            if (!in_position && !in_trade)
                            {
                                List<double> targets = client.SetTargets(ratio, true, (double) processedCandles.Last().Close);

                                // place Long order (trade quant)
                                trade = new Trade(tradeQuantity, targets[0], targets[1], targets[2], productPair, CoinbasePro.Services.Orders.Types.OrderSide.Buy, DateTime.Now, processedCandles.Last().Close);
                                client.PlaceLongOrder(trade);
                                in_position = true;
                                in_trade = true;
                                
                                // profits = calculateExpectedProfits();
                                Console.WriteLine("BUY!");
                            }

                            if (!in_position && in_trade)
                            {
                                Console.WriteLine("Tried placing long order, but already in a trade, nothing to do!");
                            }
                        }
                    }
                }

                if (processedCandles.Last().Close < ema.GetLatestEMA()) // only sell
                {
                    if (macd.GetMACDblue() > 0 && macd.GetMACDorange() > 0)
                    {
                        if (macd.GetMACDblue() < macd.GetMACDorange())
                        {
                            if (in_position && !in_trade)
                            {
                                // set targets (ratio, buying=True)
                                // place Long order (trade quant)

                                in_position = false;
                                in_trade = true;
                                trade.stoplossHit = false;
                                trade.targetHit = false;
                                trade.mainTargetHit = false;
                                // profits = calculateExpectedProfits();
                                Console.WriteLine("SELL!");

                            }

                            if (!in_position && in_trade)
                            {
                                Console.WriteLine("Tried placing short order, but already in a trade, nothing to do!");
                            }
                        }
                    }
                }
            }
        }
    }
}
