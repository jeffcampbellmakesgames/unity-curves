using UnityEditor;
using UnityEngine;

namespace JCMG.Curves.Editor
{
	/// <summary>
	/// GUI constants and styles for the Unity Editor.
	/// </summary>
	internal static class CurveEditorStyles
	{
		public static GUIStyle HeaderStyle
		{
			get
			{
				if(_headerStyle == null)
				{
					_headerStyle = new GUIStyle(EditorStyles.boldLabel);
					_headerStyle.padding.right += 4;
					_headerStyle.normal.textColor = TextColor;
					_headerStyle.fontSize += 1;
				}

				return _headerStyle;
			}
		}

		public static GUIStyle LabelStyle
		{
			get
			{
				if(_labelStyle == null)
				{
					_labelStyle = new GUIStyle(EditorStyles.label);
					_labelStyle.padding.right += 4;
					_labelStyle.normal.textColor = TextColor;
				}

				return _labelStyle;
			}
		}

		public static Color TextColor
		{
			get
			{
				if (_textColor == null)
				{
					_textColor = new Color(0.7f, 0.7f, 0.7f);
				}

				return _textColor.Value;
			}
		}

		private static GUIStyle _headerStyle;
		private static GUIStyle _labelStyle;
		private static Color? _textColor;
	}
}
