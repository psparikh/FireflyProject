using UnityEngine;
using System.Reflection;
using UnityEditor;

namespace uSky
{
	[CustomEditor(typeof(DistanceCloud))]
	public class uSkyCloudInspector : Editor {

		DistanceCloud m_DC;
		Editor currentMaterialEditor;
		Editor m_tmpEditor;
		SerializedObject serObj;
		SerializedProperty mat;
//		SerializedProperty nightBrightness;
		SerializedProperty layer;
		SerializedProperty renderCloudsDome;

//		private bool showMat = true; // true means assign as default foldout PreDrop

		private void OnEnable () {
			serObj = new SerializedObject (target);
			mat = serObj.FindProperty ("CloudMaterial");
//			nightBrightness = serObj.FindProperty ("NightBrightness");
			layer = serObj.FindProperty ("cloudLayer");
			renderCloudsDome = serObj.FindProperty ("RenderCloudsDome");
		}

		public override void OnInspectorGUI (){  

//			DrawDefaultInspector ();

			serObj.Update ();
			EditorGUILayout.PropertyField (mat, new GUIContent ("Cloud Material", "Put Cloud material here."));
//			nightBrightness.floatValue = EditorGUILayout.Slider (new GUIContent ("Night Brightness","Controls the brightness of night clouds."),nightBrightness.floatValue, 0f, 1f);
			layer.intValue = EditorGUILayout.IntField("Layer",layer.intValue);
			renderCloudsDome.boolValue = EditorGUILayout.Toggle (new GUIContent ("Render Clouds Dome","You can use both distance clouds dome and distance clouds plane, or just use distance clouds plane only by turning off this option"),renderCloudsDome.boolValue);
			serObj.ApplyModifiedProperties ();

			m_DC = (DistanceCloud)target;
			Material m_Mat = m_DC.CloudMaterial;

			EditorGUI.BeginChangeCheck ();						

			InitEditor (m_Mat);

			currentMaterialEditor = m_tmpEditor;
				if (EditorGUI.EndChangeCheck ()) {
						InitEditor (m_Mat);
					}

			// draw material setting 
//			if (tmpEditor != null && m_Mat != null && showMat) {		// for Titlebar
			if (currentMaterialEditor != null && m_Mat != null) {		// for DrawHeader
					currentMaterialEditor.DrawHeader ();
					currentMaterialEditor.OnInspectorGUI ();
			}

		}

		void InitEditor (Material m_Mat){
			if (m_Mat != null ) {
				m_tmpEditor = CreateEditor (m_Mat);
			} 

			if (currentMaterialEditor != null) {
				DestroyImmediate (currentMaterialEditor);
			}
		}
	}
}