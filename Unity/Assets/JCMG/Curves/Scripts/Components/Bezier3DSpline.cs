using System;
using System.Linq;
using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// A Bezier 3D spline whose positions and rotations are transformed by a <see cref="GameObject"/>'s <see cref="Transform"/>;
	/// </summary>
	[AddComponentMenu("JCMG/Curves/Bezier3DSpline")]
	[ExecuteInEditMode]
	public sealed class Bezier3DSpline : MonoBehaviour,
	                                     IBezier3DSplineData
	{
		#region Properties

		/// <summary>
		/// Returns true if the spline is a closed loop, otherwise false.
		/// </summary>
		public bool IsClosed
		{
			get { return _splineData.IsClosed; }
		}

		/// <summary>
		/// Returns the density of the curve caches. This determines the number of interpolation steps calculated
		/// per curve.
		/// </summary>
		public int InterpolationStepsPerCurve
		{
			get { return _splineData.InterpolationStepsPerCurve; }
		}

		/// <summary>
		/// Returns the number of curves in the spline.
		/// </summary>
		public int CurveCount
		{
			get { return _splineData.CurveCount; }
		}

		/// <summary>
		/// Returns the number of <see cref="Knot"/>s in the spline.
		/// </summary>
		public int KnotCount
		{
			get { return _splineData.KnotCount; }
		}

		/// <summary>
		/// Returns the total length of the spline based on the length of all curves.
		/// </summary>
		public float TotalLength
		{
			get { return _splineData.TotalLength; }
		}

		/// <summary>
		/// Returns the internal <see cref="Bezier3DSplineData"/> of this scene-based spline.
		/// </summary>
		internal Bezier3DSplineData SplineData
		{
			get { return _splineData; }
		}

		#endregion

		#region Fields

		[HideInInspector]
		[SerializeField]
		private Bezier3DSplineData _splineData;

		#endregion

		#region Unity

		private void Awake()
		{
			if (_splineData == null)
			{
				_splineData = ScriptableObject.CreateInstance<Bezier3DSplineData>();
			}
		}

		private void Reset()
		{
			_splineData = ScriptableObject.CreateInstance<Bezier3DSplineData>();
		}

		#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if (UnityEditor.Selection.objects.Contains(this))
			{
				return;
			}

			SceneGUITools.DrawCurveLinesGizmos(this, transform);
		}

		#endif

		#endregion

		#region Settings

		/// <summary>
		/// Recache all individual curves with new interpolation step count.
		/// </summary>
		/// <param name = "stepCount"> Number of steps per curve to cache position and rotation. </param>
		public void SetStepsPerCurve(int stepCount)
		{
			_splineData.SetStepsPerCurve(stepCount);
		}

		/// <summary>
		/// Setting spline to closed will generate an extra curve, connecting end point to start point.
		/// </summary>
		public void SetClosed(bool isClosed)
		{
			_splineData.SetClosed(isClosed);
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Returns a normalized value [0-1] based on the passed <paramref name="splineDistance"/> compared
		/// to the <see cref="TotalLength"/> of the spline.
		/// </summary>
		/// <param name="splineDistance"></param>
		/// <returns></returns>
		public float GetNormalizedValueForSplineDistance(float splineDistance)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a normalized value [0-1] based on the passed <paramref name="splineDistance"/> compared
		/// to the <see cref="TotalLength"/> of the <see cref="Bezier3DCurve"/> that the distance falls along
		/// the spline.
		/// </summary>
		/// <param name="splineDistance"></param>
		/// <returns></returns>
		public float GetCurveDistanceForSplineDistance(float splineDistance)
		{
			return _splineData.GetCurveDistanceForSplineDistance(splineDistance);
		}

		/// <summary>
		/// Returns the length of the spline leading up to and ending on the <see cref="Knot"/> at <paramref name="index"/>
		/// position in the collection.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public float GetSplineDistanceForKnotIndex(int index)
		{
			return _splineData.GetSplineDistanceForKnotIndex(index);
		}

		/// <summary>
		/// Returns the length of the spline leading up to and ending at the end of the <see cref="Bezier3DCurve"/> at
		/// <paramref name="index"/> position in the collection.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public float GetSplineDistanceForCurveIndex(int index)
		{
			return _splineData.GetSplineDistanceForCurveIndex(index);
		}

		public float GetSplineDistanceForNormalizedValue(float value)
		{
			return _splineData.GetSplineDistanceForNormalizedValue(value);
		}

		#endregion

		#region Actions

		/// <summary>
		/// Flip the spline direction.
		/// </summary>
		public void Flip()
		{
			_splineData.Flip();
		}

		#endregion

		#region Curve

		/// <summary>
		/// Get <see cref = "Bezier3DCurve"/> at <paramref name="index"/> position in the collection.
		/// </summary>
		public Bezier3DCurve GetCurve(int index)
		{
			return _splineData.GetCurve(index);
		}

		/// <summary>
		/// Returns the <see cref="Bezier3DCurve"/> where <paramref name="splineDist"/> falls upon it along the spline;
		/// <paramref name="index"/> and <paramref name="curveTime"/> are initialized to the position in the collection
		/// and the normalized value [0-1] of time through the curve.
		/// </summary>
		/// <param name="splineDist"></param>
		/// <param name="index"></param>
		/// <param name="curveTime"></param>
		/// <returns></returns>
		public Bezier3DCurve GetCurveIndexTime(float splineDist, out int index, out float curveTime)
		{
			return _splineData.GetCurveIndexTime(splineDist, out index, out curveTime);
		}

		/// <summary>
		/// Get the curve indices in direct contact with the <see cref="Knot"/> at <paramref name="knotIndex"/> position
		/// in the collection.
		/// </summary>
		public void GetCurveIndicesForKnot(int knotIndex, out int preCurveIndex, out int postCurveIndex)
		{
			_splineData.GetCurveIndicesForKnot(knotIndex, out preCurveIndex, out postCurveIndex);
		}

		#endregion

		#region Rotation

		/// <summary>
		/// Returns rotation along spline at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Quaternion GetRotation(float splineDistance)
		{
			return _splineData.GetRotation(splineDistance, transform);
		}

		/// <summary>
		/// Returns rotation along spline at set distance along the <see cref = "Bezier3DSpline"/>. Uses approximation.
		/// </summary>
		public Quaternion GetRotationFast(float splineDistance)
		{
			return _splineData.GetRotationFast(splineDistance, transform);
		}

		/// <summary>
		/// Returns a rotation along the spline where <paramref name="value"/> is a normalized value between [0-1] of
		/// its <see cref="TotalLength"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Quaternion GetNormalizedRotation(float value)
		{
			var splineDistance = _splineData.GetSplineDistanceForNormalizedValue(value);
			return GetRotation(splineDistance);
		}

		/// <summary>
		/// Returns rotation along spline at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// Uses approximation.
		/// </summary>
		internal Quaternion GetRotationLocal(float splineDistance)
		{
			return _splineData.GetRotationLocal(splineDistance);
		}

		/// <summary>
		/// Returns rotation along spline at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// Uses approximation.
		/// </summary>
		internal Quaternion GetRotationLocalFast(float splineDistance)
		{
			return _splineData.GetRotationLocalFast(splineDistance);
		}

		#endregion

		#region Position

		/// <summary>
		/// Returns position along spline at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Vector3 GetPosition(float splineDistance)
		{
			return _splineData.GetPosition(splineDistance, transform);
		}

		/// <summary>
		/// Returns a position along the spline where <paramref name="value"/> is a normalized value between [0-1] of
		/// its <see cref="TotalLength"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Vector3 GetNormalizedPosition(float value)
		{
			var splineDistance = GetSplineDistanceForNormalizedValue(value);
			return GetPosition(splineDistance);
		}

		/// <summary>
		/// Returns position along spline at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		internal Vector3 GetPointLocal(float splineDistance)
		{
			return _splineData.GetPositionLocal(splineDistance);
		}

		#endregion

		#region Direction

		/// <summary>
		/// Returns up vector at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Vector3 GetUp(float splineDistance)
		{
			return _splineData.GetUp(splineDistance, transform);
		}

		/// <summary>
		/// Returns up vector at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// </summary>
		internal Vector3 GetUpLocal(float splineDistance)
		{
			return _splineData.GetUpLocal(splineDistance);
		}

		/// <summary>
		/// Returns left vector at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Vector3 GetLeft(float splineDistance)
		{
			return _splineData.GetLeft(splineDistance, transform);
		}

		/// <summary>
		/// Returns left vector at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// </summary>
		internal Vector3 GetLeftLocal(float splineDistance)
		{
			return _splineData.GetLeftLocal(splineDistance);
		}

		/// <summary>
		/// Returns right vector at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Vector3 GetRight(float splineDistance)
		{
			return _splineData.GetRight(splineDistance, transform);
		}

		/// <summary>
		/// Returns right vector at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// </summary>
		internal Vector3 GetRightLocal(float splineDistance)
		{
			return _splineData.GetRightLocal(splineDistance);
		}

		/// <summary>
		/// Returns forward vector at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Vector3 GetForward(float splineDistance)
		{
			return _splineData.GetForward(splineDistance, transform);
		}

		/// <summary>
		/// Returns forward vector at set distance along the <see cref = "Bezier3DSpline"/>. Uses approximation.
		/// </summary>
		public Vector3 GetForwardFast(float splineDistance)
		{
			return _splineData.GetForwardFast(splineDistance, transform);
		}

		/// <summary>
		/// Returns forward vector at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// </summary>
		internal Vector3 GetForwardLocal(float splineDistance)
		{
			return _splineData.GetForwardLocal(splineDistance);
		}

		/// <summary>
		/// Returns forward vector at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates. Uses
		/// approximation.
		/// </summary>
		internal Vector3 GetForwardLocalFast(float splineDistance)
		{
			return _splineData.GetForwardLocalFast(splineDistance);
		}

		#endregion

		#region Knot

		/// <summary>
		/// Adds a new <see cref="Knot"/> <paramref name="knot"/> to the end of the spline.
		/// </summary>
		/// <param name="knot"></param>
		public void AddKnot(Knot knot)
		{
			_splineData.AddKnot(knot);
		}

		/// <summary>
		/// Returns <see cref = "Knot"/> info in local coordinates at the <paramref name="index"/> position in the collection.
		/// </summary>
		public Knot GetKnot(int index)
		{
			return _splineData.GetKnot(index);
		}

		/// <summary>
		/// Inserts a new <see cref="Knot"/> at the <paramref name="index"/> position in the collection.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="knot"></param>
		public void InsertKnot(int index, Knot knot)
		{
			_splineData.InsertKnot(index, knot);
		}

		/// <summary>
		/// Removes the <see cref="Knot"/> at the <paramref name="index"/> position in the collection.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveKnot(int index)
		{
			_splineData.RemoveKnot(index);
		}

		/// <summary>
		/// Set <see cref="Knot"/> <paramref name="knot"/> info in local coordinates at the <paramref name="index"/>
		/// position in the collection.
		/// </summary>
		public void SetKnot(int index, Knot knot)
		{
			_splineData.SetKnot(index, knot);
		}

		/// <summary>
		/// Get the knot indices in direct contact with knot. If a knot is not found before and/or after, that index
		/// will be initialized to -1.
		/// </summary>
		public void GetKnotIndicesForKnot(int knotIndex, out int preKnotIndex, out int postKnotIndex)
		{
			_splineData.GetKnotIndicesForKnot(knotIndex, out preKnotIndex, out postKnotIndex);
		}

		#endregion
	}
}
