using NUnit.Framework;
using Coinbase.Pro.WebSockets;
using Coinbase.Pro.Models;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace cryptoMACDBotNUnitTests
{
    public class WebSocketTests
    {

        private CoinbaseProWebSocket socket;
        private string ApiKey = "07661b27d60a86197091a49404aff506";
        private string ApiSecret = "uEthX/aoXnXu8y9BP6r9Bj6iMcs6of0K45JIDn3fKi2rGqRNXnkeLQmqjOdFLEU8fqR4CeKGr2Ssff76TPQFog==";
        private string Passphrase = "sysr3fz9sbb";

        [SetUp]
        public void Setup()
        {
            socket = new CoinbaseProWebSocket(new WebSocketConfig
            {
                UseTimeApi = true,
                ApiKey = this.ApiKey,
                Secret = this.ApiSecret,
                Passphrase = this.Passphrase,
                SocketUri = "wss://ws-feed.pro.coinbase.com"
            });
        }

        [Test]
        public async Task connect()
        {
            var result = await socket.ConnectAsync();
            if (!result.Success)
            {
                throw new Exception("Connection error.");
            }

            // https://docs.pro.coinbase.com/?r=1#protocol-overview
            // Request
            // Subscribe to ETH-USD and ETH-EUR with the level2, heartbeat and ticker channels,
            // plus receive the ticker entries for ETH-BTC and ETH-USD
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
            // Console.WriteLine($"{e.Message[1]}: {e.Message[2].ToString()}");
            // Debug.WriteLine(e.Message);

            if (WebSocketHelper.TryParse(e.Message, out var msg))
            {
                if (msg is TickerEvent tick)
                {
                    Console.WriteLine($"{tick.ProductId} is {tick.Price} at {tick.Time}");
                    DateTimeOffset date = tick.Time;


                    // "2022-05-21T17:40:36.836840Z"
                }



            }
        }

        [Test]
        public async Task connect_simple()
        {
            var result = await socket.ConnectAsync();
            if (!result.Success)
            {
                throw new Exception("Connect error.");
            }

            var sub = new Subscription
            {
                ProductIds =
                  {
                     "BTC-USD",
                  },
                Channels =
                  {
                     "heartbeat",
                  }
            };

            await socket.SubscribeAsync(sub);

            socket.RawSocket.MessageReceived += RawSocket_MessageReceived;

            await Task.Delay(TimeSpan.FromMinutes(1));

        }

        [Test]
        public async Task subscribe_with_error()
        {
            var result = await socket.ConnectAsync();
            if (!result.Success)
            {
                throw new Exception("Connect error.");
            }

            socket.RawSocket.MessageReceived += RawSocket_MessageReceived;

            var sub = new Subscription
            {
                ProductIds =
                  {
                     "ZZZ-YYY",
                  },
                Channels =
                  {
                     "heartbeat",
                  }
            };

            await socket.SubscribeAsync(sub);

            await Task.Delay(TimeSpan.FromMinutes(1));
        }

    }
}
