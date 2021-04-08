using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HullcamVDS;
using OfCourseIStillLoveYou.Client;
using UnityEngine;
using UnityEngine.Rendering;

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

        void LateUpdate()
        {
            RenderCameras();
        }


        private void RenderCameras()
        {
            foreach (var trackedCamerasValue in TrackedCameras.Values)
            {
                if (trackedCamerasValue.Enabled)
                {
                    trackedCamerasValue.RenderCameras();
                    trackedCamerasValue.CalculateSpeedAltitude();
                }
            }
        }

    }
}
