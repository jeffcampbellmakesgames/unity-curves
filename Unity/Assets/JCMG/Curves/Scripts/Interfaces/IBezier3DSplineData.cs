
namespace JCMG.Curves
{
	/// <summary>
	///  A Bezier 3D spline.
	/// </summary>
	public interface IBezier3DSplineData : IReadOnly3DSplineData
	{
		// Settings
		void SetStepsPerCurve(int stepCount);
		void SetClosed(bool isClosed);

		// Actions
		void Flip();

		// Curve
		Bezier3DCurve GetCurve(int index);
		Bezier3DCurve GetCurveIndexTime(float splineDist, out int index, out float curveTime);
		void GetCurveIndicesForKnot(int knotIndex, out int preCurveIndex, out int postCurveIndex);

		// Knot
		void InsertKnot(int index, Knot knot);
		void RemoveKnot(int index);
		void SetKnot(int index, Knot knot);
	}
}
