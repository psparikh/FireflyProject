using UnityEngine;
using UnityEditor;

namespace usky
{
	[CustomEditor(typeof(uSkyReflectionProbeUpdater))]
	public class uSkyReflectionProbeUpdaterEditor : Editor 
	{
		SerializedObject	serObj;
		SerializedProperty	refreshMode;
		
		private void OnEnable () 
		{
			serObj		= new SerializedObject (target);
			refreshMode	= serObj.FindProperty ("RuntimeRefreshMode");
		}

		public override void OnInspectorGUI()
		{
			serObj.Update ();
			EditorGUILayout.PropertyField (refreshMode);
			serObj.ApplyModifiedProperties();
		}
	}
}
