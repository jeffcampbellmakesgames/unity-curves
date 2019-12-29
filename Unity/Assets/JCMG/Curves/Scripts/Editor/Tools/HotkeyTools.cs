using System;
using UnityEditor;
using UnityEngine;

namespace JCMG.Curves.Editor
{
	internal static class HotkeyTools
	{
		private const string UNDO_REDO_PERFORMED = "UndoRedoPerformed";

		public static void CheckGeneralHotkeys(
			IBezier3DSplineData splineData,
			Action<IReadOnly3DSplineData> onUpdateSpline)
		{
			var evt = Event.current;
			switch (evt.type)
			{
				// Undo Last Command
				case EventType.ValidateCommand:
					if (evt.commandName == UNDO_REDO_PERFORMED)
					{
						onUpdateSpline?.Invoke(splineData);
					}
					break;

				// Flip Spline
				case EventType.KeyDown:
					if (evt.keyCode == KeyCode.I)
					{
						if ((evt.modifiers & (EventModifiers.Control | EventModifiers.Command)) != 0)
						{
							splineData.Flip();
						}
					}
					break;
			}
		}

		public static void CheckSelectedKnotHotkeys(
			IBezier3DSplineData splineData,
			Action<IReadOnly3DSplineData> onUpdateSpline)
		{
			var evt = Event.current;
			switch (evt.type)
			{
				case EventType.KeyDown:
					// Delete Selected Knot
					if (evt.keyCode == KeyCode.Delete)
					{
						if (splineData.KnotCount > 2)
						{
							Undo.RecordObject((UnityEngine.Object)splineData, "Remove Bezier Point");
							splineData.RemoveKnot(CurveEditorState.SelectedKnotIndex);

							CurveEditorState.ClearKnotSelection();

							onUpdateSpline?.Invoke(splineData);
						}

						evt.Use();
					}

					// Focus Selected Knot
					if (evt.keyCode == KeyCode.F)
					{
						var dist = splineData.GetSplineDistanceForKnotIndex(CurveEditorState.SelectedKnotIndex);
						var pos = splineData.GetPosition(dist);

						SceneView.lastActiveSceneView.Frame(new Bounds(pos, Vector3.one * 5f), false);

						evt.Use();
					}

					// Clear Knot Selection
					if (evt.keyCode == KeyCode.Escape)
					{
						CurveEditorState.ClearKnotSelection();
						evt.Use();
					}

					break;
			}
		}
	}
}
