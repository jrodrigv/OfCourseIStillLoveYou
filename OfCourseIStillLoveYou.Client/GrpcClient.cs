using Grpc.Core;
using UnityEngine;
using OfCourseIStillLoveYou.Communication;

namespace OfCourseIStillLoveYou.Client
{
    public static class GrpcClient
    {
        //public static Greeter.GreeterClient Client { get; private set; }

        public static CameraStream.CameraStreamClient Client { get; set; }

        public static void ConnectToServer()
        {
            var channel = new Channel("localhost", 50777, ChannelCredentials.Insecure);

            Client = new CameraStream.CameraStreamClient(channel);

            Debug.Log($"[OfCourseIStillLoveYou]: GrpcClient Connected to Server");
        }

        public static void SendCameraTextureAsync(CameraData cameraData)
        {
            _ = Client.SendCameraStreamAsync(new SendCameraStreamRequest()
            {
                CameraId = cameraData.CameraId,
                CameraName = cameraData.CameraName,
                Speed = cameraData.Speed,
                Altitude = cameraData.Altitude,
                Texture = Google.Protobuf.ByteString.CopyFrom(cameraData.Texture)
            });

        }

    }
}
