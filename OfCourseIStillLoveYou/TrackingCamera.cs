using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HullcamVDS;
using OfCourseIStillLoveYou.Client;
using OfCourseIStillLoveYou.TUFX;
using UnityEngine;
using UnityEngine.Rendering;

namespace OfCourseIStillLoveYou
{
    public class TrackingCamera
    {
        private static readonly float buttonHeight = 18;
        private static readonly float gap = 2;
        private static readonly float controlsStartY = 22;
        private static readonly Font TelemetryFont = Font.CreateDynamicFontFromOSFont("Bahnschrift Semibold", 17);

        private static readonly GUIStyle TelemetryGuiStyle = new GUIStyle()
            {alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() {textColor = Color.white}, fontStyle = FontStyle.Bold, font = TelemetryFont };
    



        public static Texture2D ResizeTexture =
            GameDatabase.Instance.GetTexture("OfCourseIStillLoveYou/Textures/" + "resizeSquare", false);

        private readonly MuMechModuleHullCamera _hullcamera;
        private readonly float camImageSize = 360;
        private float _adjCamImageSize = 360;


        private readonly Camera[] _cameras = new Camera[3];
        private float _windowHeight;

        private Rect _windowRect;
        private float _windowWidth;

        private readonly WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
        private readonly WaitForSeconds fixedDelay = new WaitForSeconds(0.030f);
        private byte[] jpgTexture;
        public RenderTexture TargetCamRenderTexture;
        private readonly Texture2D texture2D = new Texture2D(768, 768, TextureFormat.ARGB32, false);

        

        public TrackingCamera(int id, MuMechModuleHullCamera hullcamera)
        {
            Id = id;
            _hullcamera = hullcamera;

            TargetCamRenderTexture = new RenderTexture(768, 768, 24, RenderTextureFormat.ARGB32);

            TargetCamRenderTexture.antiAliasing = 1;
            TargetCamRenderTexture.Create();
            _windowWidth = _adjCamImageSize + 3 * buttonHeight + 16 + 2 * gap;
            _windowHeight = _adjCamImageSize + 23;
            _windowRect = new Rect(Screen.width - _windowWidth, Screen.height - _windowHeight, _windowWidth,
                _windowHeight);
            SetCameras();

            Enabled = true;
        }

        public string Name { get; private set; }

        public Vessel Vessel => _hullcamera?.vessel;

        public int Id { get; }

        public bool Enabled { get; set; }

        public float TargetWindowScaleMax { get; set; } = 3f;

        public float TargetWindowScaleMin { get; set; } = 0.5f;


        public bool ResizingWindow { get; set; }

        public bool WindowIsOpen { get; set; }

        public float TargetWindowScale { get; set; } = 1;
        public string AltitudeString { get; private set; }
        public string SpeedString { get; private set; }
        public bool StreamingEnabled { get; private set; }

        private Camera FindCamera(string cameraName)
        {
            foreach (var cam in Camera.allCameras)
                if (cam.name == cameraName)
                    return cam;

            Debug.Log("Couldn't find " + cameraName);
            return null;
        }

        public IEnumerator SendCameraImage()
        {
            while (Enabled)
            {
                yield return fixedDelay;

                if (!Enabled) yield return null;

                RenderCameras();

                yield return frameEnd;

                if (!StreamingEnabled) continue;

                Graphics.CopyTexture(TargetCamRenderTexture, texture2D);

                AsyncGPUReadback.Request(texture2D, 0,
                    request =>
                    {
                        Task.Run(() => texture2D.LoadRawTextureData(request.GetData<byte>()))
                            .ContinueWith(previous => jpgTexture = texture2D.EncodeToJPG())
                            .ContinueWith(previous =>
                                GrpcClient.SendCameraTextureAsync(new CameraData
                                {
                                    CameraId = Id.ToString(),
                                    CameraName = Name,
                                    Speed = SpeedString,
                                    Altitude = AltitudeString,
                                    Texture = jpgTexture
                                }));
                    }
                );
            }
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
            partNearCamera.fieldOfView = 50;
            partNearCamera.targetTexture = TargetCamRenderTexture;
            _cameras[0] = partNearCamera;

            
            //TUFX
            AddTufxPostProcessing();

             var cam2Obj = new GameObject();
            var partScaledCamera = cam2Obj.AddComponent<Camera>();
            var mainSkyCam = FindCamera("Camera ScaledSpace");

            partScaledCamera.CopyFrom(mainSkyCam);
            partScaledCamera.name = "jrScaled";


            partScaledCamera.transform.parent = mainSkyCam.transform;
            partScaledCamera.transform.localRotation = Quaternion.identity;
            partScaledCamera.transform.localPosition = Vector3.zero;
            partScaledCamera.transform.localScale = Vector3.one;
            partScaledCamera.fieldOfView = 50;
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
            galaxyCam.fieldOfView = 50;
            galaxyCam.targetTexture = TargetCamRenderTexture;
            _cameras[2] = galaxyCam;

            var camRotatorgalaxy = galaxyCamObj.AddComponent<TgpCamRotator>();
            camRotatorgalaxy.NearCamera = partNearCamera;

            for (var i = 0; i < _cameras.Length; i++) _cameras[i].enabled = false;
        }


        private void AddTufxPostProcessing()
        {
            try
            {
                TUFXWrapper.AddPostProcessing(_cameras[0]);
            }
            catch
            {
                // ignored
            }
        }


        public void CreateGui()
        {
            if (!Enabled) return;

            if (_hullcamera == null || _hullcamera.vessel == null)
            {
                Disable();
                return;
            }

            Name = _hullcamera.vessel.GetDisplayName() + "." + _hullcamera.cameraName;

            _windowRect = GUI.Window(Id, _windowRect, WindowTargetCam,
                Name);
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
            if (GUI.Button(new Rect(_windowWidth - 18, 2, 20, 16), "X", GUI.skin.button))
            {
                Disable();


                return;
            }

            var imageRect = DrawTexture();

            // Right side control buttons
            DrawSideControlButtons(imageRect);

            DrawTelemetry(imageRect);


            //resizing
            var resizeRect =
                new Rect(_windowWidth - 18, _windowHeight - 18, 16, 16);


            GUI.DrawTexture(resizeRect, ResizeTexture, ScaleMode.StretchToFill, true);

            if (Event.current.type == EventType.MouseDown && Event.current.clickCount == 2 && imageRect.Contains(Event.current.mousePosition))
            {
                MinimalUI = !MinimalUI;
                ResizeTargetWindow();
            }

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

        private Rect DrawTexture()
        {
            var imageRect = new Rect(2, 20, _adjCamImageSize, _adjCamImageSize);


            GUI.DrawTexture(imageRect, TargetCamRenderTexture, ScaleMode.StretchToFill, false);
            return imageRect;
        }

        private void DrawTelemetry(Rect imageRect)
        {
            if (MinimalUI) return;

            var dataStyle = new GUIStyle(TelemetryGuiStyle)
            {
                fontSize = (int) Mathf.Clamp(16 * TargetWindowScale, 9, 17),
            };

            var targetRangeRect = new Rect(imageRect.x,
                _adjCamImageSize * 0.94f - (int) Mathf.Clamp(18 * TargetWindowScale, 9, 18), _adjCamImageSize,
                (int) Mathf.Clamp(18 * TargetWindowScale, 10, 18));

            var sb = new StringBuilder();
            sb.AppendLine(AltitudeString);
            sb.AppendLine(SpeedString);

            GUI.Label(targetRangeRect, sb.ToString(), dataStyle);
        }

        public bool MinimalUI { get; set; } = false;

        private void DrawSideControlButtons(Rect imageRect)
        {
            if (MinimalUI) return;

            var buttonStyle = new GUIStyle(HighLogic.Skin.button);
            buttonStyle.fontSize = 10;
            buttonStyle.wordWrap = true;

            var line = buttonHeight + gap;
            var buttonWidth = 3 * buttonHeight + 4 * gap;
            //groundStablize button
            var startX = imageRect.width + 3 * gap;
            var streamingRect = new Rect(startX, controlsStartY, buttonWidth, buttonHeight + line);

            if (!StreamingEnabled)
            {
                if (GUI.Button(streamingRect, "Enable streaming", buttonStyle)) StreamingEnabled = true;
            }
            else
            {
                if (GUI.Button(streamingRect, "Disable streaming", buttonStyle)) StreamingEnabled = false;
            }
        }

        public void CalculateSpeedAltitude()
        {
            var altitudeInKm = (float) Math.Round(_hullcamera.vessel.altitude / 1000f, 1);
            var speed = (int) Math.Round(_hullcamera.vessel.speed * 3.6f, 0);
            AltitudeString = "ALTITUDE: " + altitudeInKm.ToString("0.0") + " KM";
            SpeedString = "SPEED: " + speed + " KM/H";
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
            if (MinimalUI)
            {
                _windowWidth = camImageSize * TargetWindowScale + 2 * gap;
            }
            else
            {
                _windowWidth = camImageSize * TargetWindowScale + 3 * buttonHeight + 16 + 2 * gap;
            }
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

        private void RenderCameras()
        {
            for (var i = _cameras.Length - 1; i >= 0; i--)
            {
                if (_cameras[i] == null) return;

                _cameras[i].Render();
            }
        }

        public void Disable()
        {
            Enabled = false;
            StreamingEnabled = false;
            this.TargetCamRenderTexture.Release();

            foreach (var camera in _cameras)
                if (camera != null)
                    camera.enabled = false;
        }
    }
}