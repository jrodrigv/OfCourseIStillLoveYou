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

        private static int count = 0;
       
        static void Main(string[] args)
        {
            Timer InfoTimer = new Timer(30000);
            InfoTimer.Elapsed += Timer_Elapsed;

            InfoTimer.Enabled = true;
            InfoTimer.Start();

            Timer CleanCamerasTimer = new Timer(1000);
            CleanCamerasTimer.Elapsed += CleanCamerasTimer_Elapsed;

            CleanCamerasTimer.Enabled = true;
            CleanCamerasTimer.Start();

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


            Console.WriteLine("OfCourseIStillLoveYou.Server server has started, waiting for incoming requests. Type exit for closing the server");

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

        private static void CleanCamerasTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<string> camerasToDelete = new List<string>();


            foreach(var camera in CameraStreamService.cameraLastOperation)
            {
                if (DateTime.Now.Subtract(camera.Value).TotalSeconds > 2)
                {
                    camerasToDelete.Add(camera.Key);
                }
            }

            foreach (var cameraToDelete in camerasToDelete)
            {
                CameraStreamService.cameraTextures.Keys.Remove(cameraToDelete);
                CameraStreamService.cameraLastOperation.Keys.Remove(cameraToDelete);
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var numberOfCameras = CameraStreamService.cameraTextures.Count;

     
            Console.WriteLine($"Total number of camera registered = {numberOfCameras}");

            if(numberOfCameras > 0)
            {
                Console.WriteLine($"Average Frames per second = {CameraStreamService.AverageFrames}");
            }
          

        }
    }
}
