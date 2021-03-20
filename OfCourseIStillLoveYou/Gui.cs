using System;
using System.Collections.Generic;
using HullcamVDS;
using KSP.UI.Screens;
using UnityEngine;

namespace OfCourseIStillLoveYou
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Gui : MonoBehaviour
    {

        private const float WindowWidth = 250;
        private const float DraggableHeight = 40;
        private const float LeftIndent = 12;
        private const float ContentTop = 20;
        public static Gui Fetch;
        public static bool GuiEnabled;
        public static bool HasAddedButton;
        private readonly float contentWidth = WindowWidth - 2 * LeftIndent;
        private readonly float entryHeight = 20;
        private bool _gameUiToggle;
        private float _windowHeight = 250;
        private Rect _windowRect;

        
        private void Awake()
        {
            if (Fetch)
                Destroy(Fetch);

            Fetch = this;
        }

        private void Start()
        {
            _windowRect = new Rect(Screen.width - WindowWidth - 40, 100, WindowWidth, _windowHeight);
            AddToolbarButton();
            GameEvents.onHideUI.Add(GameUiDisable);
            GameEvents.onShowUI.Add(GameUiEnable);
            _gameUiToggle = true;

            foreach (Camera cam in Camera.allCameras)
            {
                Debug.Log("JR CAMERA: " + cam.name);
            }
        }

        // ReSharper disable once InconsistentNaming
        private void OnGUI()
        {
            if (GuiEnabled && _gameUiToggle)
            {
                _windowRect = GUI.Window(1850, _windowRect, GuiWindow, "");
                UpdateAllCameras();
            }
            
        }

        private void GuiWindow(int windowId)
        {
            GUI.DragWindow(new Rect(0, 0, WindowWidth, DraggableHeight));
            int line = 0;

            DrawTitle();
            line++;

            foreach (var muMechModuleHullCamera in Core.GetAllTrackingCameras())
            {
                line++;

                if (!Core.TrackedCameras.ContainsKey(muMechModuleHullCamera.GetInstanceID()))
                {
                    DrawCameraButton(muMechModuleHullCamera, line);
                }
               
            }
            line++;

            _windowHeight = ContentTop + line * entryHeight + entryHeight + entryHeight;
            _windowRect.height = _windowHeight;
        }

        private void UpdateAllCameras()
        {
            foreach (var trackingCamera in Core.TrackedCameras)
            {

                if (trackingCamera.Value.Enabled)
                {

                    trackingCamera.Value.CheckIfResizing();
                    trackingCamera.Value.CreateGui();
                }
            }
        }

        private string GetCameraName(MuMechModuleHullCamera muMechModuleHullCamera)
        {
            return muMechModuleHullCamera.vessel.GetDisplayName() + "." + muMechModuleHullCamera.cameraName;
        }

 
        private void DrawTitle()
        {
            var centerLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };
            var titleStyle = new GUIStyle(centerLabel)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(0, 0, WindowWidth, 20), "Of Course I Still Love you", titleStyle);
        }

        private void DrawCameraButton(MuMechModuleHullCamera camera, int line)
        {
            var saveRect = new Rect(LeftIndent, ContentTop + line * entryHeight, contentWidth, entryHeight);
            

            if (GUI.Button(saveRect, GetCameraName(camera)))
            {
                OpenCameraInstance( camera);
            }
              
        }

        public void OpenCameraInstance(MuMechModuleHullCamera camera)
        {
            if (GuiEnabled && _gameUiToggle)
            {
                if (!Core.TrackedCameras.ContainsKey(camera.GetInstanceID()))
                {
                    var newCamera = new TrackingCamera(camera.GetInstanceID(), camera);
                    Core.TrackedCameras.Add(camera.GetInstanceID(), newCamera);
                }
            }
               
        }

        private void AddToolbarButton()
        {
            if (!HasAddedButton)
            {
                Texture buttonTexture = GameDatabase.Instance.GetTexture("OfCourseIStillLoveYou/Textures/icon", false);
                ApplicationLauncher.Instance.AddModApplication(EnableGui, DisableGui, Dummy, Dummy, Dummy, Dummy,
                    ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
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