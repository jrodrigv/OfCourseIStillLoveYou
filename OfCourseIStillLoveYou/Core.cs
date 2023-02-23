using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HullcamVDS;
using OfCourseIStillLoveYou.Client;
using UnityEngine;

namespace OfCourseIStillLoveYou
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Core : MonoBehaviour
    {
        public static  Dictionary<int, TrackingCamera> TrackedCameras = new Dictionary<int, TrackingCamera>();

        private void Awake()
        {
            GrpcClient.ConnectToServer(Settings.EndPoint,Settings.Port);
        }


        public static void Log(string message)
        {
            Debug.Log($"[OfCourseIStillLoveYou]: {message}");
        }

        public static List<MuMechModuleHullCamera> GetAllTrackingCameras()
        {
            List<MuMechModuleHullCamera> result = new List<MuMechModuleHullCamera>();

            if (!FlightGlobals.ready) return result;


            foreach (var vessel in FlightGlobals.VesselsLoaded)
            {
                result.AddRange(vessel.FindPartModulesImplementing<MuMechModuleHullCamera>());
            }

            return result;
        }

        void Update()
        {
            ToggleRender();
        }

        void LateUpdate()
        {
            Refresh();
        }


        private void Refresh()
        {
            foreach (var trackedCamerasValue in TrackedCameras.Values.Where(trackedCamerasValue => trackedCamerasValue.Enabled))
            {
                if (!trackedCamerasValue.OddFrames) continue;
               
                trackedCamerasValue.CalculateSpeedAltitude();
                trackedCamerasValue.SendCameraImage();
               
            }
        }

        private void ToggleRender()
        {
            foreach (var trackedCamerasValue in TrackedCameras.Values.Where(trackedCamerasValue => trackedCamerasValue.Enabled))
            {
                trackedCamerasValue.ToogleCameras();
            }
        }
    }
}
