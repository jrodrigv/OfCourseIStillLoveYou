using TUFX;
using TUFX.PostProcessing;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace OfCourseIStillLoveYou.TUFX
{
    public static class TUFXWrapper
    {
        public static void AddPostProcessing (Camera c)
        {
           
            var layer = c.gameObject.AddOrGetComponent<PostProcessLayer>();
            layer.Init(TexturesUnlimitedFXLoader.Resources);

            layer.volumeLayer = ~0;
            var volume = c.gameObject.AddOrGetComponent<PostProcessVolume>();
            volume.isGlobal = true;
            volume.priority = 100;
        }
        
    }
}
