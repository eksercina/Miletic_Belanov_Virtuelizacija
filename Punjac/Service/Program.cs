using System;
using System.ServiceModel;
using Service.Services;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = null;

            try
            {
                host = new ServiceHost(typeof(ChargingService));
                host.Open();

                Console.WriteLine("==================================================");
                Console.WriteLine("     WCF Charging Service - SERVER IS RUNNING");
                Console.WriteLine("     Address: net.tcp://localhost:4000/ChargingService");
                Console.WriteLine("==================================================");
                Console.WriteLine("Press ENTER to shut down the service...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                host?.Close();
            }
        }
    }
}