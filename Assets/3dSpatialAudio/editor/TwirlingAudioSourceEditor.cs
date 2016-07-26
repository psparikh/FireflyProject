#define CUSTOM_LAYOUT

using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;


[CustomEditor(typeof(TwirlingSPAudioSource))]


public class TwirlingSpatializerUserParamsEditor : Editor
{
	// target component
	private TwirlingSPAudioSource m_Component;

	// OnEnable
	void OnEnable()
	{
		m_Component = (TwirlingSPAudioSource)target;
	}
	
	// OnDestroy
	void OnDestroy()
	{
	}
	
	// OnInspectorGUI
	public override void OnInspectorGUI()
	{
		GUI.color = Color.white;
		Undo.RecordObject(m_Component, "TwirlingSpatializerUserParams");
		
		{
			#if CUSTOM_LAYOUT

			m_Component.EnableSpatialization = EditorGUILayout.Toggle("Enable Spatialization", m_Component.EnableSpatialization);
			m_Component.Reverb  = (TwirlingSPAudioSource.options) EditorGUILayout.EnumPopup("Reverberation", m_Component.Reverb);
			m_Component.VirtualSpk  = EditorGUILayout.Toggle("Speaker Virtualizer", m_Component.VirtualSpk);


			#else			 
			DrawDefaultInspector ();
			#endif
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty(m_Component);
		}
	}	
	

}

