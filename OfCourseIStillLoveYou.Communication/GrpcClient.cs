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


        //public static byte[] GetCameraTextureAsync(string cameraId)
        //{

        //    var cameraTextureProto = Client.GetCameraTexture(new GetCameraTextureRequest() { CameraId = cameraId });

        //    byte[] cameraTexture = new byte[cameraTextureProto.CalculateSize()];

        //    cameraTextureProto.Texture.CopyTo(cameraTexture, 0);
           
        //    return cameraTexture;
        //}

        public static CameraData GetCameraDataAsync(string cameraId)
        {

            var cameraTextureProto = Client.GetCameraTexture(new GetCameraTextureRequest() { CameraId = cameraId });

            byte[] cameraTexture = new byte[cameraTextureProto.Texture.Length];

            cameraTextureProto.Texture.CopyTo(cameraTexture, 0);

            return new CameraData()
            {
                CameraId = cameraTextureProto.CameraId,
                CameraName = cameraTextureProto.CameraName,
                Altitude = cameraTextureProto.Altitude,
                Speed = cameraTextureProto.Speed,
                Texture = cameraTexture
            };
        }
    }
}
