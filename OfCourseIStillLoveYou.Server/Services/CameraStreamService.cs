using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Google.Protobuf;
using Grpc.Core;
using OfCourseIStillLoveYou.Communication;

namespace OfCourseIStillLoveYou.Server.Services
{
    public class CameraStreamService : CameraStream.CameraStreamBase
    {
        private const int InitialCapacity = 17;

        private const int TimePeriod = 30;

        private static int _accumulatedFrames;

        private static int _lastAverageFrames;

        public static ConcurrentDictionary<string, CameraData> CameraTextures = new();

        public static ConcurrentDictionary<string, DateTime> CameraLastOperation = new();

        public CameraStreamService()
        {
            var numProcs = Environment.ProcessorCount;
            var concurrencyLevel = numProcs * 2;

            CameraTextures = new ConcurrentDictionary<string, CameraData>(concurrencyLevel, InitialCapacity);
            CameraLastOperation = new ConcurrentDictionary<string, DateTime>(concurrencyLevel, InitialCapacity);

            var cleanCamerasTimer = new Timer(1000);
            cleanCamerasTimer.Elapsed += CleanCamerasTimer_Elapsed;

            cleanCamerasTimer.Enabled = true;
            cleanCamerasTimer.Start();
        }

        private static void CleanCamerasTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var camerasToDelete = new List<string>();

            foreach (var camera in CameraLastOperation)
                if (DateTime.Now.Subtract(camera.Value).TotalSeconds > 2)
                    camerasToDelete.Add(camera.Key);

            foreach (var cameraToDelete in camerasToDelete)
            {
                CameraTextures.TryRemove(
                    new KeyValuePair<string, CameraData>(cameraToDelete, CameraTextures[cameraToDelete]));
                CameraLastOperation.TryRemove(
                    new KeyValuePair<string, DateTime>(cameraToDelete, CameraLastOperation[cameraToDelete]));
            }
        }

        public static int GetAverageFrames()
        {
            var newAverageFrames =
                (int) Math.Round((decimal) (GetAccumulatedFrames() / TimePeriod / CameraTextures.Count),
                    MidpointRounding.AwayFromZero);

            _lastAverageFrames = newAverageFrames;

            return newAverageFrames;
        }

        private static int GetAccumulatedFrames()
        {
            var result = _accumulatedFrames;
            _accumulatedFrames = 0;
            return result;
        }


        public override Task<SendCameraStreamResult> SendCameraStream(SendCameraStreamRequest request,
            ServerCallContext context)
        {
            Task.Run(() =>
            {
                CameraData newCameraData = new()
                {
                    CameraId = request.CameraId, CameraName = request.CameraName, Altitude = request.Altitude,
                    Speed = request.Speed, Texture = request.Texture.ToByteArray()
                };

                _ = CameraTextures.AddOrUpdate(request.CameraId, newCameraData, (_, _) => newCameraData);

                var currentTime = DateTime.Now;
                _ = CameraLastOperation.AddOrUpdate(request.CameraId, currentTime, (_, _) => currentTime);

                _accumulatedFrames++;
            });

            return Task.FromResult(new SendCameraStreamResult());
        }

        public override Task<GetActiveCameraIdsResult> GetActiveCameraIds(GetActiveCameraIdsRequest request,
            ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var result = new GetActiveCameraIdsResult();
                result.Cameras.Add(CameraTextures.Keys);

                return result;
            });
        }

        public override Task<GetCameraTextureResult> GetCameraTexture(GetCameraTextureRequest request,
            ServerCallContext context)
        {
            return Task.Run(() =>
            {
                if (!CameraTextures.ContainsKey(request.CameraId))
                    return new GetCameraTextureResult {Texture = ByteString.Empty};

                var result = new GetCameraTextureResult
                {
                    CameraId = request.CameraId,
                    Texture = ByteString.CopyFrom(CameraTextures[request.CameraId].Texture),
                    CameraName = CameraTextures[request.CameraId].CameraName,
                    Speed = CameraTextures[request.CameraId].Speed,
                    Altitude = CameraTextures[request.CameraId].Altitude
                };

                return result;
            });
        }

        public override Task<GetAverageFpsResult> GetAverageFps(GetAverageFpsRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var result = new GetAverageFpsResult {AverageFps = _lastAverageFrames};

                return result;
            });
        }
    }
}