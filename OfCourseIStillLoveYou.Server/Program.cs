using Grpc.Core;
using System;
using OfCourseIStillLoveYou.Communication;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using System.Collections.Generic;

namespace OfCourseIStillLoveYou.Server
{
    class Program
    {
        const string ServerEndpoint = "localhost";
        const int Port = 50777;
        const string ExitCommand = "exit";

    
        static void Main(string[] args)
        {
            Timer InfoTimer = new Timer(30000);
            InfoTimer.Elapsed += Timer_Elapsed;

            InfoTimer.Enabled = true;
            InfoTimer.Start();

            var serverStub = new  CameraStreamService();

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
