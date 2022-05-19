using System;
using System.Collections.Generic;
using CoinbasePro.WebSocket;
using CoinbasePro.WebSocket.Models.Response;
using CoinbasePro.WebSocket.Types;

namespace cryptoMACDBot
{
    public class WebSocket
    {
        public WebSocket(Client client)
        {
            this.client = client;

            //use the websocket feed
            var productTypes = new List<string>() { "BTC-USD" };
            var channels = new List<ChannelType>() { ChannelType.Heartbeat, ChannelType.Ticker, ChannelType.User }; // When not providing any channels, the socket will subscribe to all channels

            this.webSocket = client.client.WebSocket;
            webSocket.Start(productTypes, channels, 1);
            webSocket.OnHeartbeatReceived += WebSocket_OnHeartbeatReceived;
            webSocket.OnOpenReceived += WebSocket_OnOpenReceived;
            webSocket.OnTickerReceived += WebSocket_OnTickerReceived;
        }

        private static void WebSocket_OnHeartbeatReceived(object sender, WebfeedEventArgs<Heartbeat> e)
        {
            throw new NotImplementedException();
        }

        private static void WebSocket_OnOpenReceived(object sender, WebfeedEventArgs<Open> e)
        {
            throw new NotImplementedException();
        }

        private static void WebSocket_OnTickerReceived(object sender, WebfeedEventArgs<Ticker> e)
        {
            Console.WriteLine(e.LastOrder.Open24H);
        }



        public IWebSocket webSocket { get; }

        public Client client { get;}
     
    }
}
