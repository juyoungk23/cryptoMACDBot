using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coinbase.Pro.Models;
using Coinbase.Pro.WebSockets;
using Newtonsoft.Json.Linq;

namespace cryptoMACDBot
{
    public class CandleMaker
    {

        private CoinbaseProWebSocket socket;

        private Dictionary<string, bool> minutesProcessed = new Dictionary<string, bool>(); 
        private List<Candle> minuteCandlesticks = new List<Candle>();
        private TickerEvent currentTick;
        private TickerEvent previousTick;

        public CandleMaker(Client client)
        {
            socket = new CoinbaseProWebSocket(new WebSocketConfig
            {
                UseTimeApi = true,
                ApiKey = client.apiKey,
                Secret = client.apiSecret,
                Passphrase = client.passphrase,
                SocketUri = "wss://ws-feed.pro.coinbase.com"
            }) ;

        }

        public async void CollectData()
        {
            await Connect();
        }


        public async Task Connect()
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
                     "BTC-USD"
                  },
                Channels =
                  {
                     "level2",
                     "heartbeat",
                     JObject.FromObject(
                        new Channel
                           {
                              Name = "ticker",
                              ProductIds = {"BTC-USD"}
                           })
                  }
            };

            await socket.SubscribeAsync(sub);

            socket.RawSocket.MessageReceived += RawSocket_MessageReceived;

            await Task.Delay(TimeSpan.FromMinutes(1));

        }

        private void RawSocket_MessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
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


                    // Console.WriteLine($"{tick.ProductId} is {tick.Price} at {formattedTime}");

                    if (!minutesProcessed.ContainsKey(formattedTime))
                    {
                        minutesProcessed[formattedTime] = true;

                        Candle minuteCandle = new Candle(date.Minute, tick.Price, tick.Price, tick.Price);
                        minuteCandlesticks.Add(minuteCandle);

                    }

                    if (minuteCandlesticks.Count > 1)
                    {
                        minuteCandlesticks[minuteCandlesticks.Count - 1].close = previousTick.Price;

                        Candle currentCandle = minuteCandlesticks[minuteCandlesticks.Count - 1];

                        if (currentTick.Price > currentCandle.high)
                        {
                            currentCandle.high = currentTick.Price;
                        }

                        if (currentTick.Price < currentCandle.low)
                        {
                            currentCandle.low = currentTick.Price;
                        }
                        Console.WriteLine($"Candle Open: {currentCandle.open}, High: {currentCandle.high}, Low: {currentCandle.low}, Close: {currentCandle.close}");
                    }
                }
            }
        }
    }
}
