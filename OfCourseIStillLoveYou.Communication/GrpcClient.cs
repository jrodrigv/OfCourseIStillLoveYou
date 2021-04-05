using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace OfCourseIStillLoveYou.Communication
{
    public static class GrpcClient
    {
        public static CameraStream.CameraStreamClient Client { get; set; }


        public static void ConnectToServer()
        {
            var channel = new Channel("localhost", 50777, ChannelCredentials.Insecure);

            Client = new CameraStream.CameraStreamClient(channel);
        }

        public static void SendCameraTexture(int id, byte[] texture)
        {
            Client.SendCameraStream(new SendCameraStreamRequest()
            { CameraId = id.ToString(), Texture = Google.Protobuf.ByteString.CopyFrom(texture) });
        }

        public static List<string> GetCameraIds()
        {
            List<string> cameraIds = new List<string>();

            var cameraIdsProto = Client.GetActiveCameraIds(new GetActiveCameraIdsRequest());

            cameraIds = cameraIdsProto.Cameras.ToList<string>();
           
            return cameraIds;
        }

        public static Task<CameraData> GetCameraDataAsync(string cameraId)
        {
            var cameraTextureProto = Client.GetCameraTextureAsync(new GetCameraTextureRequest() { CameraId = cameraId });
    
            return cameraTextureProto.ResponseAsync.ContinueWith((previous) =>
            {
                if (previous.Result.Texture.Length == 0)
                {
                    return new CameraData()
                    {
                        Texture = null
                    };
                }

                byte[] cameraTexture = new byte[previous.Result.Texture.Length];

                previous.Result.Texture.CopyTo(cameraTexture, 0);

                return new CameraData()
                {
                    CameraId = previous.Result.CameraId,
                    CameraName = previous.Result.CameraName,
                    Altitude = previous.Result.Altitude,
                    Speed = previous.Result.Speed,
                    Texture = cameraTexture,
                };
            });
        }

        public static Task<int>  GetCurrentFPSAsync()
        {
            var result = Client.GetAverageFpsAsync(new GetAverageFpsRequest());

            return result.ResponseAsync.ContinueWith((previous) => previous.Result.AverageFps);
        }
    }
}
