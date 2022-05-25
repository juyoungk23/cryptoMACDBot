using System;
using System.Threading.Tasks;
using Coinbase.Pro.Models;
using Newtonsoft.Json.Linq;

namespace cryptoMACDBot
{
    class Program
    {

        // Todo:
        // 1. Figure out how to update the MACD indicator as the data is streaming live


        static async Task Main(string[] args)
        {
            
            string apiKey = "07661b27d60a86197091a49404aff506";
            string apiSecret = "uEthX/aoXnXu8y9BP6r9Bj6iMcs6of0K45JIDn3fKi2rGqRNXnkeLQmqjOdFLEU8fqR4CeKGr2Ssff76TPQFog==";
            string passphrase = "sysr3fz9sbb";

            var client = new Client(apiKey, apiSecret, passphrase);
            CandleMaker candleMaker = new CandleMaker(client, 3);
            MACD macd = new MACD(candleMaker);


            // before I connect, I need to add all the indicators I want to listen to


            await candleMaker.Connect();

        }

    }
}
