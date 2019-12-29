using System.Collections.Generic;
using UnityEditor;

namespace JCMG.Curves.Editor
{
	/// <summary>
	/// Shared state for modifying curves in the Unity Editor.
	/// </summary>
	internal static class CurveEditorState
	{
		public static bool HasKnotSelected => SelectedKnotIndex != -1;

		public static bool HasSingleKnotSelected => SelectedKnots.Count == 1;

		public static bool HasMultipleKnotsSelected => SelectedKnots.Count > 1;

		public static int SelectedKnotIndex { get; set; }

		public static List<int> SelectedKnots { get; set; }

		static CurveEditorState()
		{
			SelectedKnots = new List<int>();
		}

		public static bool ValidateSelectedKnotIsValid(IReadOnly3DSplineData splineData)
		{
			return SelectedKnotIndex > splineData.CurveCount;
		}

		public static void ClearKnotSelection()
		{
			SelectKnot(-1, false);
		}

		public static void SelectKnot(int i, bool add)
		{
			SelectedKnotIndex = i;
			if (i == -1)
			{
				SelectedKnots.Clear();
				Tools.hidden = false;
			}
			else
			{
				Tools.hidden = true;
				if (add)
				{
					if (SelectedKnots.Contains(i))
					{
						SelectedKnots.Remove(i);
						if (SelectedKnots.Count == 0)
						{
							SelectedKnotIndex = -1;
							Tools.hidden = false;
						}
						else
						{
							SelectedKnotIndex = SelectedKnots[SelectedKnots.Count - 1];
						}
					}
					else
					{
						SelectedKnots.Add(i);

						SelectedKnotIndex = i;
					}
				}
				else
				{
					SelectedKnots.Clear();
					SelectedKnots.Add(i);

					SelectedKnotIndex = i;
				}
			}
		}

		public static void Reset()
		{
			ClearKnotSelection();

			SelectedKnots.Clear();
		}
	}
}
