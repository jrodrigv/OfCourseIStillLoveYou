using System;
using UnityEngine;

namespace OfCourseIStillLoveYou
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class Settings : MonoBehaviour
    {
        public static string SettingsConfigUrl = "GameData/OfCourseIStillLoveYou/settings.cfg";
        public static int Port { get; set; }

        public static string EndPoint { get; set; }

        public static bool ConfigLoaded { get; set; }

        void Awake()
        {
            LoadConfig();
            ConfigLoaded = true;
        }

        public static void LoadConfig()
        {
            try
            {
                Debug.Log("[OfCourseIStillLoveYou]: Loading settings.cfg ==");

                ConfigNode fileNode = ConfigNode.Load(SettingsConfigUrl);
                if (!fileNode.HasNode("Settings")) return;

                ConfigNode settings = fileNode.GetNode("Settings");
                EndPoint = settings.GetValue("EndPoint");
                Port = int.Parse(settings.GetValue("Port"));
            }
            catch (Exception ex)
            {
                Debug.Log("[OfCourseIStillLoveYou]: Failed to load settings config:" + ex.Message);
            }
        }

    }
}