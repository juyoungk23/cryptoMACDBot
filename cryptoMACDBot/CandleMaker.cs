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

        public CoinbaseProWebSocket socket;
        private Dictionary<string, bool> minutesProcessed = new Dictionary<string, bool>(); 
        public List<Candle> minuteCandlesticks = new List<Candle>();
        public List<Candle> processedCandles = new List<Candle>();
        private TickerEvent currentTick;
        private TickerEvent previousTick;
        private int timeframe;

        public CandleMaker(Client client, int timeframe)
        {
            socket = new CoinbaseProWebSocket(new WebSocketConfig
            {
                UseTimeApi = true,
                ApiKey = client.apiKey,
                Secret = client.apiSecret,
                Passphrase = client.passphrase,
                SocketUri = "wss://ws-feed.exchange.coinbase.com"
            }) ;

            this.timeframe = timeframe;

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
                              Name = "ticker_batch",
                              ProductIds = {"BTC-USD"}
                           })
                  }
            };

            await socket.SubscribeAsync(sub);

            socket.RawSocket.MessageReceived += RawSocket_MessageReceived;
       
            socket.RawSocket.EnableAutoSendPing = true;
            socket.RawSocket.AutoSendPingInterval = 1;
            
            while (socket.RawSocket.LastActiveTime.Second - DateTime.Now.Second < 30)
            {
                await Task.Delay(TimeSpan.FromSeconds(30));
            }

        }

        public void addEventHandler(EventHandler<WebSocket4Net.MessageReceivedEventArgs> method)
        {
            object sender = this.socket;
            socket.RawSocket.MessageReceived += method;

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
    }
}
