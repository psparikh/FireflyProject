using UnityEngine;
using UnityEditor;

namespace usky
{
	[CustomPropertyDrawer(typeof (HeaderLayoutAttribute))]
	public class HeaderLayoutDrawer : PropertyDrawer
	{
		private const float kHeadingSpace = 20.0f; // header height

		private Vector3[] totalVector3Count;

		static Styles m_Styles;
		
		private class Styles
		{
			public readonly GUIStyle header = "ShurikenModuleTitle";
			
			internal Styles()
			{
				header.font = (new GUIStyle("Label")).font;
				header.border = new RectOffset(15, 7, 4, 4);
				header.fixedHeight = kHeadingSpace;
				header.contentOffset = new Vector2(20f, -2f);
			}
		}
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!property.isExpanded)
				return kHeadingSpace;

			var count = property.CountInProperty();
			return EditorGUIUtility.singleLineHeight * count + 20;
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (m_Styles == null)
				m_Styles = new Styles ();

			position.height = EditorGUIUtility.singleLineHeight;
			property.isExpanded = Header (position, property.displayName, property.isExpanded);
			position.y += kHeadingSpace;
			
			if (!property.isExpanded)
				return;
			
			foreach (SerializedProperty child in property)
			{
				EditorGUI.PropertyField (position, child);

				/*	Correction for Wavelengths (Vector3)
				 *	Here we bypass the extra float field from the childs of vector hierarchy .
				 *	However the vector float field will move to second line if the inpector width is smaller than "wideMode".
				 *	So it still need to add extra [Space()] in next parameter to prevent parameter penetration or overlapping.
				 *	In this case uSkyPro added that [Space()] on "public Material SkyboxMaterial;" to get extra line height. */

				switch (child.propertyType) 
				{
				case SerializedPropertyType.Vector2:
					child.Next (true);
					child.Next (true);
					if (!EditorGUIUtility.wideMode)
						position.y += EditorGUIUtility.singleLineHeight + 2f;
					break;
				case SerializedPropertyType.Vector3:
					child.Next (true);
					child.Next (true);
					child.Next (true);
					if (!EditorGUIUtility.wideMode)
						position.y += EditorGUIUtility.singleLineHeight + 2f;
					break;
				case SerializedPropertyType.Vector4:
					if (!child.isExpanded) 
					{
						child.Next (true);
						child.Next (true);
						child.Next (true);
						child.Next (true);
					} 
					else
						position.y -= 2f;
					break;
				default:
					break;
				}

				position.y += EditorGUIUtility.singleLineHeight + 2f; // vertical spacing between PropertyField
			}
		}
		
		private bool Header(Rect position, string title, bool display)
		{
			Rect rect = position;
			position.height = EditorGUIUtility.singleLineHeight;
			GUI.Box(rect, title, m_Styles.header);
			
			Rect toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
			if (Event.current.type == EventType.Repaint)
				EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
			
			Event e = Event.current;
			if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
			{
				display = !display;
				e.Use();
			}
			return display;
		}
	}
}
