using System;
using UnityEditor;
using UnityEngine;

namespace JCMG.Curves.Editor
{
	[CustomEditor(typeof(Bezier3DSplineData))]
	public sealed class Bezier3DSplineDataInspector : UnityEditor.Editor
	{
		/// <summary>
		/// Get or sets whether or not this inspector should be drawing the scene GUI. Default is true.
		/// </summary>
		public bool ShouldDrawSceneGUI
		{
			get
			{
				return _shouldDrawSceneGUI;
			}
			set
			{
				if (_shouldDrawSceneGUI && !value)
				{
					SceneView.duringSceneGui -= OnSceneGUI;
				}
				else if (!_shouldDrawSceneGUI && value)
				{
					SceneView.duringSceneGui += OnSceneGUI;
				}

				_shouldDrawSceneGUI = value;
			}
		}

		#pragma warning disable 0649
		public event Action<IReadOnly3DSplineData> SplineUpdated;
		#pragma warning restore 0649

		private bool _shouldDrawSceneGUI;
		private Bezier3DSplineData _spline;
		private static Bezier3DSplineData _copyAndPasteSplineData;

		internal void Awake()
		{
			ShouldDrawSceneGUI = true;
		}

		private void OnDestroy()
		{
			ShouldDrawSceneGUI = false;
		}

		internal void OnEnable()
		{
			CurveEditorState.Reset();

			_spline = target as Bezier3DSplineData;
		}

		internal void OnDisable()
		{
			ShouldDrawSceneGUI = false;

			Tools.hidden = false;
			CurveEditorState.ClearKnotSelection();
			Repaint();
		}

		private void OnSceneGUI(SceneView sceneView)
		{
			if (ShouldDrawSceneGUI)
			{
				OnSceneGUI();
			}
		}

		internal void OnSceneGUI()
		{
			HotkeyTools.CheckGeneralHotkeys(_spline, SplineUpdated);

			SceneGUITools.DrawCurveLinesHandles(_spline);
			SceneGUITools.DrawSceneScreenUI();

			ValidateSelected();
			SceneGUITools.DrawUnselectedKnots(_spline, this);

			if (CurvePreferences.ShouldVisualizeRotation)
			{
				SceneGUITools.DrawCurveOrientations(_spline);
			}

			if (CurveEditorState.HasKnotSelected)
			{
				if (CurveEditorState.HasSingleKnotSelected)
				{
					SceneGUITools.DrawSelectedSplitters(_spline, SplineUpdated);
					SceneGUITools.DrawSelectedKnot(_spline, SplineUpdated, this);

					// Hotkeys
					HotkeyTools.CheckSelectedKnotHotkeys(_spline, SplineUpdated);
				}
				else
				{
					SceneGUITools.DrawMultiSelect(_spline, this);
				}
			}
		}

		public override void OnInspectorGUI()
		{
			ValidateSelected();

			// Spline Properties
			EditorGUI.indentLevel = 0;
			GUILayout.BeginVertical(GUI.skin.box);
			EditorGUILayout.LabelField("Spline Settings", EditorStyles.boldLabel);
			EditorGUILayout.Space(5);
			DrawInterpolationSteps();
			DrawClosedToggle();

			// Spline Actions
			GUILayout.BeginVertical(GUI.skin.box);
			EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
			EditorGUILayout.Space(5);
			DrawSplineFlipAction();
			DrawCopySplineDataToClipboard();
			DrawPasteSplineDataToClipboard();
			DrawResetSplineData();
			GUILayout.EndVertical();

			GUILayout.EndVertical();

			// Selected Point Properties
			EditorGUILayout.Space();
			if (CurveEditorState.HasKnotSelected)
			{
				// Header information for selected knot.
				GUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.LabelField(
					$"Selected Knot (index = {CurveEditorState.SelectedKnotIndex})",
					EditorStyles.boldLabel);
				EditorGUILayout.Space(2);

				var knot = _spline.GetKnot(CurveEditorState.SelectedKnotIndex);

				// Draw Position
				DrawKnotPosition(knot);
				EditorGUILayout.Space(2);

				// Draw Orientation
				DrawKnotOrientationToggle(knot);
				DrawKnotOrientationValue(knot);

				EditorGUILayout.Space(2);

				// Draw Auto-Handle
				DrawKnotAutoHandleToggle(knot);
				if (knot.IsUsingAutoHandles)
				{
					DrawKnotAutoHandleValue(knot);
				}
				else
				{
					DrawKnotHandleValues(knot);
				}

				GUILayout.EndVertical();
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

		private void DrawInterpolationSteps()
		{
			using (var changeCheck = new EditorGUI.ChangeCheckScope())
			{
				var steps = _spline.InterpolationStepsPerCurve;
				steps = EditorGUILayout.DelayedIntField("Interpolation Steps Per Curve", steps);

				if (changeCheck.changed)
				{
					Undo.RecordObject(_spline, "Set interpolation steps per curve");

					_spline.SetStepsPerCurve(steps);
					SplineUpdated?.Invoke(_spline);
				}
			}
		}

		private void DrawClosedToggle()
		{
			using (var changeCheck = new EditorGUI.ChangeCheckScope())
			{
				var closed = _spline.IsClosed;
				closed = EditorGUILayout.Toggle(
					new GUIContent("Closed", "Generate an extra curve, connecting the final point to the first point."),
					closed);

				if (changeCheck.changed)
				{
					Undo.RecordObject(_spline, "Set closed");

					_spline.SetClosed(closed);
					SplineUpdated?.Invoke(_spline);
					SceneView.RepaintAll();
				}
			}
		}

		private void DrawSplineFlipAction()
		{
			if (GUILayout.Button(new GUIContent("Flip", "Flip spline direction.")))
			{
				Undo.RecordObject(_spline, "Flip spline");

				_spline.Flip();
				SplineUpdated?.Invoke(_spline);
				SceneView.RepaintAll();
			}
		}

		private void DrawCopySplineDataToClipboard()
		{
			if (GUILayout.Button(new GUIContent("Copy To Clipboard", "Copies this spline to the inspector")))
			{
				_copyAndPasteSplineData = _spline;
			}
		}

		private void DrawPasteSplineDataToClipboard()
		{
			using (new EditorGUI.DisabledGroupScope(_copyAndPasteSplineData == null))
			{
				if (GUILayout.Button(new GUIContent("Paste From Clipboard", "Copies this spline to the inspector")))
				{
					Undo.RecordObject(_spline, "Paste spline");

					EditorUtility.CopySerialized(_copyAndPasteSplineData, _spline);
				}
			}
		}

		private void DrawResetSplineData()
		{
			if (GUILayout.Button(new GUIContent("Reset", "Resets the spline back to its starting values")))
			{
				Undo.RecordObject(_spline, "Reset spline");

				_spline.Reset();
			}
		}

		private void DrawKnotPosition(Knot knot)
		{
			// Draw Position
			using (var changeCheck = new EditorGUI.ChangeCheckScope())
			{
				knot.position = EditorGUILayout.Vector3Field("Position", knot.position);

				if (changeCheck.changed)
				{
					Undo.RecordObject(_spline, "Edit Bezier Point");

					_spline.SetKnot(CurveEditorState.SelectedKnotIndex, knot);

					SplineUpdated?.Invoke(_spline);
					SceneView.RepaintAll();
				}
			}
		}

		private void DrawKnotOrientationToggle(Knot knot)
		{
			using (var changeCheck = new EditorGUI.ChangeCheckScope())
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(new GUIContent("Uses Orientation Anchor"));
				var isUsingOrientation = GUILayout.Toggle(knot.IsUsingRotation, string.Empty);
				EditorGUILayout.EndHorizontal();

				if (changeCheck.changed)
				{
					Undo.RecordObject(_spline, "Toggle Bezier Orientation Anchor");

					knot.rotation = !knot.IsUsingRotation ? (Quaternion?)Quaternion.identity : null;
					_spline.SetKnot(CurveEditorState.SelectedKnotIndex, knot);

					SplineUpdated?.Invoke(_spline);
					SceneView.RepaintAll();
				}
			}
		}

		private void DrawKnotOrientationValue(Knot knot)
		{
			if (knot.IsUsingRotation)
			{
				using (var changeCheck = new EditorGUI.ChangeCheckScope())
				{
					var orientationEuler = knot.rotation.Value.eulerAngles;
					orientationEuler = EditorGUILayout.Vector3Field("Orientation", orientationEuler);

					if (changeCheck.changed)
					{
						Undo.RecordObject(_spline, "Modify Knot Rotaton");

						knot.rotation = Quaternion.Euler(orientationEuler);
						_spline.SetKnot(CurveEditorState.SelectedKnotIndex, knot);

						SceneView.RepaintAll();
					}
				}
			}
		}

		private void DrawKnotAutoHandleToggle(Knot knot)
		{
			using (var changeCheck = new EditorGUI.ChangeCheckScope())
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(new GUIContent("Uses Auto Handles"));
				var isUsingAutoHandles = GUILayout.Toggle(knot.IsUsingAutoHandles, string.Empty);
				EditorGUILayout.EndHorizontal();

				if (changeCheck.changed)
				{
					Undo.RecordObject(_spline, "Toggle Bezier Auto Handles");

					knot.auto = isUsingAutoHandles ? 0.33f : 0f;
					_spline.SetKnot(CurveEditorState.SelectedKnotIndex, knot);

					SplineUpdated?.Invoke(_spline);
					SceneView.RepaintAll();
				}
			}
		}

		private void DrawKnotAutoHandleValue(Knot knot)
		{
			// Auto-Handles Distance
			using (var changeCheck = new EditorGUI.ChangeCheckScope())
			{
				knot.auto = EditorGUILayout.FloatField("Distance", knot.auto);

				if (changeCheck.changed)
				{
					Undo.RecordObject(_spline, "Edit Bezier Point");

					_spline.SetKnot(CurveEditorState.SelectedKnotIndex, knot);

					SplineUpdated?.Invoke(_spline);
					SceneView.RepaintAll();
				}
			}
		}

		private void DrawKnotHandleValues(Knot knot)
		{
			// In-Handle
			using (var changeCheck = new EditorGUI.ChangeCheckScope())
			{
				knot.handleIn = EditorGUILayout.Vector3Field("Handle in", knot.handleIn);

				if (changeCheck.changed)
				{
					Undo.RecordObject(_spline, "Edit Bezier Handle");

					if (CurvePreferences.ShouldMirrorHandleMovement)
					{
						knot.handleOut = -knot.handleIn;
					}
					_spline.SetKnot(CurveEditorState.SelectedKnotIndex, knot);

					SplineUpdated?.Invoke(_spline);
					SceneView.RepaintAll();
				}
			}

			// Out-Handle
			using (var changeCheck = new EditorGUI.ChangeCheckScope())
			{
				knot.handleOut = EditorGUILayout.Vector3Field("Handle out", knot.handleOut);

				if (changeCheck.changed)
				{
					Undo.RecordObject(_spline, "Edit Bezier Handle");

					if (CurvePreferences.ShouldMirrorHandleMovement)
					{
						knot.handleIn = -knot.handleOut;
					}
					_spline.SetKnot(CurveEditorState.SelectedKnotIndex, knot);

					SplineUpdated?.Invoke(_spline);
					SceneView.RepaintAll();
				}
			}
		}
	}
}
