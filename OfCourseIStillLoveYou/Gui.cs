using System.Collections.Generic;
using HullcamVDS;
using KSP.UI.Screens;
using UnityEngine;

namespace OfCourseIStillLoveYou
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Gui : MonoBehaviour
    {
        private const string ModTitle = "Of Course I Still Love you";
        private const float WindowWidth = 250;
        private const float DraggableHeight = 40;
        private const float LeftIndent = 12;
        private const float ContentTop = 20;
        public static Gui Fetch;
        public static bool GuiEnabled;
        public static bool HasAddedButton;
        private const float ContentWidth = WindowWidth - 2 * LeftIndent;
        private const float EntryHeight = 20;
        private bool _gameUiToggle;
        private float _windowHeight = 250;
        private Rect _windowRect;

  
        private static readonly GUIStyle CenterLabelStyle = new GUIStyle()
        { alignment = TextAnchor.UpperCenter, normal = { textColor = Color.white } };

        private static readonly GUIStyle TitleStyle = new GUIStyle(CenterLabelStyle)
        {
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter
        };

        void Awake()
        {
            if (Fetch)
                Destroy(Fetch);

            Fetch = this;
        }

        void Start()
        {
            _windowRect = new Rect(Screen.width - WindowWidth - 40, 100, WindowWidth, _windowHeight);
            AddToolbarButton();
            GameEvents.onHideUI.Add(GameUiDisable);
            GameEvents.onShowUI.Add(GameUiEnable);
            _gameUiToggle = true;
        }

        void OnGUI()
        {
            if (GuiEnabled && _gameUiToggle)
            {
                _windowRect = GUI.Window(1850, _windowRect, GuiWindow, "");
                UpdateAllCameras();
            }
        }

        void LateUpdate()
        {
            RemoveDisabledCameras();
        }

        private void RemoveDisabledCameras()
        {
            var camerasToDelete = new List<int>();

            foreach (var trackingCamera in Core.TrackedCameras)
                if (CameraHasToBeDeleted(trackingCamera.Value))
                {
                    trackingCamera.Value.Disable();
                    camerasToDelete.Add(trackingCamera.Value.Id);
                }

            foreach (var cameraId in camerasToDelete) Core.TrackedCameras.Remove(cameraId);
        }

        private bool CameraHasToBeDeleted(TrackingCamera trackingCamera)
        {
            return !trackingCamera.Enabled || trackingCamera.Vessel == null || !trackingCamera.Vessel.loaded;
        }

        private void GuiWindow(int windowId)
        {
            GUI.DragWindow(new Rect(0, 0, WindowWidth, DraggableHeight));
            var line = 0;

            DrawTitle();
            line++;

            foreach (var muMechModuleHullCamera in Core.GetAllTrackingCameras())
            {
                line++;

                if (!Core.TrackedCameras.ContainsKey(muMechModuleHullCamera.GetInstanceID()))
                    DrawCameraButton(muMechModuleHullCamera, line);
            }

            line++;

            _windowHeight = ContentTop + line * EntryHeight + EntryHeight + EntryHeight;
            _windowRect.height = _windowHeight;
        }

        private void UpdateAllCameras()
        {
            foreach (var trackingCamera in Core.TrackedCameras)
                if (trackingCamera.Value.Enabled)
                {
                    trackingCamera.Value.CheckIfResizing();
                    trackingCamera.Value.CreateGui();
                }
        }

        private string GetCameraName(MuMechModuleHullCamera muMechModuleHullCamera)
        {
            return muMechModuleHullCamera.vessel.GetDisplayName() + "." + muMechModuleHullCamera.cameraName;
        }


        private void DrawTitle()
        {
            GUI.Label(new Rect(0, 0, WindowWidth, 20), ModTitle, TitleStyle);
        }

        private void DrawCameraButton(MuMechModuleHullCamera camera, int line)
        {
            var saveRect = new Rect(LeftIndent, ContentTop + line * EntryHeight, ContentWidth, EntryHeight);

            if (GUI.Button(saveRect, GetCameraName(camera))) OpenCameraInstance(camera);
        }

        public void OpenCameraInstance(MuMechModuleHullCamera camera)
        {
            if (GuiEnabled && _gameUiToggle)
                if (!Core.TrackedCameras.ContainsKey(camera.GetInstanceID()))
                {
                    var newCamera = new TrackingCamera(camera.GetInstanceID(), camera);

                    StartCoroutine(newCamera.SendCameraImage());

                    Core.TrackedCameras.Add(camera.GetInstanceID(), newCamera);
                }
        }

        private void AddToolbarButton()
        {
            if (!HasAddedButton)
            {
                Texture buttonTexture = GameDatabase.Instance.GetTexture("OfCourseIStillLoveYou/Textures/icon", false);
                ApplicationLauncher.Instance.AddModApplication(EnableGui, DisableGui, Dummy, Dummy, Dummy, Dummy,
                    ApplicationLauncher.AppScenes.FLIGHT, buttonTexture);
                HasAddedButton = true;
            }
        }

        private void EnableGui()
        {
            GuiEnabled = true;
            Core.Log(" Showing GUI");
        }

        private void DisableGui()
        {
            GuiEnabled = false;
            Core.Log("Hiding GUI");
        }

        private void Dummy()
        {
        }

        private void GameUiEnable()
        {
            _gameUiToggle = true;
        }

        private void GameUiDisable()
        {
            _gameUiToggle = false;
        }
    }
}