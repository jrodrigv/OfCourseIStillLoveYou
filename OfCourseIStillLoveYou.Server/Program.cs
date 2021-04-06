using Grpc.Core;
using System;
using OfCourseIStillLoveYou.Communication;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace OfCourseIStillLoveYou.Server
{
    class Program
    {
        private static string ServerEndpoint = "localhost";
        private static int Port = 50777;
        const string ExitCommand = "exit";

    
        static void Main(string[] args)
        {
            UpdateConfiguration(args);

            Timer InfoTimer = new Timer(30000);
            InfoTimer.Elapsed += Timer_Elapsed;

            InfoTimer.Enabled = true;
            InfoTimer.Start();

            var serverStub = new CameraStreamService();

            var server = new Grpc.Core.Server
            {
                Services =
            {
                CameraStream.BindService(serverStub),
            },
                Ports = { new ServerPort(ServerEndpoint, Port, ServerCredentials.Insecure) }
            };
            server.Start();


            Console.WriteLine($"OfCourseIStillLoveYou.Server is listening on {ServerEndpoint}:{Port}, waiting for incoming camera feed. Type exit for closing the server");

            var keyStroke = string.Empty;


            while (keyStroke != ExitCommand)
            {
                var random = new Random();
                keyStroke = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(keyStroke) && keyStroke != ExitCommand)
                {

                }
                Task.Delay(100).Wait();
            }


            server.ShutdownAsync().Wait();
        }

        private static void UpdateConfiguration(string[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("--port"))
                    {
                        Port = Int32.Parse(args[i + 1]);
                    }
                    else if (args[i].Equals("--endpoint"))
                    {
                        ServerEndpoint = args[i + 1];
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Incorrect arguments. Please use --port 5001 --endpoint 192.168.1.7");
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var numberOfCameras = CameraStreamService.cameraTextures.Count;

            if(numberOfCameras > 0)
            {
                Console.WriteLine($"Receiving video signal from {numberOfCameras} cameras. At {CameraStreamService.GetAverageFrames()} FPS");
            }
            else
            {
                Console.WriteLine($"Waiting for camera signal");
            }
        }
    }
}
