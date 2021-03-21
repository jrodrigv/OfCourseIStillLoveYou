using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scatterer;
using UnityEngine;

namespace OfCourseIStillLoveYou
{
	public class PartialDepthBuffer : MonoBehaviour
	{
		private Camera depthCamera;
		private GameObject depthCameraGO;

		public Camera targetCamera;

		public RenderTexture depthTexture;
		Shader depthShader;


		public void Init(Camera target)
		{
			depthCameraGO = new GameObject("ScattererPartialDepthBuffer");
			depthCamera = depthCameraGO.AddComponent<Camera>();
			targetCamera = target;
			depthCamera.CopyFrom(targetCamera);
			depthCamera.transform.position = targetCamera.transform.position;
			depthCamera.transform.parent = targetCamera.transform;

			depthCamera.farClipPlane = QualitySettings.shadowDistance;
			depthCamera.depthTextureMode = DepthTextureMode.None;
			depthCamera.enabled = false;
			depthCamera.clearFlags = CameraClearFlags.Depth;

			depthTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth); //we could almost get away with 16bit but it gets weird sometimes
			depthTexture.useMipMap = false;
			depthTexture.antiAliasing = 1; //no AA needed
			depthTexture.filterMode = FilterMode.Point;
			depthTexture.Create();
			depthShader = ShaderReplacer.Instance.LoadedShaders[("Scatterer/SimpleDepthTexture")]; //Don't use VertexLit, causes the camera to render shadowmaps
			depthCamera.SetReplacementShader(depthShader, "RenderType");
		}

		public void OnPreCull()
		{
			UpdateClipPlanes();

			depthCamera.targetTexture = depthTexture;
			depthCamera.RenderWithShader(depthShader, "RenderType"); //doesn't fire camera events (doesn't pick up EVE planetLight commandbuffers)

			Shader.SetGlobalMatrix(ShaderProperties.ScattererAdditionalInvProjection_PROPERTY, depthCamera.projectionMatrix.inverse);
			Shader.SetGlobalTexture(ShaderProperties.AdditionalDepthBuffer_PROPERTY, depthTexture);
		}

		// Adjusts nearClipPlane to cover minimum shadow Distance we are going for
		void UpdateClipPlanes()
		{
			depthCamera.farClipPlane = QualitySettings.shadowDistance;  //apparently this needs to be set again every frame or we get a projection
																		// matrix in plugin which doesn't match what is rendered
			depthCamera.fieldOfView = targetCamera.fieldOfView;

			Vector3 topLeft = depthCamera.ViewportPointToRay(new Vector3(0f, 1f, 0f)).direction; // Take a frustum corner and compute the angle to the camera forward direction
			topLeft.Normalize();
			float angle = Vector3.Dot(topLeft, depthCamera.transform.forward);
			depthCamera.nearClipPlane = 8000f * angle;
		}

		public void OnDestroy()
		{
			UnityEngine.Object.Destroy(depthCamera);
			GameObject.Destroy(depthCameraGO);
			depthTexture.Release();

		}
	}
}
