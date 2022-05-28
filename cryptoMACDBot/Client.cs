using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CoinbasePro.Network.Authentication;

namespace cryptoMACDBot
{
    public class Client
    {
        public Client(string apiKey, string apiSecret, string passphrase)
        {
            this.apiKey = apiKey;
            this.apiSecret = apiSecret;
            this.passphrase = passphrase;

            var authenticator = new Authenticator(apiKey, apiSecret, passphrase);
            var coinbaseProClient = new CoinbasePro.CoinbaseProClient(authenticator);
            this.client = coinbaseProClient;
        }

        public CoinbasePro.CoinbaseProClient client { get; }

        public string apiKey { get; }

        public string apiSecret { get; }

        public string passphrase { get; }

        public async Task<List<CoinbasePro.Services.CoinbaseAccounts.Models.CoinbaseAccount>> GetAllAccounts()
        {
            var allAccounts = await client.CoinbaseAccountsService.GetAllAccountsAsync();

            return (List<CoinbasePro.Services.CoinbaseAccounts.Models.CoinbaseAccount>)allAccounts;
        }

        public async void PlaceLongOrder(Trade trade)
        {
            await client.OrdersService.PlaceMarketOrderAsync(trade.orderSide, trade.productPair, trade.amount);
            Console.WriteLine($"BOUGHT {trade.amount} of {trade.productPair}");
        }

        public async void PlaceShortOrder(Trade trade)
        {
            await client.OrdersService.PlaceMarketOrderAsync(trade.orderSide, trade.productPair, trade.amount);
            Console.WriteLine($"SOLD {trade.amount} of {trade.productPair}");
        }

        public List<double> SetTargets(double ratio, bool buying, double price)
        {
            int factor = 1;
            double risk;

            if (!buying)
            {
                factor = -1;
            }

            if (ratio < 0.25)
            {
                risk = 1.5;
            }
            else if (ratio >= 0.25 && ratio < 0.5)
            {
                risk = 3;
            }
            else if (ratio >= 0.5 && ratio < 0.75)
            {
                risk = 4;
            }
            else if (ratio >= 0.75 && ratio < 1)
            {
                risk = 5;
            } else
            {
                risk = 6;
            }

            double stoploss = price * (1 - factor * (risk / 100));
            double target = price * (1 + factor * (risk / 100));
            double mainTarget = price * (1 + factor * (risk / 100) * 2);

            List<double> targets = new List<double>();
            targets.Add(stoploss);
            targets.Add(target);
            targets.Add(mainTarget);

            return targets;
        }

        public void Check_Stoploss(Trade trade, decimal price)
        {
            if (trade.orderSide.Equals(CoinbasePro.Services.Orders.Types.OrderSide.Buy))
            {
                //if (price <= trade.stoploss)
                //{
                //    // stoploss
                //    trade.open = false;
                //    trade.stoplossHit = true;
                //    trade.mainTargetHit = false;
                //}
            }

            if (trade.orderSide.Equals(CoinbasePro.Services.Orders.Types.OrderSide.Sell))
            {
                //if (price >= trade.stoploss)
                //{
                //    // stoploss
                //    trade.open = false;
                //    trade.stoplossHit = true;
                //    trade.mainTargetHit = false;
                //}
            }
        }
    }
}
