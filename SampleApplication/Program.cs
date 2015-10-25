using PixyUSBNet;
using System;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set up our connection to pixy:
            var pixyConnection = new PixyConnection();

            Console.WriteLine("Attempting to connect to Pixie...");
            var connectionResult = pixyConnection.Initialize();

            // Make sure connection successful..
            if (connectionResult == PixyResult.SUCCESSFUL_CONNECTION)
            {
                Console.WriteLine("Pixie connected!");
                try
                {
                    // Read some Pixy config:
                    Console.WriteLine("Ver:" + pixyConnection.GetFirmwareVersion().ToString());
                    Console.WriteLine("Servo 0 position: " + pixyConnection.ServoGetPosition(0).ToString());
                    Console.WriteLine("Servo 1 position: " + pixyConnection.ServoGetPosition(1).ToString());
                    Console.WriteLine();
                    Console.WriteLine("Listening for data in 2 seconds...");
                    System.Threading.Thread.Sleep(2000);

                    // Press any key to end connection:
                    while (!Console.KeyAvailable)
                    {
                        // Do we have block data we can read?
                        if (pixyConnection.BlockDataAvailable())
                        {
                            // Do we have image blocks?
                            Block[] blocks = pixyConnection.GetBlocks(2);

                            Console.Clear();
                            Console.SetCursorPosition(0, 0);

                            // If yes, display them:
                            if (blocks != null && blocks.Length > 0)
                            {
                                Console.WriteLine($"Press any key to disconnect; Showing {blocks.Length} block[s]:");
                                if (blocks.Length > 0)
                                {
                                    Console.SetCursorPosition(0, 0);
                                    for (int x = 0; x < blocks.Length; x++)
                                    {
                                        Console.SetCursorPosition(blocks[x].X / 4, blocks[x].Y / 8);
                                        Console.Write("[*]");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Press any key to disconnect; no signatures detected. Bring one within view or set a signature. ");
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error when reading pixy: " + ex.Message);
                }
                finally
                {
                    Console.WriteLine("Done; attempting to disconnect...");
                    pixyConnection.Close();
                    Console.WriteLine("Disconnected!");
                }
            }
            else
            {
                Console.WriteLine("Could not connect to pixie. :( " + connectionResult.ToString());
            }
            Console.ReadKey(true);
        }
    }
}
