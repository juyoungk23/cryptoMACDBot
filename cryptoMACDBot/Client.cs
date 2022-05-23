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

        public CoinbasePro.CoinbaseProClient client {get;}

        public string apiKey { get;}

        public string apiSecret { get; }

        public string passphrase { get; }

        public async Task<List<CoinbasePro.Services.CoinbaseAccounts.Models.CoinbaseAccount>> GetAllAccounts()
        {
            var allAccounts = await client.CoinbaseAccountsService.GetAllAccountsAsync();

            return (List<CoinbasePro.Services.CoinbaseAccounts.Models.CoinbaseAccount>) allAccounts;
        }

        public async void PlaceLongOrder(string productPair, decimal amount)
        {
            await client.OrdersService.PlaceMarketOrderAsync(CoinbasePro.Services.Orders.Types.OrderSide.Buy, productPair, amount);
            Console.WriteLine($"BOUGHT {amount} of {productPair}");
        }

        public async void PlaceShortOrder(string productPair, decimal amount)
        {
            await client.OrdersService.PlaceMarketOrderAsync(CoinbasePro.Services.Orders.Types.OrderSide.Sell, productPair, amount);
            Console.WriteLine($"SOLD {amount} of {productPair}");
        }

    }
}
