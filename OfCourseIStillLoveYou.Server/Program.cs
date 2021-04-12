using System;
using System.Threading.Tasks;
using System.Timers;
using Grpc.Core;
using OfCourseIStillLoveYou.Communication;
using OfCourseIStillLoveYou.Server.Services;

namespace OfCourseIStillLoveYou.Server
{
    internal class Program
    {
        private const string ExitCommand = "exit";
        private static string _serverEndpoint = "localhost";
        private static int _port = 5077;


        private static void Main(string[] args)
        {
            UpdateConfiguration(args);

            var infoTimer = new Timer(30000);
            infoTimer.Elapsed += Timer_Elapsed;

            infoTimer.Enabled = true;
            infoTimer.Start();

            var serverStub = new CameraStreamService();

            var server = new Grpc.Core.Server
            {
                Services =
                {
                    CameraStream.BindService(serverStub)
                },
                Ports = {new ServerPort(_serverEndpoint, _port, ServerCredentials.Insecure)}
            };
            server.Start();


            Console.WriteLine(
                $"OfCourseIStillLoveYou.Server is listening on {_serverEndpoint}:{_port}, waiting for incoming camera feed. Type exit for closing the server");

            var keyStroke = string.Empty;


            while (keyStroke != ExitCommand)
            {
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
                for (var i = 0; i < args.Length; i++)
                    if (args[i].Equals("--port"))
                        _port = int.Parse(args[i + 1]);
                    else if (args[i].Equals("--endpoint")) _serverEndpoint = args[i + 1];
            }
            catch (Exception)
            {
                Console.WriteLine("Incorrect arguments. Please use --port 5001 --endpoint 192.168.1.7");
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var numberOfCameras = CameraStreamService.CameraTextures.Count;

            Console.WriteLine(
                numberOfCameras > 0
                    ? $"Receiving video signal from {numberOfCameras} cameras. At {CameraStreamService.GetAverageFrames()} FPS"
                    : "Waiting for camera signal");
        }
    }
}