using UnityEngine;
using UnityEditor;

namespace usky
{
	[CustomEditor(typeof(uSkyFogGradient))]
	public class uSkyFogGradientEditor : Editor 
	{
		SerializedObject	serObj;
		SerializedProperty	fogGradient;

		private void OnEnable () 
		{
			serObj		= new SerializedObject (target);
			fogGradient	= serObj.FindProperty ("FogColor");
		}

		public override void OnInspectorGUI()
		{
			serObj.Update ();
			EditorGUILayout.PropertyField (fogGradient);
			serObj.ApplyModifiedProperties();
		}
	}
}
