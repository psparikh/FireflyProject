using UnityEngine;
using UnityEngine.UI;
using System.Text;

namespace usky
{
	[AddComponentMenu("uSkyPro/Other/Display Camera Altitude UI")]
	public class DisplayCameraAltitudeUI : MonoBehaviour {

		public Text AltitudeText;
		public Transform PlayerCamera = null;

		// Update is called once per frame
		void Update () {
			if (!AltitudeText)
				return;

			if (PlayerCamera) {
				float value = PlayerCamera.transform.position.y;

				StringBuilder strAltitude = new StringBuilder ();
				strAltitude.Append ("Camera Height");
				strAltitude.Append ("\n");
				strAltitude.Append (value.ToString ("####"));
				strAltitude.Append (" Meter");
				AltitudeText.text = strAltitude.ToString ();
			} else {
				AltitudeText.text = string.Empty;
			}
		}
	}
}