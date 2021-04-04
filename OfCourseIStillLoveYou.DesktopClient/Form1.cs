using OfCourseIStillLoveYou.Communication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfCourseIStillLoveYou.DesktopClient
{
    public partial class FormMain : Form
    {
        private static Image cameraTexture = null;

        public CameraData currentCamaraData { get; set; }
        public string currentCamera { get; private set; }


        private int FPS = 30;

        private bool connectedToServer = false;

        public int GetPerFrameDelay()
        {
            return (int)Math.Round((decimal)(1000 / FPS), 0);
        }

        public FormMain()
        {
            InitializeComponent();

            toolStripStatusLabel1.Text = "Connecting to server...";

            try
            {
                GrpcClient.ConnectToServer();
                Task.Run(FetchTextureWorker);
                connectedToServer = true;
                toolStripStatusLabel1.Text = "Connection to server successful";
            }
            catch
            {
                toolStripStatusLabel1.Text = "Connection to server unsuccessful";
            }
        }

        private void FetchTextureWorker()
        {
            while (true)
            {
                Task.Delay(GetPerFrameDelay()).Wait();

                if (!connectedToServer) continue;
                
                if (string.IsNullOrEmpty(currentCamera)) continue;

                try
                {
                    GrpcClient.GetCameraDataAsync(currentCamera).ContinueWith((previous) =>
                           {
                               this.Name = previous.Result.ToString();
                               currentCamaraData = previous.Result;
                               loadImage(previous.Result.Texture);
                           });
                }
                catch (Exception)
                {
                    Disconnected();
                }
            }
        }

        private void loadImage(byte[] texture)
        {
            if(texture == null)
            {
                cameraTexture = null;
                return;
            }

            using (MemoryStream ms = new(texture))
            cameraTexture = Image.FromStream(ms);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null) return;

            currentCamera = comboBox1.SelectedItem.ToString();
        }

        private void timerCameras_Tick(object sender, EventArgs e)
        {
            try
            {
                var cameras = GrpcClient.GetCameraIds();

                comboBox1.Items.Clear();

                foreach (string cameraId in cameras)
                {
                    if (!comboBox1.Items.Contains(cameraId))
                    {
                        comboBox1.Items.Add(cameraId);
                    }
                }

                if (!comboBox1.Items.Contains(currentCamera))
                {
                    pictureBox1.Image = null;
                }

                GrpcClient.GetCurrentFPSAsync().ContinueWith((newfps) =>
                {
                    FPS = Math.Max(newfps.Result, 30);
                });

                Connected();
            }
            catch (Exception)
            {
                Disconnected();
            }
        }

        private void Disconnected()
        {
            connectedToServer = false;
            toolStripStatusLabel1.Text = "Connection to server unsuccessful";
        }

        private void Connected()
        {
            connectedToServer = true;
            toolStripStatusLabel1.Text = "Connection to server successful";
        }

        private void timerCameraTexture_Tick(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count == 0) return;
            if (String.IsNullOrEmpty(currentCamera)) return;
            if (this.currentCamaraData?.Texture == null) return;

      
            this.pictureBox1.Image = cameraTexture;

            this.labelSpeed.Text = this.currentCamaraData.Speed;
            this.labelAltitude.Text = this.currentCamaraData.Altitude;
        }
    }
}
