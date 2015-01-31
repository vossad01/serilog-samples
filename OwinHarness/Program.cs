using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace OwinHarness
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // You can use any compatible OWIN host
            // Self-hosting is used here in order to make it easier to automatically make requests on startup
            using (WebApp.Start<Startup>("http://localhost:58593"))
            {
                PerformExampleClientRequests().Wait();

                Console.WriteLine("Press any key to shutdown.");
                Console.ReadKey();
            }
        }

        private static async Task PerformExampleClientRequests()
        {
            var httpClient = new HttpClient();

            await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:58593/error"));

            await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:58593/hello/world"));

            await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:58593/missing/method"));
        }
    }
}