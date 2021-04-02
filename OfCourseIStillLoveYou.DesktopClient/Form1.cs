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
    public partial class Form1 : Form
    {
        private static Image cameraTexture = null;

        public string currentCamera = "";

        public CameraData currentCamaraData { get; set; }

        public Form1()
        {
            InitializeComponent();
            GrpcClient.ConnectToServer();
            Task.Run(FetchTextureWorker);

            this.pictureBox1.Image = cameraTexture;
        }

        private void FetchTextureWorker()
        {
            while (true)
            {
                Task.Delay(33).Wait();
                
                if (string.IsNullOrEmpty(currentCamera)) continue;

                currentCamaraData = GrpcClient.GetCameraDataAsync(currentCamera);

                cameraTexture = byteArrayToImage(currentCamaraData.Texture);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentCamera = comboBox1.SelectedItem.ToString();
        }

        private void timerCameras_Tick(object sender, EventArgs e)
        {
            //var previousSelectedItem = comboBox1.SelectedIndex;

            var cameras = GrpcClient.GetCameraIds();

            comboBox1.Items.Clear();

            foreach (string cameraId in cameras)
            {
                if (!comboBox1.Items.Contains(cameraId))
                {
                    comboBox1.Items.Add(cameraId);
                }
            }
        }

        private void timerCameraTexture_Tick(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count == 0) return;
            if (String.IsNullOrEmpty(currentCamera)) return;
            if (cameraTexture == null) return;

            this.pictureBox1.Image = cameraTexture;
            this.labelSpeed.Text = this.currentCamaraData.Speed;
            this.labelAltitude.Text = this.currentCamaraData.Altitude;

            //this.pictureBox1.Refresh();
        }

        public Image byteArrayToImage(byte[] byteBLOBData)
        {
            MemoryStream ms = new MemoryStream(byteBLOBData);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
