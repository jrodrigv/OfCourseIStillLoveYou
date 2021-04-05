using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OfCourseIStillLoveYou.Server;
using System.Collections.Concurrent;
using System.Timers;

namespace OfCourseIStillLoveYou.Communication
{
    public class CameraStreamService : CameraStream.CameraStreamBase
    {
        private const int initialCapacity = 17;

        private const int timePeriod = 30;

        private static int accumulatedFrames = 0;

        private static int lastAverageFrames = 0;

        public static ConcurrentDictionary<string, CameraData> cameraTextures = new ConcurrentDictionary<string, CameraData>();

        public static ConcurrentDictionary<string, DateTime> cameraLastOperation = new ConcurrentDictionary<string, DateTime>();

        public CameraStreamService()
        {
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;

            cameraTextures = new ConcurrentDictionary<string, CameraData>(concurrencyLevel, initialCapacity);
            cameraLastOperation = new ConcurrentDictionary<string, DateTime>(concurrencyLevel, initialCapacity);

            Timer CleanCamerasTimer = new Timer(1000);
            CleanCamerasTimer.Elapsed += CleanCamerasTimer_Elapsed;

            CleanCamerasTimer.Enabled = true;
            CleanCamerasTimer.Start();
        }

        private static void CleanCamerasTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<string> camerasToDelete = new List<string>();

            foreach (var camera in cameraLastOperation)
            {
                if (DateTime.Now.Subtract(camera.Value).TotalSeconds > 2)
                {
                    camerasToDelete.Add(camera.Key);
                }
            }

            foreach (var cameraToDelete in camerasToDelete)
            {
                cameraTextures.TryRemove(new KeyValuePair<string, CameraData>(cameraToDelete, CameraStreamService.cameraTextures[cameraToDelete]));
                cameraLastOperation.TryRemove(new KeyValuePair<string, DateTime>(cameraToDelete, CameraStreamService.cameraLastOperation[cameraToDelete]));
            }
        }

        public static int GetAverageFrames()
        {

            var newAverageFrames = (int)Math.Round((decimal)((GetAccumulatedFrames() / timePeriod) / cameraTextures.Count), MidpointRounding.AwayFromZero);

            lastAverageFrames = newAverageFrames;

            return newAverageFrames;
        }

        private static int GetAccumulatedFrames()
        {
            var result = accumulatedFrames;
            accumulatedFrames = 0;
            return result;
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
                if (!cameraTextures.ContainsKey(request.CameraId)) 
                    return new GetCameraTextureResult() { Texture = Google.Protobuf.ByteString.Empty };

                GetCameraTextureResult result = new GetCameraTextureResult();
                result.CameraId = request.CameraId;
                result.Texture = Google.Protobuf.ByteString.CopyFrom(cameraTextures[request.CameraId].Texture);
                result.CameraName = cameraTextures[request.CameraId].CameraName;
                result.Speed = cameraTextures[request.CameraId].Speed;
                result.Altitude = cameraTextures[request.CameraId].Altitude;

                return result;
            });
        }

        public override Task<GetAverageFpsResult> GetAverageFps(GetAverageFpsRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                GetAverageFpsResult result = new GetAverageFpsResult();
                result.AverageFps = lastAverageFrames;

                return result;
            });
        }

    }
}
