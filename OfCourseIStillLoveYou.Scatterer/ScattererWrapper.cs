using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using scatterer;
using UnityEngine;

namespace OfCourseIStillLoveYou.Scatterer
{
    public static class ScattererWrapper
    {
        public static void AddShadowFixToCamera(Camera c)
        {
            var partialUnifiedCameraDepthBuffer =
                (PartialDepthBuffer) c.gameObject.AddComponent(typeof(PartialDepthBuffer));
            partialUnifiedCameraDepthBuffer.Init(c);
        }
    }
}
