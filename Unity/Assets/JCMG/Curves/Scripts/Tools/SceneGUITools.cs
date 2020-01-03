using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// Helper methods for drawing in the scene
	/// </summary>
	public static class SceneGUITools
	{
		public static void DrawCurveLinesGizmos(IBezier3DSplineData splineData, Transform transform = null)
		{
			Gizmos.color = Color.white;

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
					Gizmos.DrawLine(prev, cur);
					prev = cur;
				}
			}
		}
	}
}
