using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

namespace usky
{
	[AddComponentMenu("uSkyPro/Other/Display Time UI")]
	public class DisplayTimeUI : MonoBehaviour {

		public Text DisplayTimeText;
		uSkyTimeline uST {
			get {return uSkyTimeline.instance;}
		}
		
		// Update is called once per frame
		void Update () {

			if (!uST || !DisplayTimeText)
				return;

			TimeSpan t = TimeSpan.FromHours ((double) uST.Timeline); 

			StringBuilder strTime = new StringBuilder ();
			strTime.Append (t.Hours.ToString("D2"));
			strTime.Append (":");
			strTime.Append (t.Minutes.ToString("D2"));
			DisplayTimeText.text = strTime.ToString ();

		}
	}
}