using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CoinbasePro.Network.Authentication;
using System.Linq;

namespace cryptoMACDBot
{
    public class Client
    {

        public CoinbasePro.CoinbaseProClient client { get; }
        public string apiKey { get; }
        public string apiSecret { get; }
        public string passphrase { get; }
        public decimal tradeQuantity { get; set; }
        public string productPair { get; set; }
        public List<double> winLossTracker = new List<double>();

        public Client(string apiKey, string apiSecret, string passphrase)
        {
            this.apiKey = apiKey;
            this.apiSecret = apiSecret;
            this.passphrase = passphrase;

            var authenticator = new Authenticator(apiKey, apiSecret, passphrase);
            var coinbaseProClient = new CoinbasePro.CoinbaseProClient(authenticator);
            this.client = coinbaseProClient;
        }


        public async Task<List<CoinbasePro.Services.CoinbaseAccounts.Models.CoinbaseAccount>> GetAllAccounts()
        {
            var allAccounts = await client.CoinbaseAccountsService.GetAllAccountsAsync();
            return (List<CoinbasePro.Services.CoinbaseAccounts.Models.CoinbaseAccount>)allAccounts;
        }

        public async Task<List<Candle>> GetHistoricalData(DateTime startDate, DateTime endDate, CoinbasePro.Services.Products.Types.CandleGranularity granularity)
        {
            // Iterates data backwards, so first element in array is latest candle. Last element is start date.
          
            var historicalData = await client.ProductsService.GetHistoricRatesAsync(productPair, startDate, endDate, granularity);

            List<Candle> processedHistoricalData = new List<Candle>();
            foreach (var candle in historicalData)
            {
                Candle newCandle = new Candle(candle.Time, (decimal)candle.Open, (decimal)candle.High, (decimal)candle.Low);
                newCandle.Close = (decimal)candle.Close;
                processedHistoricalData.Add(newCandle);
            }
            processedHistoricalData.Reverse();
            return processedHistoricalData;
        }

        public async void PlaceLongOrder(Trade trade)
        {
            await client.OrdersService.PlaceMarketOrderAsync(CoinbasePro.Services.Orders.Types.OrderSide.Buy, trade.productPair, trade.amount);
            Console.WriteLine($"BOUGHT {trade.amount} of {trade.productPair}");
        }

        public async void PlaceShortOrder(Trade trade)
        {
            await client.OrdersService.PlaceMarketOrderAsync(CoinbasePro.Services.Orders.Types.OrderSide.Sell, trade.productPair, trade.amount);
            Console.WriteLine($"SOLD {trade.amount} of {trade.productPair}");
        }

        public void CheckStoploss(Trade trade, decimal price)
        {
            if (!trade.open)
            {
                return;
            }

            if (trade.orderSide.Equals(CoinbasePro.Services.Orders.Types.OrderSide.Buy))
            {
                if (price <= trade.stoploss)
                {
                    trade.stoplossHit = true;
                    trade.inPosition = false;
                    // PlaceShortOrder(trade);
                }
            }

            if (trade.orderSide.Equals(CoinbasePro.Services.Orders.Types.OrderSide.Sell))
            {
                if (price >= trade.stoploss)
                {
                    trade.stoplossHit = true;
                    trade.inPosition = true;
                    // PlaceLongOrder(trade);
                }
            }

            if (trade.stoplossHit)
            {
                tradeQuantity *= 0.98m;
                trade.open = false;

                if (trade.targetHit)
                {
                    winLossTracker.Add(0.25);
                } else
                {
                    winLossTracker.Add(0);
                }

                //Console.WriteLine($"Stoploss Hit! Price: {price}");
            }

        }

        public void CheckProfits(Trade trade, decimal price)
        {
            if (!trade.open)
            {
                return;
            }

            if (trade.orderSide.Equals(CoinbasePro.Services.Orders.Types.OrderSide.Buy))
            {
                if (price >= trade.target && !trade.targetHit)
                {
                    Trade takingSomeProfits = new Trade(trade.amount * 0.25m, 0, 0, 0, productPair, CoinbasePro.Services.Orders.Types.OrderSide.Sell, DateTime.Now, price);
                    // PlaceShortOrder(takingSomeProfits);
                    trade.amount *= 0.75m;
                    trade.stoploss = trade.priceAtTrade;
                    trade.targetHit = true;
                    trade.inPosition = true;

                    // Console.WriteLine($"Target Hit! ${price}");
                }

                if (price >= trade.mainTarget && !trade.mainTargetHit && trade.targetHit)
                {
                    Trade takeRemainingProfits = new Trade(trade.amount, 0, 0, 0, productPair, CoinbasePro.Services.Orders.Types.OrderSide.Sell, DateTime.Now, price);
                    // PlaceShortOrder(takeRemainingProfits);
                    tradeQuantity *= 1.02m;
                    trade.mainTargetHit = true;
                    trade.inPosition = false;
                    trade.open = false;
                    winLossTracker.Add(1);

                    // Console.WriteLine($"Main Target Hit! ${price}");

                }
            }

            if (trade.orderSide.Equals(CoinbasePro.Services.Orders.Types.OrderSide.Sell))
            {
                if (price <= trade.target && !trade.targetHit)
                {
                    Trade takingSomeProfits = new Trade(trade.amount * 0.25m, 0, 0, 0, productPair, CoinbasePro.Services.Orders.Types.OrderSide.Sell, DateTime.Now, price);
                    // PlaceShortOrder(takingSomeProfits);
                    trade.amount *= 0.75m;
                    trade.stoploss = trade.priceAtTrade;
                    trade.inPosition = false;
                    trade.targetHit = true;

                    // Console.WriteLine($"Target Hit! ${price}");
                }

                if (price <= trade.mainTarget && !trade.mainTargetHit && trade.targetHit)
                {
                    Trade takeRemainingProfits = new Trade(trade.amount, 0, 0, 0, productPair, CoinbasePro.Services.Orders.Types.OrderSide.Sell, DateTime.Now, price);
                    // PlaceShortOrder(takeRemainingProfits);
                    tradeQuantity *= 1.02m;
                    trade.mainTargetHit = true;
                    trade.inPosition = true;
                    trade.open = false;
                    winLossTracker.Add(1);

                    // Console.WriteLine($"Main Target Hit! ${price}");
                    
                }
            }
        }

        public List<decimal> SetTargets(double ratio, bool buying, decimal price)
        {
            int factor = 1;
            decimal risk;
            decimal riskRewardRatio = 0.75m;

            if (!buying)
            {
                factor = -1;
            }

            if (ratio < 0.25)
            {
                risk = 1;
            }
            else if (ratio >= 0.25 && ratio < 0.5)
            {
                risk = 1.5m;
            }
            else if (ratio >= 0.5 && ratio < 0.75)
            {
                risk = 2m;
            }
            else if (ratio >= 0.75 && ratio < 1)
            {
                risk = 3m;
            }
            else
            {
                risk = 4m;
            }

            decimal stoploss = price * (1 - factor * (risk / 100));
            decimal target = price * (1 + factor * (risk / 100));
            decimal mainTarget = price * (1 + factor * (risk / 100) * riskRewardRatio);

            List<decimal> targets = new List<decimal>();
            targets.Add(stoploss);
            targets.Add(target);
            targets.Add(mainTarget);

            return targets;
        }

        public double GetWinLossPercentage()
        {
            return winLossTracker.Sum() / winLossTracker.Count * 100;
            
        }

        public void SetTradeAmount(decimal amount)
        {
            tradeQuantity = amount;
        }

        public void SetProductPair(string pair)
        {
            productPair = pair;
        }
    }
}
