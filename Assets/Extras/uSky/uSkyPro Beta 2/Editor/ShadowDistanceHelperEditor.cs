using UnityEngine;
using UnityEditor;

namespace usky
{
	[CustomEditor(typeof(ShadowDistanceHelper))]
	public class ShadowDistanceHelperEditor : Editor 
	{
		SerializedObject	serObj;
		SerializedProperty	shadowDistance;
		
		private void OnEnable () 
		{
			serObj			= new SerializedObject (target);
			shadowDistance	= serObj.FindProperty ("ShadowDistance");
		}

		public override void OnInspectorGUI()
		{
			serObj.Update ();
			EditorGUILayout.PropertyField (shadowDistance);
			serObj.ApplyModifiedProperties();
		}
	}
}
