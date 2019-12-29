using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// A read-only 3D-spline
	/// </summary>
	public interface IReadOnly3DSplineData
	{
		// Properties
		bool IsClosed { get; }
		int InterpolationStepsPerCurve { get; }
		int CurveCount { get; }
		int KnotCount { get; }
		float TotalLength { get; }

		// Helper
		float GetNormalizedValueForSplineDistance(float splineDistance);
		float GetCurveDistanceForSplineDistance(float splineDistance);
		float GetSplineDistanceForKnotIndex(int index);
		float GetSplineDistanceForCurveIndex(int index);
		float GetSplineDistanceForNormalizedValue(float value);

		// Orientation
		Quaternion GetRotation(float splineDistance);
		Quaternion GetRotationFast(float splineDistance);
		Quaternion GetNormalizedRotation(float value);

		// Position
		Vector3 GetPosition(float splineDistance);
		Vector3 GetNormalizedPosition(float value);

		// Direction
		Vector3 GetUp(float splineDistance);
		Vector3 GetLeft(float splineDistance);
		Vector3 GetRight(float splineDistance);
		Vector3 GetForward(float splineDistance);
		Vector3 GetForwardFast(float splineDistance);

		// Knot
		void AddKnot(Knot knot);
		Knot GetKnot(int index);
		void GetKnotIndicesForKnot(int knotIndex, out int preKnotIndex, out int postKnotIndex);
	}
}
