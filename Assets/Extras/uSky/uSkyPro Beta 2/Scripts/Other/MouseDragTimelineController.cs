// This script controls the uSkyTimeline script in "Default" type only.
// Support Mouse Drag and Touch input for mobile devices.
// USAGE: This script can be applied to any gameObject.

using UnityEngine;
using UnityEngine.EventSystems;

namespace usky
{
	[AddComponentMenu("uSkyPro/Other/Mouse Drag Timeline Controller")]
	public class MouseDragTimelineController : MonoBehaviour {

		public float Speed = 1.0f;
		
		uSkyTimeline uST { get { return uSkyTimeline.instance; } }

		// set uSkyTimeline Mode to "Default"
		void Start () {
			if (uST)
				uST.Type = TimeSettingsMode.Default;
		}
		
		// Update is called once per frame
		void Update () {

			if (!uST)
				return;

			if ( EventSystem.current != null){
				if ( EventSystem.current.IsPointerOverGameObject())
					return;
			}
			// Touch input
			#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
			if ( Input.touchCount == 1 )  
			{	
				// Assume using landscape orientation on build
				uST.Timeline = uST.Timeline - Input.GetTouch(0).deltaPosition.y * Time.smoothDeltaTime * Speed * 0.25f;
				uST.SunDirection = uST.SunDirection + Input.GetTouch(0).deltaPosition.x * Time.smoothDeltaTime * Speed * 2f;
			}
			#else
			// Mouse input
			if ( Input.GetMouseButton (0))  
			{	
				uST.Timeline = uST.Timeline - Input.GetAxis ("Mouse Y") * Time.smoothDeltaTime * Speed ;
				uST.SunDirection = uST.SunDirection + Input.GetAxis ("Mouse X") * Time.smoothDeltaTime * Speed * 20f;
			}
			#endif 
		}
	}
}