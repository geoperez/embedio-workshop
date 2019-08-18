using System;
using EmbedIO;

namespace EmbedioWorkshop
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var server = new WebServer("http://localhost:1234");

            server.RunAsync();
            
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
