using UnityEngine;
using System.Collections;
using UnityEditor;

namespace uSky
{
	[CustomEditor(typeof(uSkyFog))]
	public class uSkyFogInspector : Editor {

		uSkyFog m_uSF;
		SerializedObject serObj;
		SerializedProperty mode;
		SerializedProperty radial;
		SerializedProperty scattering;
		SerializedProperty density;
		SerializedProperty colorDecay;
		SerializedProperty horizonOffset;
		SerializedProperty startDistance;
		SerializedProperty endDistance;

		private void OnEnable () {
			serObj = new SerializedObject (target);
			mode = serObj.FindProperty ("fogMode");
			radial = serObj.FindProperty ("useRadialDistance");
			scattering = serObj.FindProperty ("Scattering");
			density = serObj.FindProperty ("Density");
			colorDecay = serObj.FindProperty ("ColorDecay");
			horizonOffset = serObj.FindProperty ("HorizonOffset");
			startDistance = serObj.FindProperty ("StartDistance");
			endDistance = serObj.FindProperty ("EndDistance");
		}

		public override void OnInspectorGUI (){ 
			serObj.Update ();
			EditorGUILayout.PropertyField (mode, new GUIContent ("Fog Mode", "The way in which the fogging accumulates with distance from the camera"));
			EditorGUILayout.PropertyField (radial, new GUIContent ("Use Radial Distance?", "Distance fog is based on radial distance from camera when checked"));

			if (mode.enumValueIndex == 0) {
				EditorGUILayout.PropertyField (startDistance, new GUIContent ("Start Distance", "Push fog away from the camera by this amount"));
				EditorGUILayout.PropertyField (endDistance, new GUIContent ("End Distance", "The distance from camera at which the fog completely obscures scene objects."));
			}
			if (mode.enumValueIndex != 0)
				EditorGUILayout.PropertyField (density, new GUIContent ("Density", "Distance fog amount"));
			EditorGUILayout.PropertyField (colorDecay, new GUIContent ("Color Decay", "Shifting the scattering color between horizon and zenith."));
			EditorGUILayout.PropertyField (scattering, new GUIContent ("Scattering", "How much light will scattering through the occlusion, It will be useful to lower this value when the sun is behind the mountain"));
			EditorGUILayout.PropertyField (horizonOffset, new GUIContent ("Horizon Offset", "Simulate the viewer or camera height level offset. Create a curve shape effect of the horizon that can be match the far away ground or ocean edge"));

			serObj.ApplyModifiedProperties ();

		}
	}
}