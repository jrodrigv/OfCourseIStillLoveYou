using System.Reflection;
using UnityEngine;

namespace OfCourseIStillLoveYou
{
    public class CanvasHack : MonoBehaviour
    {
        static readonly FieldInfo canvasHackField = typeof(Canvas).GetField("willRenderCanvases", BindingFlags.NonPublic | BindingFlags.Static);
        private object _canvasHackObject;
        
        
        void OnPreRender()
        {
            this._canvasHackObject = canvasHackField.GetValue(null);
            canvasHackField.SetValue(null, null);
          
        }

        void OnPostRender()
        {
            canvasHackField.SetValue(null, _canvasHackObject);
        }
    }
}