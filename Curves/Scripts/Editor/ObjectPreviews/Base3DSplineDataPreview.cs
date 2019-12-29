using UnityEditor;
using UnityEngine;

namespace JCMG.Curves.Editor
{
	internal abstract class Base3DSplineDataPreview : ObjectPreview
	{
		public sealed override GUIContent GetPreviewTitle()
		{
			return new GUIContent("Properties");
		}

		public sealed override bool HasPreviewGUI()
		{
			return true;
		}

		protected void DrawProperty(ref Rect labelRect, ref Rect valueRect, string label, string value)
		{
			EditorGUI.LabelField(labelRect, label, CurveEditorStyles.LabelStyle);
			EditorGUI.LabelField(valueRect, value);

			labelRect.y += EditorGUIUtility.singleLineHeight;
			valueRect.y += EditorGUIUtility.singleLineHeight;
		}
	}
}
