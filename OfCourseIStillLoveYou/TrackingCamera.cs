using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HullcamVDS;
using UnityEngine;

namespace OfCourseIStillLoveYou
{
    public class TrackingCamera
    {
        private readonly int _id;
        private readonly MuMechModuleHullCamera _hullcamera;
        private float _windowWidth;
        private float _windowHeight;
        private Rect _windowRect;
        private float camImageSize = 360;
        private float adjCamImageSize = 360;
        public RenderTexture targetCamRenderTexture;
        private static float buttonHeight = 18;
        private static float gap = 2;


        private Camera[] cameras = new Camera[3];

        public bool Enabled { get; set; }

        //private Camera partRealCamera;


        public static Texture2D resizeTexture =
            GameDatabase.Instance.GetTexture("OfCourseIStillLoveYou/Textures/" + "resizeSquare", false);

        public float TARGET_WINDOW_SCALE_MAX { get; set; } = 2f;

        public float TARGET_WINDOW_SCALE_MIN { get; set; } = 0.5f;


        public bool ResizingWindow { get; set; }

        public bool windowIsOpen { get; set; }

        public float TARGET_WINDOW_SCALE { get; set; } = 1;


        public TrackingCamera(int id, MuMechModuleHullCamera hullcamera)
        {
            _id = id;
            _hullcamera = hullcamera;

            targetCamRenderTexture = new RenderTexture(768, 768, 24);
            targetCamRenderTexture.antiAliasing = 1;
            targetCamRenderTexture.Create();
            _windowWidth = adjCamImageSize + (3 * buttonHeight) + 16 + 2 * gap;
            _windowHeight = adjCamImageSize + 23;
            this._windowRect = new Rect(Screen.width - _windowWidth, Screen.height - _windowHeight, _windowWidth, _windowHeight);
            SetCameras();
            
            Enabled = true;

        }

        private Camera FindCamera(string cameraName)
        {
            foreach (Camera cam in Camera.allCameras)
            {
                if (cam.name == cameraName)
                {
                    return cam;
                }

            }
            Debug.Log("Couldn't find " + cameraName);
            return null;
        }

        private void SetCameras()
        {


            GameObject cam1Obj = new GameObject();
            Camera partNearCamera = cam1Obj.AddComponent<Camera>();
   
            partNearCamera.CopyFrom(Camera.allCameras.FirstOrDefault(cam => cam.name == "Camera 00"));
            partNearCamera.name = "jrNear";
            partNearCamera.transform.parent = _hullcamera.cameraTransformName.Length <= 0 ? _hullcamera.part.transform : _hullcamera.part.FindModelTransform(_hullcamera.cameraTransformName);
            partNearCamera.transform.localRotation = Quaternion.LookRotation(_hullcamera.cameraForward, _hullcamera.cameraUp);
            partNearCamera.transform.localPosition = _hullcamera.cameraPosition;
            partNearCamera.fieldOfView = this._hullcamera.cameraFoV;
            partNearCamera.targetTexture = targetCamRenderTexture;
            cameras[0] = partNearCamera;


            GameObject cam2Obj = new GameObject();
            Camera partScaledCamera = cam2Obj.AddComponent<Camera>();
            Camera mainSkyCam = FindCamera("Camera ScaledSpace");

            partScaledCamera.CopyFrom(mainSkyCam);
            partScaledCamera.name = "jrScaled";


            partScaledCamera.transform.parent = mainSkyCam.transform;
            partScaledCamera.transform.localRotation = Quaternion.identity;
            partScaledCamera.transform.localPosition = Vector3.zero;
            partScaledCamera.fieldOfView = 60;
            partScaledCamera.targetTexture = targetCamRenderTexture;
            partScaledCamera.transform.localScale = Vector3.one;

            cameras[1] = partScaledCamera;
            var camRotator = cam2Obj.AddComponent<TgpCamRotator>();
            camRotator.NearCamera = partNearCamera;


            //galaxy camera
            GameObject galaxyCamObj = new GameObject();
            Camera galaxyCam = galaxyCamObj.AddComponent<Camera>();
            Camera mainGalaxyCam = FindCamera("GalaxyCamera");

            galaxyCam.CopyFrom(mainGalaxyCam);
            galaxyCam.name = "jrGalaxy";
            galaxyCam.transform.parent = mainGalaxyCam.transform;
            galaxyCam.transform.position = Vector3.zero;
            galaxyCam.transform.localRotation = Quaternion.identity;
            galaxyCam.transform.localScale = Vector3.one;
            galaxyCam.fieldOfView = 60;
            galaxyCam.targetTexture = targetCamRenderTexture;
            cameras[2] = galaxyCam;
            var camRotatorgalaxy = galaxyCamObj.AddComponent<TgpCamRotator>();
            camRotatorgalaxy.NearCamera = partNearCamera;

            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].enabled = false;
            }
        }


        public void CreateGUI()
        {
            if (_hullcamera == null || _hullcamera.vessel == null)
            {
                Disable();
                return;
            }

            if (!Enabled) return;

            _windowRect = GUI.Window(_id, _windowRect, WindowTargetCam, _hullcamera.vessel.GetDisplayName() + "." + _hullcamera.cameraName);
        }

        public void CheckIfResizing()
        {
            if (!Enabled) return;

            if (Event.current.type == EventType.MouseUp)
            {
                if (ResizingWindow) ResizingWindow = false;
            }
        }

        void WindowTargetCam(int windowID)
        {
            if (!Enabled) return;

            float windowScale = TARGET_WINDOW_SCALE;
            adjCamImageSize = camImageSize * windowScale;
   

            windowIsOpen = true;

            GUI.DragWindow(new Rect(0, 0, this._windowHeight - 18, 30));
            if (GUI.Button(new Rect(_windowWidth - 18, 2, 16, 16), "X", GUI.skin.button))
            {
                Disable();
                

                
                return;
            }

            Rect imageRect = new Rect(2, 20, adjCamImageSize, adjCamImageSize);


            GUI.DrawTexture(imageRect, targetCamRenderTexture, ScaleMode.StretchToFill, false);
            
            //resizing
            Rect resizeRect =
                new Rect(_windowWidth - 18, _windowHeight - 18, 16, 16);
           
            
            GUI.DrawTexture(resizeRect, resizeTexture, ScaleMode.StretchToFill, true);
            
            if (Event.current.type == EventType.MouseDown && resizeRect.Contains(Event.current.mousePosition))
            {
                ResizingWindow = true;
            }

            if (Event.current.type == EventType.Repaint && ResizingWindow)
            {
                if (Math.Abs(Mouse.delta.x) > 1 || Math.Abs(Mouse.delta.y) > 0.1f)
                {
                    var diff = Mouse.delta.x + Mouse.delta.y;
                    UpdateTargetScale(diff);
                    ResizeTargetWindow();
                }
            }
            //ResetZoomKeys();
            RepositionWindow(ref _windowRect);
        }
        void UpdateTargetScale(float diff)
        {
            float scaleDiff = ((diff / (_windowRect.width + _windowRect.height)) * 100 * .01f);
            TARGET_WINDOW_SCALE += Mathf.Abs(scaleDiff) > .01f ? scaleDiff : scaleDiff > 0 ? .01f : -.01f;

            TARGET_WINDOW_SCALE += Mathf.Abs(scaleDiff) > .01f ? scaleDiff : scaleDiff > 0 ? .01f : -.01f;
            TARGET_WINDOW_SCALE = Mathf.Clamp(TARGET_WINDOW_SCALE,
              TARGET_WINDOW_SCALE_MIN,
                TARGET_WINDOW_SCALE_MAX);
        }


        void ResizeTargetWindow()
        {
            _windowWidth = camImageSize * TARGET_WINDOW_SCALE + (3 * buttonHeight) + 16 + 2 * gap;
            _windowHeight = camImageSize * TARGET_WINDOW_SCALE + 23;
            _windowRect = new Rect(_windowRect.x, _windowRect.y, _windowWidth, _windowHeight);
        }

        internal static void RepositionWindow(ref Rect windowPosition)
        {
            // This method uses Gui point system.
            if (windowPosition.x < 0) windowPosition.x = 0;
            if (windowPosition.y < 0) windowPosition.y = 0;

            if (windowPosition.xMax > Screen.width)
                windowPosition.x = Screen.width - windowPosition.width;
            if (windowPosition.yMax > Screen.height)
                windowPosition.y = Screen.height - windowPosition.height;
        }

        public void RenderCameras()
        {
            for (int i = cameras.Length-1; i >= 0; i--)
            {
                if (cameras[i] == null) return;

                if (i > 0)
                {
                    cameras[i].fieldOfView = 60;
                }
                cameras[i].Render();
            }
        }

        public void Disable()
        {
            Core.TrackedCameras.Remove(this._id);
            this.Enabled = false;
           
            foreach (var camera in cameras)
            {
                if (camera == null) continue;

                camera.enabled = false;
            }

            this.cameras = null;
        }

   
    }


   
}
