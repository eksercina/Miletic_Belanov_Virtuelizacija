using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using Common;
using Client.Helpers;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== EV Charging Data Sender - More sessions ===\n");

            string projectPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string dataPath = Path.Combine(projectPath, "Data");

            if (!Directory.Exists(dataPath))
            {
                Console.WriteLine($"ERROR: Data folder not found at:\n{dataPath}");
                Console.ReadLine();
                return;
            }

            while (true)
            {
                IChargingService proxy = null;
                ChannelFactory<IChargingService> factory = null;

                try
                {
                    factory = new ChannelFactory<IChargingService>("ChargingServiceEndpoint");
                    proxy = factory.CreateChannel();

                    Console.WriteLine("Client connected successfully.\n");

                    var evFolders = Directory.GetDirectories(dataPath)
                                             .Where(d => File.Exists(Path.Combine(d, "Charging_Profile.csv")))
                                             .OrderBy(d => d)
                                             .ToList();

                    if (evFolders.Count == 0)
                    {
                        Console.WriteLine("No data folder found!");
                        break;
                    }

                    Console.WriteLine($"Found {evFolders.Count} vehicles:\n");
                    for (int i = 0; i < evFolders.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {Path.GetFileName(evFolders[i])}");
                    }

                    Console.WriteLine("\n0. Exit program");
                    Console.Write("\nChoose vehicle number: ");

                    if (!int.TryParse(Console.ReadLine(), out int choice))
                        continue;

                    if (choice == 0) break;

                    if (choice < 1 || choice > evFolders.Count)
                    {
                        Console.WriteLine("Incorrect selection!\n");
                        continue;
                    }

                    string selectedFolder = evFolders[choice - 1];
                    string vehicleId = Path.GetFileName(selectedFolder);
                    string csvPath = Path.Combine(selectedFolder, "Charging_Profile.csv");

                    Console.WriteLine($"\n>>> Sending for vehicle: {vehicleId} <<<");

                    bool simulateBreak = (choice == 12);

                    string sessionId = proxy.StartSession(vehicleId);
                    Console.WriteLine($"Session started! Session ID: {sessionId}\n");

                    int sent = 0, errors = 0, row = 1;

                    using (var reader = new StreamReader(csvPath))
                    {
                        reader.ReadLine();

                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            try
                            {
                                var sample = CsvHelper.ParseLine(line, row++, vehicleId);
                                proxy.PushSample(sample);
                                sent++;

                                if (sent % 150 == 0)
                                    Console.WriteLine($"   Sent: {sent} examples...");

                                if (simulateBreak && sent == 5)
                                {
                                    Console.WriteLine($"\n[INTERRUPTION SIMULATION] Transfer interruption for vehicle {vehicleId} (number 12) after {sent} examples!");
                                    throw new Exception("Simulated interruption of transfer - test of Dispose pattern");
                                }
                            }
                            catch (Exception ex) when (ex.Message.Contains("Simulated interruption"))
                            {
                                Console.WriteLine($"Interruption of transfer simulated successfully.");
                                break;
                            }
                            catch (Exception ex)
                            {
                                errors++;
                                Console.WriteLine($"   Error in row {row}: {ex.Message}");
                            }
                        }
                    }

                    proxy.EndSession(sessionId);

                    Console.WriteLine($"\n=== FINNISHED FOR {vehicleId.ToUpper()} ===");
                    Console.WriteLine($"Examples sent : {sent}");
                    Console.WriteLine($"Error        : {errors}\n");
                }
                catch (FaultException<ValidationFault> f)
                {
                    Console.WriteLine($"FAULT: {f.Detail.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAULT: {ex.Message}");
                }
                finally
                {
                    if (proxy != null)
                        try { ((IClientChannel)proxy).Close(); } catch { }
                    factory?.Close();
                }

                Console.WriteLine("Press ENTER for next vehicle...\n");
                Console.ReadLine();
            }

            Console.WriteLine("Program ended.");
            Console.ReadLine();
        }
    }
}