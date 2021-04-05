using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using OfCourseIStillLoveYou.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OfCourseIStillLoveYou.DesktopClient
{
    public class MainWindow : Window
    {
        private Bitmap initialImage;
        private string currentCamera;
        private Bitmap texture;

        public CameraData curretCameraData { get; private set; }

        public MainWindow()
       {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif        
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            StoreInitialImage();
            GrpcClient.ConnectToServer();
            Task.Run(CameraFetchWorker);
            Task.Run(CameraTextureWorker);
        }

        private void StoreInitialImage()
        {
            Dispatcher.UIThread.InvokeAsync(() => initialImage = (Bitmap)this.FindControl<Image>("imgCameraTexture").Source);
        }

        private void CameraTextureWorker()
        {
            while (true)
            {
                Task.Delay(33).Wait();

                if (String.IsNullOrEmpty(currentCamera))
                {
                    continue;
                }
                
                
                Dispatcher.UIThread.InvokeAsync<double>(() =>
                {
                    return this.FindControl<Image>("imgCameraTexture").DesiredSize.Height;
                }).ContinueWith((imageHeight) =>
                {
                    GrpcClient.GetCameraDataAsync(currentCamera).ContinueWith((camaraData) =>
                    {
                        if(camaraData.Result.Texture == null)
                        {
                            texture = initialImage;
                        }
                        else
                        {
                            using MemoryStream ms = new(camaraData.Result.Texture);
                            texture = Bitmap.DecodeToHeight(ms, (int)imageHeight.Result, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.MediumQuality);
                        }
                        this.curretCameraData = camaraData.Result;

                    }).ContinueWith((newTexture) => 
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        this.FindControl<Image>("imgCameraTexture").Source = texture;
                        
                        if(curretCameraData.Texture == null)
                        {
                            NotifyUnstableCameraFeed();
                        }
                        else
                        {
                            TextBlock textInfo = this.FindControl<TextBlock>("TextInfo");
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine(curretCameraData.Altitude);
                            sb.AppendLine(curretCameraData.Speed);
                            textInfo.Text = sb.ToString();
                        }
                    }));
                });  
            }
        }

        private void CameraFetchWorker()
        {
            while (true)
            {
                Task.Delay(1000).Wait();
                try
                {
                    var cameraIds = GrpcClient.GetCameraIds();
                    if(cameraIds == null || cameraIds.Count == 0)
                    {
                        Dispatcher.UIThread.InvokeAsync(() => NotifyWaitingForCameraFeed());
                        continue;
                    }

                    Dispatcher.UIThread.InvokeAsync<string>(() => GetSelectedCamera()).ContinueWith((selectedCamera) => { currentCamera = selectedCamera.Result; });

                    Dispatcher.UIThread.InvokeAsync(() => UpdateCameraList(cameraIds));
                }
                catch (Exception ex)
                {
                    Dispatcher.UIThread.InvokeAsync(() => NotifyConnectingToServer());
                }
            }
        }

        private string GetSelectedCamera()
        {
            var cbCameras = this.FindControl<ComboBox>("cbCameras");
            return cbCameras.SelectedItem == null? String.Empty:cbCameras.SelectedItem?.ToString();
        }

        private void NotifyWaitingForCameraFeed()
        {
            TextBlock textInfo = this.FindControl<TextBlock>("TextInfo");
            textInfo.Text = "Waiting for camera feed...";
        }

        private void NotifyUnstableCameraFeed()
        {
            TextBlock textInfo = this.FindControl<TextBlock>("TextInfo");
            textInfo.Text = "VIDEO CONNECTION HAS BEEN LOST. RIP";
        }

        private void NotifyConnectingToServer()
        {
            TextBlock textInfo = this.FindControl<TextBlock>("TextInfo");
            textInfo.Text = "Connecting to server...";
        }

        private void UpdateCameraList(List<string> cameraIds)
        {
            var cbCameras = this.FindControl<ComboBox>("cbCameras");
            cbCameras.Items = cameraIds;    
        }
    }
}
