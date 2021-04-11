using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;

namespace OfCourseIStillLoveYou.Communication
{
    public static class GrpcClient
    {
        public static CameraStream.CameraStreamClient Client { get; set; }


        public static void ConnectToServer(string endpoint = "localhost", int port = 50777)
        {
            var channel = new Channel(endpoint, port, ChannelCredentials.Insecure);

            Client = new CameraStream.CameraStreamClient(channel);
        }

        public static void SendCameraTexture(int id, byte[] texture)
        {
            Client.SendCameraStream(new SendCameraStreamRequest
                {CameraId = id.ToString(), Texture = ByteString.CopyFrom(texture)});
        }

        public static List<string> GetCameraIds()
        {
            var cameraIdsProto = Client.GetActiveCameraIds(new GetActiveCameraIdsRequest());

            var cameraIds = cameraIdsProto.Cameras.ToList();

            return cameraIds;
        }

        public static Task<CameraData> GetCameraDataAsync(string cameraId)
        {
            var cameraTextureProto = Client.GetCameraTextureAsync(new GetCameraTextureRequest {CameraId = cameraId});

            return cameraTextureProto.ResponseAsync.ContinueWith(previous =>
            {
                if (previous.Result.Texture.Length == 0)
                    return new CameraData
                    {
                        Texture = null
                    };

                var cameraTexture = new byte[previous.Result.Texture.Length];

                previous.Result.Texture.CopyTo(cameraTexture, 0);

                return new CameraData
                {
                    CameraId = previous.Result.CameraId,
                    CameraName = previous.Result.CameraName,
                    Altitude = previous.Result.Altitude,
                    Speed = previous.Result.Speed,
                    Texture = cameraTexture
                };
            });
        }
    }
}