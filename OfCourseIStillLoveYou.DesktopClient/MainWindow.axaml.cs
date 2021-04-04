using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using OfCourseIStillLoveYou.Communication;
using System;
using System.Threading.Tasks;

namespace OfCourseIStillLoveYou.DesktopClient
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            GrpcClient.ConnectToServer();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            Task.Run(CameraFetchWorker);

        }

     

        private void CameraFetchWorker()
        {
            while (true)
            {
                Task.Delay(1000).Wait();

                var cameraIds = GrpcClient.GetCameraIds();
                var cbCameras = this.FindControl<ComboBox>("cbCameras");
                cbCameras.Items = cameraIds;

            }
        }
    }
}
