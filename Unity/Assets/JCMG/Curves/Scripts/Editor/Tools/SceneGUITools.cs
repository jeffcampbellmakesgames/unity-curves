using System;
using UnityEditor;
using UnityEngine;

namespace JCMG.Curves.Editor
{
	/// <summary>
	/// Helper methods for drawing in the scene
	/// </summary>
	public static class SceneGUITools
	{
		public static void DrawCurveLinesHandles(IBezier3DSplineData splineData, Transform transform = null)
		{
			Handles.color = Color.yellow;

			//Loop through each curve in spline
			var segments = splineData.InterpolationStepsPerCurve;
			var spacing = 1f / segments;
			for (var i = 0; i < splineData.CurveCount; i++)
			{
				var curve = splineData.GetCurve(i);

				//Get curve in world space
				Vector3 a, b, c, d;

				if (transform != null)
				{
					a = transform.TransformPoint(curve.StartPoint);
					b = transform.TransformPoint(curve.FirstHandle + curve.StartPoint);
					c = transform.TransformPoint(curve.SecondHandle + curve.EndPoint);
					d = transform.TransformPoint(curve.EndPoint);
				}
				else
				{
					a = curve.StartPoint;
					b = curve.FirstHandle + curve.StartPoint;
					c = curve.SecondHandle + curve.EndPoint;
					d = curve.EndPoint;
				}

				var prev = Bezier3DCurve.GetPoint(
					a,
					b,
					c,
					d,
					0f);

				for (var k = 0; k <= segments; k++)
				{
					var cur = Bezier3DCurve.GetPoint(
						a,
						b,
						c,
						d,
						k * spacing);
					Handles.DrawLine(prev, cur);
					prev = cur;
				}
			}
		}

		public static void DrawCurveOrientations(IBezier3DSplineData splineData)
		{
			var sceneViewCameraPosition = SceneView.lastActiveSceneView.camera.transform.position;
			var maxViewDistance = CurvePreferences.MaximumViewDistance;

			for (var dist = 0f; dist < splineData.TotalLength; dist += 1)
			{
				var point = splineData.GetPosition(dist);

				if (Vector3.Distance(sceneViewCameraPosition, point) > maxViewDistance)
				{
					continue;
				}

				// Draw Up Vector
				var up = splineData.GetUp(dist);
				Handles.color = Handles.yAxisColor;
				Handles.DrawLine(point, point + up);

				// Draw Forward Vector
				var forward = splineData.GetForward(dist);
				Handles.color = Handles.zAxisColor;
				Handles.DrawLine(point, point + forward);

				// Draw Right Vector
				var right = splineData.GetRight(dist);
				Handles.color = Handles.xAxisColor;
				Handles.DrawLine(point, point + right);
			}
		}

		public static void DrawSelectedKnot(
			Bezier3DSplineData splineData,
			Action<IReadOnly3DSplineData> onUpdateSpline,
			UnityEditor.Editor editorWindow,
			Transform transform = null)
		{
			var knot = splineData.GetKnot(CurveEditorState.SelectedKnotIndex);
			Handles.color = Color.green;

			var knotWorldPos = transform == null
				? knot.position
				: transform.TransformPoint(knot.position);

			if (knot.rotation.HasValue)
			{
				Handles.color = Handles.yAxisColor;
				var rot = knot.rotation.Value;
				Handles.ArrowHandleCap(
					0,
					knotWorldPos,
					rot * Quaternion.AngleAxis(90, Vector3.left),
					0.15f,
					EventType.Repaint);
			}

			if (Tools.current == Tool.Move)
			{
				//Position handle
				using (var changeCheck = new EditorGUI.ChangeCheckScope())
				{
					knotWorldPos = Handles.PositionHandle(knotWorldPos, Tools.handleRotation);

					if (changeCheck.changed)
					{
						Undo.RecordObject(splineData, "Edit Bezier Point");
						knot.position = transform == null ? knotWorldPos : transform.InverseTransformPoint(knotWorldPos);
						splineData.SetKnot(CurveEditorState.SelectedKnotIndex, knot);
						onUpdateSpline?.Invoke(splineData);
						editorWindow.Repaint();
					}
				}

				Handles.color = Color.white;

				//In Handle
				if (knot.handleIn != Vector3.zero)
				{
					using (var changeCheck = new EditorGUI.ChangeCheckScope())
					{
						var inHandleWorldPos = transform == null
							? knot.position + knot.handleIn
							: transform.TransformPoint(knot.position + knot.handleIn);

						inHandleWorldPos = Handles.PositionHandle(inHandleWorldPos, Tools.handleRotation);

						if (changeCheck.changed)
						{
							Undo.RecordObject(splineData, "Edit Bezier Handle");
							knot.handleIn = transform == null
								? inHandleWorldPos - knot.position
								: transform.InverseTransformPoint(inHandleWorldPos) - knot.position;
							knot.auto = 0;
							if (CurvePreferences.ShouldMirrorHandleMovement)
							{
								knot.handleOut = -knot.handleIn;
							}

							splineData.SetKnot(CurveEditorState.SelectedKnotIndex, knot);
							onUpdateSpline?.Invoke(splineData);
							editorWindow.Repaint();
						}

						Handles.DrawLine(knotWorldPos, inHandleWorldPos);
					}
				}

				//outHandle
				if (knot.handleOut != Vector3.zero)
				{
					using (var changeCheck = new EditorGUI.ChangeCheckScope())
					{
						var outHandleWorldPos = transform == null
							? knot.position + knot.handleOut
							: transform.TransformPoint(knot.position + knot.handleOut);

						outHandleWorldPos = Handles.PositionHandle(outHandleWorldPos, Tools.handleRotation);

						if (changeCheck.changed)
						{
							Undo.RecordObject(splineData, "Edit Bezier Handle");
							knot.handleOut = transform == null
								? outHandleWorldPos - knot.position
								: transform.InverseTransformPoint(outHandleWorldPos) - knot.position;
							knot.auto = 0;
							if (CurvePreferences.ShouldMirrorHandleMovement)
							{
								knot.handleIn = -knot.handleOut;
							}

							splineData.SetKnot(CurveEditorState.SelectedKnotIndex, knot);
							onUpdateSpline?.Invoke(splineData);
							editorWindow.Repaint();
						}

						Handles.DrawLine(knotWorldPos, outHandleWorldPos);
					}
				}

			}
			else if (Tools.current == Tool.Rotate)
			{
				//Rotation handle
				using (var changeCheck = new EditorGUI.ChangeCheckScope())
				{
					var rot = (knot.rotation.HasValue ? knot.rotation.Value : Quaternion.identity).normalized;
					rot = Handles.RotationHandle(rot, knotWorldPos);
					if (changeCheck.changed)
					{
						Undo.RecordObject(splineData, "Edit Bezier Point");
						knot.rotation = rot;
						splineData.SetKnot(CurveEditorState.SelectedKnotIndex, knot);
						onUpdateSpline?.Invoke(splineData);
						editorWindow.Repaint();
					}
				}
			}
		}

		public static void DrawMultiSelect(
			IBezier3DSplineData splineData,
			UnityEditor.Editor editorWindow,
			Transform transform = null)
		{
			Handles.color = Color.blue;
			for (var i = 0; i < CurveEditorState.SelectedKnots.Count; i++)
			{
				if (Handles.Button(
					transform == null
						? splineData.GetKnot(CurveEditorState.SelectedKnots[i]).position
						: transform.TransformPoint(splineData.GetKnot(CurveEditorState.SelectedKnots[i]).position),
					Camera.current.transform.rotation,
					SceneGUIConstants.HandleSize,
					SceneGUIConstants.HandleSize,
					Handles.CircleHandleCap))
				{
					CurveEditorState.SelectKnot(CurveEditorState.SelectedKnots[i], true);
					editorWindow.Repaint();
				}
			}

			var handlePos = Vector3.zero;
			if (Tools.pivotMode == PivotMode.Center)
			{
				for (var i = 0; i < CurveEditorState.SelectedKnots.Count; i++)
				{
					handlePos += splineData.GetKnot(CurveEditorState.SelectedKnots[i]).position;
				}

				handlePos /= CurveEditorState.SelectedKnots.Count;
			}
			else
			{
				handlePos = splineData.GetKnot(CurveEditorState.SelectedKnotIndex).position;
			}

			if (transform != null)
			{
				handlePos = transform.TransformPoint(handlePos);
			}

			Handles.PositionHandle(handlePos, Tools.handleRotation);
		}

		public static void DrawUnselectedKnots(
			IBezier3DSplineData splineData,
			UnityEditor.Editor editorWindow,
			Transform transform = null)
		{
			for (var i = 0; i < splineData.KnotCount; i++)
			{
				if (CurveEditorState.SelectedKnots.Contains(i))
				{
					continue;
				}

				var knot = splineData.GetKnot(i);
				var knotWorldPos = transform == null
					? knot.position
					: transform.TransformPoint(knot.position);

				if (knot.rotation.HasValue)
				{
					Handles.color = Handles.yAxisColor;
					var rot = knot.rotation.Value;
					Handles.ArrowHandleCap(
						0,
						knotWorldPos,
						rot * Quaternion.AngleAxis(90, Vector3.left),
						0.15f,
						EventType.Repaint);
				}

				Handles.color = Color.white;
				if (Handles.Button(
					knotWorldPos,
					Camera.current.transform.rotation,
					HandleUtility.GetHandleSize(knotWorldPos) * SceneGUIConstants.HandleSize,
					HandleUtility.GetHandleSize(knotWorldPos) * SceneGUIConstants.HandleSize,
					Handles.CircleHandleCap))
				{
					CurveEditorState.SelectKnot(i, Event.current.control);
					editorWindow.Repaint();
				}
			}
		}

		public static void DrawSelectedSplitters(
			Bezier3DSplineData splineData,
			Action<IReadOnly3DSplineData> onUpdateSpline,
			Transform transform = null)
		{
			Handles.color = Color.white;

			//Start add
			if (!splineData.IsClosed && CurveEditorState.SelectedKnotIndex == 0)
			{
				var curve = splineData.GetCurve(0);
				var a = transform == null
						? curve.StartPoint
						: transform.TransformPoint(curve.StartPoint);
				var b = transform == null
						? curve.FirstHandle.normalized * 2f
						: transform.TransformDirection(curve.FirstHandle).normalized * 2f;

				var handleScale = HandleUtility.GetHandleSize(a);
				b *= handleScale;
				Handles.DrawDottedLine(a, a - b, 3f);
				if (Handles.Button(
					a - b,
					Camera.current.transform.rotation,
					handleScale * SceneGUIConstants.HandleSize * 0.4f,
					handleScale * SceneGUIConstants.HandleSize * 0.4f,
					Handles.DotHandleCap))
				{
					Undo.RecordObject(splineData, "Add Bezier Point");
					var knot = splineData.GetKnot(CurveEditorState.SelectedKnotIndex);
					splineData.InsertKnot(
						0,
						new Knot(
							curve.StartPoint - curve.FirstHandle.normalized * handleScale * 2,
							Vector3.zero,
							curve.FirstHandle.normalized * 0.5f,
							knot.auto,
							knot.rotation));
					onUpdateSpline?.Invoke(splineData);
				}
			}

			//End add
			if (!splineData.IsClosed && CurveEditorState.SelectedKnotIndex == splineData.CurveCount)
			{
				var curve = splineData.GetCurve(splineData.CurveCount - 1);
				var c = transform == null
						? curve.SecondHandle.normalized * 2f
						: transform.TransformDirection(curve.SecondHandle).normalized * 2f;
				var d = transform == null
						? curve.EndPoint
						: transform.TransformPoint(curve.EndPoint);
				var handleScale = HandleUtility.GetHandleSize(d);
				c *= handleScale;
				Handles.DrawDottedLine(d, d - c, 3f);

				if (Handles.Button(
					d - c,
					Camera.current.transform.rotation,
					handleScale * SceneGUIConstants.HandleSize * 0.4f,
					handleScale * SceneGUIConstants.HandleSize * 0.4f,
					Handles.DotHandleCap))
				{
					Undo.RecordObject(splineData, "Add Bezier Point");

					var knot = splineData.GetKnot(CurveEditorState.SelectedKnotIndex);
					splineData.AddKnot(
						new Knot(
							curve.EndPoint - curve.SecondHandle.normalized * handleScale * 2,
							curve.SecondHandle.normalized * 0.5f,
							Vector3.zero,
							knot.auto,
							knot.rotation));

					CurveEditorState.SelectKnot(splineData.CurveCount, false);

					onUpdateSpline?.Invoke(splineData);
				}
			}

			// Prev split
			if (splineData.IsClosed || CurveEditorState.SelectedKnotIndex != 0)
			{
				var curve = splineData.GetCurve(CurveEditorState.SelectedKnotIndex == 0 ? splineData.CurveCount - 1 : CurveEditorState.SelectedKnotIndex - 1);
				var centerLocal = curve.GetPoint(curve.ConvertDistanceToTime(curve.Length * 0.5f));
				var center = transform == null ? centerLocal : transform.TransformPoint(centerLocal);

				var a = curve.StartPoint + curve.FirstHandle;
				var b = curve.SecondHandle + curve.EndPoint;
				var ab = (b - a) * 0.3f;
				var handleScale = HandleUtility.GetHandleSize(center);

				if (Handles.Button(
					center,
					Camera.current.transform.rotation,
					handleScale * SceneGUIConstants.HandleSize * 0.4f,
					handleScale * SceneGUIConstants.HandleSize * 0.4f,
					Handles.DotHandleCap))
				{
					Undo.RecordObject(splineData, "Add Bezier Point");
					var knot = splineData.GetKnot(CurveEditorState.SelectedKnotIndex);
					splineData.InsertKnot(
						CurveEditorState.SelectedKnotIndex == 0 ? splineData.CurveCount : CurveEditorState.SelectedKnotIndex,
						new Knot(
							centerLocal,
							-ab,
							ab,
							knot.auto,
							knot.rotation));

					if (CurveEditorState.SelectedKnotIndex == 0)
					{
						CurveEditorState.SelectKnot(splineData.CurveCount - 1, false);
					}

					onUpdateSpline?.Invoke(splineData);
				}
			}

			// Next split
			if (CurveEditorState.SelectedKnotIndex != splineData.CurveCount)
			{
				var curve = splineData.GetCurve(CurveEditorState.SelectedKnotIndex);
				var centerLocal = curve.GetPoint(curve.ConvertDistanceToTime(curve.Length * 0.5f));
				var center = transform == null ? centerLocal : transform.TransformPoint(centerLocal);

				var a = curve.StartPoint + curve.FirstHandle;
				var b = curve.SecondHandle + curve.EndPoint;
				var ab = (b - a) * 0.3f;
				var handleScale = HandleUtility.GetHandleSize(center);
				if (Handles.Button(
					center,
					Camera.current.transform.rotation,
					handleScale * SceneGUIConstants.HandleSize * 0.4f,
					handleScale * SceneGUIConstants.HandleSize * 0.4f,
					Handles.DotHandleCap))
				{
					Undo.RecordObject(splineData, "Add Bezier Point");
					splineData.InsertKnot(CurveEditorState.SelectedKnotIndex + 1, new Knot(centerLocal, -ab, ab));
					CurveEditorState.SelectKnot(CurveEditorState.SelectedKnotIndex + 1, false);
					onUpdateSpline?.Invoke(splineData);
				}
			}
		}

		public static void DrawSceneScreenUI()
		{
			Handles.BeginGUI();
			var defaultColor = GUI.contentColor;
			var guiLayoutOptions = new GUILayoutOption[]
			{
				GUILayout.MaxWidth(50f),
				GUILayout.MinWidth(50f),
				GUILayout.MinHeight(50f),
				GUILayout.MaxHeight(50f)
			};
			using (new GUILayout.AreaScope(new Rect(SceneGUIConstants.GUIOffset, new Vector2(125f, 50f))))
			{
				using (new GUILayout.HorizontalScope())
				{
					GUI.contentColor = CurvePreferences.ShouldMirrorHandleMovement ? Color.green : Color.red;
					if (GUILayout.Button(new GUIContent(
						(Texture2D)EditorGUIUtility.Load("EchoFilter Icon"),
						"Should opposite handles mirror edited handles?"),
						guiLayoutOptions))
					{
						CurvePreferences.ShouldMirrorHandleMovement = !CurvePreferences.ShouldMirrorHandleMovement;
					}

					GUI.contentColor = CurvePreferences.ShouldVisualizeRotation ? Color.white : Color.red;
					if (GUILayout.Button(new GUIContent(
						(Texture2D)EditorGUIUtility.Load("Transform Icon"),
						"Should visualize rotation along spline?"),
						guiLayoutOptions))
					{
						CurvePreferences.ShouldVisualizeRotation = !CurvePreferences.ShouldVisualizeRotation;
					}
				}
			}
			Handles.EndGUI();
		}
	}
}
