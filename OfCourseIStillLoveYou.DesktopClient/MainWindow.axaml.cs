using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Visuals.Media.Imaging;
using OfCourseIStillLoveYou.Communication;

namespace OfCourseIStillLoveYou.DesktopClient
{
    public class MainWindow : Window
    {
        private const int Delay = 30;
        private const string SettingPath = "settings.json";
        private const string Endpoint = "localhost";
        private const int Port = 5077;
        private string? currentCamera;

        private Bitmap? initialImage;

        private SettingsPoco? settings;

        private bool statusUnstable;
        private Bitmap? texture;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public CameraData? CurretCameraData { get; private set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            StoreInitialImage();
            ReadSettings();

            if (settings != null) GrpcClient.ConnectToServer(settings.EndPoint, settings.Port);

            Task.Run(CameraFetchWorker);
            Task.Run(CameraTextureWorker);
        }

        private void ReadSettings()
        {
            try
            {
                var settingsText = File.ReadAllText(SettingPath);
                settings = JsonSerializer.Deserialize<SettingsPoco>(settingsText);
            }
            catch (Exception)
            {
                settings = new SettingsPoco {EndPoint = Endpoint, Port = Port};
            }
        }

        private void StoreInitialImage()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
                initialImage = (Bitmap) this.FindControl<Image>("imgCameraTexture").Source);
        }

        private void CameraTextureWorker()
        {
            while (true)
            {
                Task.Delay(Delay).Wait();

                if (string.IsNullOrEmpty(currentCamera)) continue;


                Dispatcher.UIThread
                    .InvokeAsync(() => this.FindControl<Image>("imgCameraTexture").DesiredSize.Height)
                    .ContinueWith(imageHeight =>
                    {
                        GrpcClient.GetCameraDataAsync(currentCamera).ContinueWith(camaraData =>
                        {
                            if (camaraData.Result.Texture == null)
                            {
                                texture = initialImage;
                            }
                            else
                            {
                                using MemoryStream ms = new(camaraData.Result.Texture);
                                texture = Bitmap.DecodeToHeight(ms, (int) imageHeight.Result,
                                    BitmapInterpolationMode.MediumQuality);
                            }

                            CurretCameraData = camaraData.Result;
                        }).ContinueWith(_ =>
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                this.FindControl<Image>("imgCameraTexture").Source = texture;

                                if (CurretCameraData?.Texture == null)
                                {
                                    statusUnstable = true;
                                }
                                else
                                {
                                    statusUnstable = false;

                                    TextBlock textInfo = this.FindControl<TextBlock>("TextInfo");
                                    StringBuilder sb = new();
                                    sb.AppendLine(CurretCameraData.Altitude);
                                    sb.AppendLine(CurretCameraData.Speed);
                                    textInfo.Text = sb.ToString();

                                    Window window = this.FindControl<Window>("MainWindow");
                                    window.Title = CurretCameraData.CameraName;
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

                    if (statusUnstable)
                    {
                        Dispatcher.UIThread.InvokeAsync(NotifyUnstableCameraFeed);
                    }
                    else if ((cameraIds == null || cameraIds.Count == 0))
                    {
                        Dispatcher.UIThread.InvokeAsync(NotifyWaitingForCameraFeed);
                    }
                    
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (cameraIds != null) UpdateCameraList(cameraIds);
                    });

                    Dispatcher.UIThread.InvokeAsync(GetSelectedCamera).ContinueWith(selectedCamera =>
                    {
                        currentCamera = selectedCamera.Result;
                    });

                }
                catch (Exception)
                {
                    Dispatcher.UIThread.InvokeAsync(NotifyConnectingToServer);
                }
            }
        }

        private string? GetSelectedCamera()
        {
            var cbCameras = this.FindControl<ComboBox>("cbCameras");
            return cbCameras.SelectedItem == null ? string.Empty : cbCameras.SelectedItem.ToString();
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

        private void UpdateCameraList(List<string?> cameraIds)
        {
            var cbCameras = this.FindControl<ComboBox>("cbCameras");
            cbCameras.Items = cameraIds;

            if (cbCameras.SelectedItem != null && !cameraIds.Contains(cbCameras.SelectedItem.ToString()))
            {
                cbCameras.SelectedItem = "";
            }

        }

        private void ImgCameraTexture_OnDoubleTapped(object? sender, RoutedEventArgs e)
        {

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var textInfo = this.FindControl<TextBlock>("TextInfo");
                var cbCameras = this.FindControl<ComboBox>("cbCameras");
                var labelCameras = this.FindControl<Label>("labelCameras");

                labelCameras.IsVisible = !labelCameras.IsVisible;
                textInfo.IsVisible = !textInfo.IsVisible;
                cbCameras.IsVisible = !cbCameras.IsVisible;
            });
        }
    }
}