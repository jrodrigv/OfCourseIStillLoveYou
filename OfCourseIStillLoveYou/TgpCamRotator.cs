using UnityEngine;

namespace OfCourseIStillLoveYou
{
    public class TgpCamRotator : MonoBehaviour
    {

        public Camera NearCamera { get; set; } = new Camera();
        
        void OnPreRender()
        {
            if (NearCamera == null) return;
            if (NearCamera.transform == null) return;
            if (transform == null) return;

            transform.position = ScaledSpace.LocalToScaledSpace((Vector3d)NearCamera.transform.localPosition);
            transform.rotation = NearCamera.transform.rotation;
        }
    }
}
