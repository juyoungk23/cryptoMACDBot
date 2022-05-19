using System;
using System.Threading.Tasks;

namespace cryptoMACDBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string apiKey = "e6c12defbf45a78e1bc24284197a9bcb";
            string apiSecret = "rFvCHPXBPCAQ8iM9SKmW3v6er+jtYVHsbdv6VUp3yurdSHs1nKNM1eXv8FUeZ46CuBUFpBTdB6FUHP0mCD+TPA==";
            string passphrase = "gs1kqbwt3i";

            var client = new Client(apiKey, apiSecret, passphrase);
            var accounts = await client.GetAllAccounts2();
            WebSocket webSocket = new WebSocket(client);
            Console.WriteLine(webSocket.GetHashCode());
 
        }

    }
}
