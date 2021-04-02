using OfCourseIStillLoveYou.Client;
using System.Threading.Tasks;
using UnityEngine;
namespace OfCourseIStillLoveYou
{

   
    public class CameraStreaming : MonoBehaviour
    {

        public RenderTexture CameraTexture { get; set; }

        public int CameraId { get; set; }


        private Texture2D texture2D = new Texture2D(768, 768, TextureFormat.ARGB32, false);
    
        private byte[] jpgTexture;


        void OnPostRender()
        {
            //Graphics.CopyTexture(CameraTexture, texture2D);

            RenderTexture.active = CameraTexture;

            texture2D.ReadPixels(new Rect(0, 0, CameraTexture.width, CameraTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = null;

           


            //Texture2D tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
            //RenderTexture.active = rTex;
            //tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            //tex.Apply();

        }

    }
}