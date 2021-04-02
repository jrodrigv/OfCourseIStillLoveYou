using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OfCourseIStillLoveYou.Server;
using System.Collections.Concurrent;

namespace OfCourseIStillLoveYou.Communication
{
    public class CameraStreamService : CameraStream.CameraStreamBase
    {
        private const int initialCapacity = 17;

        private const int timePeriod = 30;

        private static int accumulatedFrames = 0;


        public static int AverageFrames => (int)Math.Round((decimal)((GetAccumulatedFrames() / timePeriod) / cameraTextures.Count), MidpointRounding.AwayFromZero);

        private static int GetAccumulatedFrames()
        {
            var result = accumulatedFrames;
            accumulatedFrames = 0;
            return result;
        }

        public static ConcurrentDictionary<string, CameraData> cameraTextures = new ConcurrentDictionary<string, CameraData>();

        public static ConcurrentDictionary<string, DateTime> cameraLastOperation = new ConcurrentDictionary<string, DateTime>();


        public string SelectedCamera { get; set; }

        private static bool once = false;
        public CameraStreamService()
        {
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;

            cameraTextures = new ConcurrentDictionary<string, CameraData>(concurrencyLevel, initialCapacity);
            cameraLastOperation = new ConcurrentDictionary<string, DateTime>(concurrencyLevel, initialCapacity);
        }

        public override Task<SendCameraStreamResult> SendCameraStream(SendCameraStreamRequest request, ServerCallContext context)
        {
            Task.Run(() =>
            {
                
                CameraData newCameraData = new() { CameraId = request.CameraId, CameraName = request.CameraName, Altitude = request.Altitude, Speed = request.Speed, Texture = request.Texture.ToByteArray() };

                _ = cameraTextures.AddOrUpdate(request.CameraId, newCameraData, (key, oldValue) => newCameraData);

                var currentTime = DateTime.Now;
                _ = cameraLastOperation.AddOrUpdate(request.CameraId, currentTime, (key, oldValue) => currentTime);

                accumulatedFrames++;
            });

            return Task.FromResult(new SendCameraStreamResult
            {

            });
        }

        public override Task<GetActiveCameraIdsResult> GetActiveCameraIds(GetActiveCameraIdsRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                GetActiveCameraIdsResult result = new GetActiveCameraIdsResult();
                result.Cameras.Add(cameraTextures.Keys);

                return result;
            });
        }

        public override Task<GetCameraTextureResult> GetCameraTexture(GetCameraTextureRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                GetCameraTextureResult result = new GetCameraTextureResult();
                result.CameraId = request.CameraId;
                result.Texture = Google.Protobuf.ByteString.CopyFrom(cameraTextures[request.CameraId].Texture);
                result.CameraName = cameraTextures[request.CameraId].CameraName;
                result.Speed = cameraTextures[request.CameraId].Speed;
                result.Altitude = cameraTextures[request.CameraId].Altitude;

                return result;
            });
        }


        //public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        //{

        //    Console.WriteLine("Hello request received");
        //    return Task.FromResult(new HelloReply
        //    {
        //        Message = "Hello " + request.Name
        //    });
        //}
    }
}
