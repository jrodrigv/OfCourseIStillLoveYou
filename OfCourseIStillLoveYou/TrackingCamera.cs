using System;
using System.Linq;
using System.Text;
using HullcamVDS;
using scatterer;
using UnityEngine;

namespace OfCourseIStillLoveYou
{
    public class TrackingCamera
    {
        private static readonly float buttonHeight = 18;
        private static readonly float gap = 2;

        //private Camera partRealCamera;


        public static Texture2D ResizeTexture =
            GameDatabase.Instance.GetTexture("OfCourseIStillLoveYou/Textures/" + "resizeSquare", false);

        private readonly MuMechModuleHullCamera _hullcamera;
        private readonly int _id;
        private readonly float camImageSize = 360;
        private float _adjCamImageSize = 360;


        private Camera[] _cameras = new Camera[3];
        private float _windowHeight;
        private Rect _windowRect;
        private float _windowWidth;
        public RenderTexture TargetCamRenderTexture;


        public TrackingCamera(int id, MuMechModuleHullCamera hullcamera)
        {
            _id = id;
            _hullcamera = hullcamera;

            TargetCamRenderTexture = new RenderTexture(768, 768, 24);
            TargetCamRenderTexture.antiAliasing = 1;
            TargetCamRenderTexture.Create();
            _windowWidth = _adjCamImageSize + 3 * buttonHeight + 16 + 2 * gap;
            _windowHeight = _adjCamImageSize + 23;
            _windowRect = new Rect(Screen.width - _windowWidth, Screen.height - _windowHeight, _windowWidth,
                _windowHeight);
            SetCameras();

            Enabled = true;
        }

        public bool Enabled { get; set; }

        public float TargetWindowScaleMax { get; set; } = 2f;

        public float TargetWindowScaleMin { get; set; } = 0.5f;


        public bool ResizingWindow { get; set; }

        public bool WindowIsOpen { get; set; }

        public float TargetWindowScale { get; set; } = 1;

        private Camera FindCamera(string cameraName)
        {
            foreach (var cam in Camera.allCameras)
                if (cam.name == cameraName)
                    return cam;

            Debug.Log("Couldn't find " + cameraName);
            return null;
        }

        private void SetCameras()
        {
            var cam1Obj = new GameObject();
            var partNearCamera = cam1Obj.AddComponent<Camera>();

            partNearCamera.CopyFrom(Camera.allCameras.FirstOrDefault(cam => cam.name == "Camera 00"));
            partNearCamera.name = "jrNear";
            partNearCamera.transform.parent = _hullcamera.cameraTransformName.Length <= 0
                ? _hullcamera.part.transform
                : _hullcamera.part.FindModelTransform(_hullcamera.cameraTransformName);
            partNearCamera.transform.localRotation =
                Quaternion.LookRotation(_hullcamera.cameraForward, _hullcamera.cameraUp);
            partNearCamera.transform.localPosition = _hullcamera.cameraPosition;
            partNearCamera.fieldOfView = _hullcamera.cameraFoV;
            partNearCamera.targetTexture = TargetCamRenderTexture;
            _cameras[0] = partNearCamera;
            var partialUnifiedCameraDepthBuffer = (PartialDepthBuffer) _cameras[0].gameObject.AddComponent(typeof(PartialDepthBuffer));
            partialUnifiedCameraDepthBuffer.Init(partNearCamera);
            
            
            
            var cam2Obj = new GameObject();
            var partScaledCamera = cam2Obj.AddComponent<Camera>();
            var mainSkyCam = FindCamera("Camera ScaledSpace");

            partScaledCamera.CopyFrom(mainSkyCam);
            partScaledCamera.name = "jrScaled";


            partScaledCamera.transform.parent = mainSkyCam.transform;
            partScaledCamera.transform.localRotation = Quaternion.identity;
            partScaledCamera.transform.localPosition = Vector3.zero;
            partScaledCamera.transform.localScale = Vector3.one;
            partScaledCamera.targetTexture = TargetCamRenderTexture;
            _cameras[1] = partScaledCamera;
            var camRotator = cam2Obj.AddComponent<TgpCamRotator>();
            camRotator.NearCamera = partNearCamera;


            //galaxy camera
            var galaxyCamObj = new GameObject();
            var galaxyCam = galaxyCamObj.AddComponent<Camera>();
            var mainGalaxyCam = FindCamera("GalaxyCamera");

            galaxyCam.CopyFrom(mainGalaxyCam);
            galaxyCam.name = "jrGalaxy";
            galaxyCam.transform.parent = mainGalaxyCam.transform;
            galaxyCam.transform.position = Vector3.zero;
            galaxyCam.transform.localRotation = Quaternion.identity;
            galaxyCam.transform.localScale = Vector3.one;
            galaxyCam.fieldOfView = 60;
            galaxyCam.targetTexture = TargetCamRenderTexture;
            _cameras[2] = galaxyCam;
            var camRotatorgalaxy = galaxyCamObj.AddComponent<TgpCamRotator>();
            camRotatorgalaxy.NearCamera = partNearCamera;

            for (var i = 0; i < _cameras.Length; i++) _cameras[i].enabled = false;
        }


        public void CreateGui()
        {
            if (_hullcamera == null || _hullcamera.vessel == null)
            {
                Disable();
                return;
            }

            if (!Enabled) return;

            _windowRect = GUI.Window(_id, _windowRect, WindowTargetCam,
                _hullcamera.vessel.GetDisplayName() + "." + _hullcamera.cameraName);
        }

        public void CheckIfResizing()
        {
            if (!Enabled) return;

            if (Event.current.type == EventType.MouseUp)
                if (ResizingWindow)
                    ResizingWindow = false;
        }

        private void WindowTargetCam(int windowId)
        {
            if (!Enabled) return;

            var windowScale = TargetWindowScale;
            _adjCamImageSize = camImageSize * windowScale;


            WindowIsOpen = true;

            GUI.DragWindow(new Rect(0, 0, _windowHeight - 18, 30));
            if (GUI.Button(new Rect(_windowWidth - 18, 2, 16, 16), "X", GUI.skin.button))
            {
                Disable();


                return;
            }

            var imageRect = new Rect(2, 20, _adjCamImageSize, _adjCamImageSize);


            GUI.DrawTexture(imageRect, TargetCamRenderTexture, ScaleMode.StretchToFill, false);

            GUIStyle dataStyle = new GUIStyle();
            dataStyle.alignment = TextAnchor.MiddleCenter;
            dataStyle.normal.textColor = Color.white;
            dataStyle.fontStyle = FontStyle.Bold;

            //target data
            dataStyle.fontSize = (int)Mathf.Clamp(16 * TargetWindowScale, 9, 16);
            //float dataStartX = stabilStartX + stabilizeRect.width + 8;
            Rect targetRangeRect = new Rect(imageRect.x, (_adjCamImageSize * 0.94f) - (int)Mathf.Clamp(18 * TargetWindowScale, 9, 18), _adjCamImageSize, (int)Mathf.Clamp(18 * TargetWindowScale, 10, 18));
            
            float  altitudeInKm = (float) Math.Round(this._hullcamera.vessel.altitude / 1000f, 1);
            int speed = (int) Math.Round(this._hullcamera.vessel.speed * 3.6f,0);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ALTITUDE: " + altitudeInKm.ToString("0.0") + " KM");
            sb.AppendLine("SPEED: " + speed + " KM/H");

            GUI.Label(targetRangeRect, sb.ToString(), dataStyle);

            //resizing
            var resizeRect =
                new Rect(_windowWidth - 18, _windowHeight - 18, 16, 16);


            GUI.DrawTexture(resizeRect, ResizeTexture, ScaleMode.StretchToFill, true);

            if (Event.current.type == EventType.MouseDown && resizeRect.Contains(Event.current.mousePosition))
                ResizingWindow = true;

            if (Event.current.type == EventType.Repaint && ResizingWindow)
                if (Math.Abs(Mouse.delta.x) > 1 || Math.Abs(Mouse.delta.y) > 0.1f)
                {
                    var diff = Mouse.delta.x + Mouse.delta.y;
                    UpdateTargetScale(diff);
                    ResizeTargetWindow();
                }
            
            //ResetZoomKeys();
            RepositionWindow(ref _windowRect);
        }

        private void UpdateTargetScale(float diff)
        {
            var scaleDiff = diff / (_windowRect.width + _windowRect.height) * 100 * .01f;
            TargetWindowScale += Mathf.Abs(scaleDiff) > .01f ? scaleDiff : scaleDiff > 0 ? .01f : -.01f;

            TargetWindowScale += Mathf.Abs(scaleDiff) > .01f ? scaleDiff : scaleDiff > 0 ? .01f : -.01f;
            TargetWindowScale = Mathf.Clamp(TargetWindowScale,
                TargetWindowScaleMin,
                TargetWindowScaleMax);
        }


        private void ResizeTargetWindow()
        {
            _windowWidth = camImageSize * TargetWindowScale + 3 * buttonHeight + 16 + 2 * gap;
            _windowHeight = camImageSize * TargetWindowScale + 23;
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
            for (var i = _cameras.Length - 1; i >= 0; i--)
            {
                if (_cameras[i] == null) return;

                if (i > 0) _cameras[i].fieldOfView = 60;

                _cameras[i].Render();
            }
        }

        public void Disable()
        {
            Core.TrackedCameras.Remove(_id);
            Enabled = false;

            foreach (var camera in _cameras)
            {
                if (camera == null) continue;

                camera.enabled = false;
            }

            _cameras = null;
        }
    }
}