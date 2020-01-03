using System;
using UnityEditor;

namespace JCMG.Curves.Editor
{
	[CustomEditor(typeof(Bezier3DSpline))]
	public sealed class Bezier3DSplineInspector : UnityEditor.Editor
	{
		#pragma warning disable 0649
		public event Action<IReadOnly3DSplineData> SplineUpdated;
		#pragma warning restore 0649

		private Bezier3DSpline _spline;
		private Bezier3DSplineDataInspector _splineDataEditor;

		private void OnEnable()
		{
			CurveEditorState.Reset();

			_spline = (Bezier3DSpline)target;

			_splineDataEditor = (Bezier3DSplineDataInspector)CreateEditor(_spline.SplineData, typeof(Bezier3DSplineDataInspector));
			_splineDataEditor.ShouldDrawSceneGUI = false;
		}

		private void OnDisable()
		{
			_splineDataEditor.OnDisable();

			DestroyImmediate(_splineDataEditor);
		}

		public override void OnInspectorGUI()
		{
			_spline = (Bezier3DSpline)target;

			_splineDataEditor.OnInspectorGUI();
		}

		private void OnSceneGUI()
		{
			HotkeyTools.CheckGeneralHotkeys(_spline, SplineUpdated);

			SceneGUITools.DrawCurveLinesHandles(_spline, _spline.transform);
			SceneGUITools.DrawSceneScreenUI();

			ValidateSelected();

			SceneGUITools.DrawUnselectedKnots(_spline, this, _spline.transform);

			if (CurvePreferences.ShouldVisualizeRotation)
			{
				SceneGUITools.DrawCurveOrientations(_spline);
			}

			if (CurveEditorState.HasKnotSelected)
			{
				if (CurveEditorState.HasSingleKnotSelected)
				{
					SceneGUITools.DrawSelectedSplitters(_spline.SplineData, SplineUpdated, _spline.transform);
					SceneGUITools.DrawSelectedKnot(_spline.SplineData, SplineUpdated, this, _spline.transform);

					// Hotkeys
					HotkeyTools.CheckSelectedKnotHotkeys(_spline, SplineUpdated);
				}
				else
				{
					SceneGUITools.DrawMultiSelect(_spline, this, _spline.transform);
				}
			}
		}

		private void ValidateSelected()
		{
			if (CurveEditorState.ValidateSelectedKnotIsValid(_spline))
			{
				CurveEditorState.ClearKnotSelection();

				Repaint();
			}
		}
	}
}
