using UnityEditor;
using UnityEngine;

namespace JCMG.Curves.Editor
{
	[CustomPreview(typeof(Bezier3DSplineData))]
	internal sealed class Bezier3DSplineDataPreview : Base3DSplineDataPreview
	{
		public override void OnPreviewGUI(Rect rect, GUIStyle background)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}

			var spline = (Bezier3DSplineData)target;

			var rectOffset = new RectOffset(
				-5,
				-5,
				-5,
				-5);
			rect = rectOffset.Add(rect);

			var position1 = rect;
			position1.width = 110f;

			var position2 = rect;
			position2.xMin += 110f;
			position2.width = 110f;

			EditorGUI.LabelField(position1, "Property", CurveEditorStyles.HeaderStyle);
			EditorGUI.LabelField(position2, "Value", CurveEditorStyles.HeaderStyle);

			position1.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			position2.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

			DrawProperty(
				ref position1,
				ref position2,
				"Point Count",
				spline.KnotCount.ToString());
			DrawProperty(
				ref position1,
				ref position2,
				"Total Length",
				spline.TotalLength.ToString("F"));
		}
	}
}
