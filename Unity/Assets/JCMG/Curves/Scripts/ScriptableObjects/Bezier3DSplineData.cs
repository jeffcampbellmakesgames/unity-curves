using System;
using System.Collections.Generic;
using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// A Bezier 3D spline whose positions and rotations are set in World Space.
	/// </summary>
	[CreateAssetMenu(fileName = "Bezier3DSplineData", menuName = "JCMG/Curves/Bezier3DSplineData")]
	public sealed class Bezier3DSplineData : ScriptableObject,
	                                         IBezier3DSplineData
	{
		#region Properties

		/// <summary>
		/// Returns true if the spline is a closed loop, otherwise false.
		/// </summary>
		public bool IsClosed
		{
			get { return _isClosed; }
		}

		/// <summary>
		/// Returns the density of the curve caches. This determines the number of interpolation steps calculated
		/// per curve.
		/// </summary>
		public int InterpolationStepsPerCurve
		{
			get { return _interpolationStepsPerCurve; }
		}

		/// <summary>
		/// Returns the number of curves in the spline.
		/// </summary>
		public int CurveCount
		{
			get { return _curves.Length; }
		}

		/// <summary>
		/// Returns the number of <see cref="Knot"/>s in the spline.
		/// </summary>
		public int KnotCount
		{
			get { return _curves.Length + (IsClosed ? 0 : 1); }
		}

		/// <summary>
		/// Returns the total length of the spline based on the length of all curves.
		/// </summary>
		public float TotalLength
		{
			get { return _totalLength; }
		}

		#endregion

		#region Fields

		#pragma warning disable 0649

		[SerializeField]
		private bool _isClosed;

		[Min(10)]
		[SerializeField]
		private int _interpolationStepsPerCurve = 60;

		[SerializeField]
		private float _totalLength;

		/// <summary>
		/// Automatic knots don't have handles. Instead they have a percentage and adjust their handles accordingly. A
		/// percentage of 0 indicates that this is not automatic
		/// </summary>
		[SerializeField]
		private List<float> _autoKnotsCache;

		/// <summary>
		/// Curves of the spline
		/// </summary>
		[SerializeField]
		private Bezier3DCurve[] _curves;

		/// <summary>
		/// The cache of rotations for each knot.
		/// </summary>
		[SerializeField]
		private List<NullableQuaternion> _knotRotations;

		[SerializeField]
		private Vector3[] tangentCache;

		#pragma warning restore 0649

		#endregion

		#region Unity

		private void OnEnable()
		{
			Init();
		}

		#endregion

		#region Settings

		/// <summary>
		/// Recache all individual curves with new interpolation step count.
		/// </summary>
		/// <param name = "stepCount"> Number of steps per curve to cache position and rotation. </param>
		public void SetStepsPerCurve(int stepCount)
		{
			_interpolationStepsPerCurve = stepCount;
			for (var i = 0; i < CurveCount; i++)
			{
				_curves[i] = new Bezier3DCurve(
					_curves[i].StartPoint,
					_curves[i].FirstHandle,
					_curves[i].SecondHandle,
					_curves[i].EndPoint,
					_interpolationStepsPerCurve);
			}

			_totalLength = GetTotalLength();
		}

		/// <summary>
		/// Setting spline to closed will generate an extra curve, connecting end point to start point.
		/// </summary>
		public void SetClosed(bool isClosed)
		{
			if (isClosed != _isClosed)
			{
				_isClosed = isClosed;
				if (isClosed)
				{
					var curveList = new List<Bezier3DCurve>(_curves);
					curveList.Add(
						new Bezier3DCurve(
							_curves[CurveCount - 1].EndPoint,
							-_curves[CurveCount - 1].SecondHandle,
							-_curves[0].FirstHandle,
							_curves[0].StartPoint,
							InterpolationStepsPerCurve));
					_curves = curveList.ToArray();
				}
				else
				{
					var curveList = new List<Bezier3DCurve>(_curves);
					curveList.RemoveAt(CurveCount - 1);
					_curves = curveList.ToArray();
				}

				RecalculateCurve();

				_totalLength = GetTotalLength();
			}
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
			return Mathf.Clamp01(splineDistance / TotalLength);
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
			var time = 0f;
			var curveDistance = splineDistance;
			for (var i = 0; i < CurveCount; i++)
			{
				if (_curves[i].Length < curveDistance)
				{
					curveDistance -= _curves[i].Length;
					time += 1f / CurveCount;
				}
				else
				{
					time += _curves[i].ConvertDistanceToTime(curveDistance) / CurveCount;
					return time;
				}
			}

			return 1f;
		}

		/// <summary>
		/// Returns the length of the spline leading up to and ending on the <see cref="Knot"/> at
		/// <paramref name="index"/> position in the collection.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public float GetSplineDistanceForKnotIndex(int index)
		{
			float length;

			GetCurveIndicesForKnot(index, out var preCurveIndex, out var postCurveIndex);
			if (preCurveIndex == -1)
			{
				length = 0f;
			}
			else if (postCurveIndex == -1)
			{
				length = TotalLength;
			}
			else
			{
				return GetSplineDistanceForCurveIndex(preCurveIndex);
			}

			return length;
		}

		/// <summary>
		/// Returns the length of the spline leading up to and ending at the end of the <see cref="Bezier3DCurve"/> at
		/// <paramref name="index"/> position in the collection.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public float GetSplineDistanceForCurveIndex(int index)
		{
			var length = 0f;
			for (var i = 0; i <= index; i++)
			{
				length += _curves[i].Length;
			}

			return length;
		}

		/// <summary>
		/// Returns a set distance along the spline based along a normalized <paramref name="value"/> between [0-1].
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public float GetSplineDistanceForNormalizedValue(float value)
		{
			return Mathf.Clamp01(value) * TotalLength;
		}

		/// <summary>
		/// Resets the spline back to its starting values.
		/// </summary>
		public void Reset()
		{
			Init(true);
		}

		/// <summary>
		/// Initializes the starting values for the spline if not already set or if <paramref name="force"/> is set to
		/// true.
		/// </summary>
		/// <param name="force"></param>
		private void Init(bool force = false)
		{
			if (_autoKnotsCache == null || force)
			{
				_autoKnotsCache = new List<float>()
				{
					0, 0
				};
			}

			if (_curves == null || force)
			{
				_curves = new[]
				{
					new Bezier3DCurve(
						new Vector3(-2, 0, 0),
						new Vector3(0, 0, 2),
						new Vector3(0, 0, -2),
						new Vector3(2, 0, 0),
						_interpolationStepsPerCurve)
				};
			}

			if (_knotRotations == null || force)
			{
				_knotRotations = new List<NullableQuaternion>()
				{
					new NullableQuaternion(null), new NullableQuaternion(null)
				};
			}

			if (force)
			{
				_interpolationStepsPerCurve = 60;
				_isClosed = false;
			}

			RecalculateCurve();
		}

		/// <summary>
		/// Recalculates the entire curve. Should only be used when changes to the curve would fundamentally change the
		/// shape.
		/// </summary>
		private void RecalculateCurve()
		{
			for (var i = 0; i < KnotCount; i++)
			{
				var knot = GetKnot(i);

				SetKnot(i, knot);
			}

			SetStepsPerCurve(InterpolationStepsPerCurve);
		}

		#endregion

		#region Actions

		/// <summary>
		/// Flip the spline direction.
		/// </summary>
		public void Flip()
		{
			var curves = new Bezier3DCurve[CurveCount];
			for (var i = 0; i < CurveCount; i++)
			{
				curves[CurveCount - 1 - i] = new Bezier3DCurve(
					_curves[i].EndPoint,
					_curves[i].SecondHandle,
					_curves[i].FirstHandle,
					_curves[i].StartPoint,
					InterpolationStepsPerCurve);
			}

			_curves = curves;
			_autoKnotsCache.Reverse();
			_knotRotations.Reverse();
		}

		#endregion

		#region Curve

		/// <summary>
		/// Get <see cref = "Bezier3DCurve"/> at <paramref name="index"/> position in the collection.
		/// </summary>
		public Bezier3DCurve GetCurve(int index)
		{
			if (index >= CurveCount || index < 0)
			{
				throw new IndexOutOfRangeException($"Curve index [{index}] out of range");
			}

			return _curves[index];
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
			Bezier3DCurve result;
			for (var i = 0; i < CurveCount; i++)
			{
				result = _curves[i];
				if (result.Length < splineDist)
				{
					splineDist -= result.Length;
				}
				else
				{
					index = i;
					curveTime = result.ConvertDistanceToTime(splineDist);
					return result;
				}
			}

			index = CurveCount - 1;
			result = _curves[index];
			curveTime = 1f;
			return result;
		}

		/// <summary>
		/// Get the curve indices in direct contact with the <see cref="Knot"/> at <paramref name="knotIndex"/> position
		/// in the collection.
		/// </summary>
		public void GetCurveIndicesForKnot(int knotIndex, out int preCurveIndex, out int postCurveIndex)
		{
			//Get the curve index in direct contact with, before the knot
			preCurveIndex = -1;
			if (knotIndex != 0)
			{
				preCurveIndex = knotIndex - 1;
			}
			else if (IsClosed)
			{
				preCurveIndex = CurveCount - 1;
			}

			//Get the curve index in direct contact with, after the knot
			postCurveIndex = -1;
			if (knotIndex != CurveCount)
			{
				postCurveIndex = knotIndex;
			}
			else if (IsClosed)
			{
				postCurveIndex = 0;
			}
		}

		#endregion

		#region Rotation

		/// <summary>
		/// Returns rotation along spline at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Quaternion GetRotation(float splineDistance)
		{
			var forward = GetForward(splineDistance);
			var up = GetUp(splineDistance, forward);
			if (Math.Abs(forward.sqrMagnitude) > 0.00001f)
			{
				return Quaternion.LookRotation(forward, up);
			}
			else
			{
				return Quaternion.identity;
			}
		}

		/// <summary>
		/// Returns rotation along spline at set distance along the <see cref = "Bezier3DSpline"/> in the local
		/// coordinate space of the passed <see cref="Transform"/> <paramref name="transform"/>.
		/// </summary>
		internal Quaternion GetRotation(float splineDistance, Transform transform)
		{
			var forward = GetForward(splineDistance);
			var up = GetUp(splineDistance, forward, transform);
			if (Math.Abs(forward.sqrMagnitude) > 0.00001f)
			{
				return Quaternion.LookRotation(forward, up);
			}
			else
			{
				return Quaternion.identity;
			}
		}

		/// <summary>
		/// Returns rotation along spline at set distance along the <see cref = "Bezier3DSpline"/>. Uses approximation.
		/// </summary>
		public Quaternion GetRotationFast(float splineDistance)
		{
			var forward = GetForwardFast(splineDistance);
			var up = GetUp(splineDistance, forward);
			if (Math.Abs(forward.sqrMagnitude) > 0.00001f)
			{
				return Quaternion.LookRotation(forward, up);
			}
			else
			{
				return Quaternion.identity;
			}
		}

		/// <summary>
		/// Returns rotation along spline at set distance along the <see cref = "Bezier3DSpline"/> in the local
		/// coordinate space of the passed <see cref="Transform"/> <paramref name="transform"/>. Uses approximation.
		/// </summary>
		internal Quaternion GetRotationFast(float splineDistance, Transform transform)
		{
			var forward = GetForwardFast(splineDistance);
			var up = GetUp(splineDistance, forward, transform);
			if (Math.Abs(forward.sqrMagnitude) > 0.00001f)
			{
				return Quaternion.LookRotation(forward, up);
			}
			else
			{
				return Quaternion.identity;
			}
		}

		/// <summary>
		/// Returns rotation along spline at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// Uses approximation.
		/// </summary>
		internal Quaternion GetRotationLocal(float splineDistance)
		{
			var forward = GetForwardLocal(splineDistance);
			var up = GetUp(splineDistance, forward);
			if (Math.Abs(forward.sqrMagnitude) > 0.00001f)
			{
				return Quaternion.LookRotation(forward, up);
			}
			else
			{
				return Quaternion.identity;
			}
		}

		/// <summary>
		/// Returns rotation along spline at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// Uses approximation.
		/// </summary>
		internal Quaternion GetRotationLocalFast(float splineDistance)
		{
			var forward = GetForwardLocalFast(splineDistance);
			var up = GetUp(splineDistance, forward);
			if (Math.Abs(forward.sqrMagnitude) > 0.00001f)
			{
				return Quaternion.LookRotation(forward, up);
			}
			else
			{
				return Quaternion.identity;
			}
		}

		/// <summary>
		/// Returns a rotation along the spline where <paramref name="value"/> is a normalized value between [0-1] of
		/// its <see cref="TotalLength"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Quaternion GetNormalizedRotation(float value)
		{
			var normalizedValue = Mathf.Clamp01(value);
			var splineDistance = TotalLength * normalizedValue;

			return GetRotation(splineDistance);
		}

		#endregion

		#region Position

		/// <summary>
		/// Returns position along spline at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Vector3 GetPosition(float splineDistance)
		{
			return GetPositionLocal(splineDistance);
		}

		/// <summary>
		/// Returns position along spline at set distance along the <see cref = "Bezier3DSpline"/> where the point is
		/// transformed by the <see cref="Transform"/> <paramref name="transform"/>.
		/// </summary>
		internal Vector3 GetPosition(float splineDistance, Transform transform)
		{
			return transform.TransformPoint(GetPositionLocal(splineDistance));
		}

		/// <summary>
		/// Returns position along spline at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		internal Vector3 GetPositionLocal(float splineDistance)
		{
			var curve = GetCurveDistance(splineDistance, out var curveDistance);
			return curve.GetPoint(curve.ConvertDistanceToTime(curveDistance));
		}

		/// <summary>
		/// Returns position along the spline where <paramref name="value"/> is a normalized value between [0-1] of
		/// its <see cref="TotalLength"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Vector3 GetNormalizedPosition(float value)
		{
			var normalizedValue = Mathf.Clamp01(value);
			var splineDistance = TotalLength * normalizedValue;

			return GetPosition(splineDistance);
		}

		#endregion

		#region Direction

		/// <summary>
		/// Returns up vector at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Vector3 GetUp(float splineDistance)
		{
			return GetUpLocal(splineDistance);
		}

		/// <summary>
		/// Returns up vector at set distance along the <see cref = "Bezier3DSpline"/> where direction is transformed
		/// based on the passed <see cref="Transform"/> <paramref name="transform"/>.
		/// </summary>
		internal Vector3 GetUp(float splineDistance, Transform transform)
		{
			return GetUp(splineDistance, GetForward(splineDistance, transform), transform);
		}

		/// <summary>
		/// Returns up vector at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// </summary>
		internal Vector3 GetUpLocal(float splineDistance)
		{
			return GetUp(splineDistance, GetForward(splineDistance));
		}

		private Vector3 GetUp(float splineDistance, Vector3 tangent, Transform transform = null)
		{
			var t = GetCurveDistanceForSplineDistance(splineDistance);
			t *= CurveCount;

			var rotA = Quaternion.identity;
			var rotB = Quaternion.identity;
			var tA = 0;
			var tB = 0;

			// Find earlier rotations
			var foundRotation = false;
			var startIndex =  Mathf.Min((int)t, CurveCount);
			for (var i = startIndex; i >= 0; i--)
			{
				i = (int)Mathf.Repeat(i, KnotCount);

				if (_knotRotations[i].HasValue)
				{
					rotA = _knotRotations[i].Value;
					rotB = _knotRotations[i].Value;
					tA = i;
					tB = i;
					foundRotation = true;
					break;
				}
			}

			// If we don't find any earlier rotations and the curve is closed, start over from the end to our original
			// starting point.
			if (!foundRotation && IsClosed)
			{
				for (var i = CurveCount - 1; i > startIndex; i--)
				{
					i = (int)Mathf.Repeat(i, KnotCount);

					if (_knotRotations[i].HasValue)
					{
						rotA = _knotRotations[i].Value;
						rotB = _knotRotations[i].Value;
						tA = i;
						tB = i;
						break;
					}
				}
			}

			// Find later rotations
			foundRotation = false;
			var endIndex = Mathf.Max((int)t + 1, 0);
			for (var i = endIndex; i < _knotRotations.Count; i++)
			{
				if (_knotRotations[i].HasValue)
				{
					rotB = _knotRotations[i].Value;
					tB = i;
					foundRotation = true;
					break;
				}
			}

			// If we don't find any later rotations and the curve is closed, start over from the beginning to our
			// original starting point.
			if (!foundRotation && IsClosed)
			{
				var upperLimit = Mathf.Min(_knotRotations.Count, endIndex);
				for (var i = 0; i < upperLimit; i++)
				{
					if (_knotRotations[i].HasValue)
					{
						rotB = _knotRotations[i].Value;
						tB = i;
						break;
					}
				}
			}

			// If we end up finding we need to lerp between the end and beginning rotations, set the end index to the
			// length of the knot/curve count
			if (tA > tB)
			{
				tB = tA + 1;
			}

			t = Mathf.InverseLerp(tA, tB, t);
			var rot = Quaternion.Lerp(rotA, rotB, t);

			if (transform != null)
			{
				rot = transform.rotation * rot;
			}

			return Vector3.ProjectOnPlane(rot * Vector3.up, tangent).normalized;
		}

		/// <summary>
		/// Returns left vector at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Vector3 GetLeft(float splineDistance)
		{
			return Vector3.Cross(GetForward(splineDistance), GetUp(splineDistance));
		}

		/// <summary>
		/// Returns left vector at set distance along the <see cref = "Bezier3DSpline"/> where direction is transformed
		/// based on the passed <see cref="Transform"/> <paramref name="transform"/>.
		/// </summary>
		internal Vector3 GetLeft(float splineDistance, Transform transform)
		{
			return Vector3.Cross(GetForward(splineDistance, transform), GetUp(splineDistance, transform));
		}

		/// <summary>
		/// Returns left vector at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// </summary>
		internal Vector3 GetLeftLocal(float splineDistance)
		{
			return Vector3.Cross(GetForwardLocal(splineDistance), GetUpLocal(splineDistance));
		}

		/// <summary>
		/// Returns right vector at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Vector3 GetRight(float splineDistance)
		{
			return -Vector3.Cross(GetForward(splineDistance), GetUp(splineDistance));
		}

		/// <summary>
		/// Returns right vector at set distance along the <see cref = "Bezier3DSpline"/> where direction is transformed
		/// based on the passed <see cref="Transform"/> <paramref name="transform"/>.
		/// </summary>
		internal Vector3 GetRight(float splineDistance, Transform transform)
		{
			return -GetLeft(splineDistance, transform);
		}

		/// <summary>
		/// Returns right vector at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// </summary>
		internal Vector3 GetRightLocal(float splineDistance)
		{
			return -GetLeftLocal(splineDistance);
		}

		/// <summary>
		/// Returns forward vector at set distance along the <see cref = "Bezier3DSpline"/>.
		/// </summary>
		public Vector3 GetForward(float splineDistance)
		{
			return GetForwardLocal(splineDistance);
		}

		/// <summary>
		/// Returns forward vector at set distance along the <see cref = "Bezier3DSpline"/> where the direction is
		/// transformed based on the passed <see cref="Transform"/> <paramref name="transform"/>.
		/// </summary>
		internal Vector3 GetForward(float splineDistance, Transform transform)
		{
			return transform.TransformDirection(GetForwardLocal(splineDistance));
		}

		/// <summary>
		/// Returns forward vector at set distance along the <see cref = "Bezier3DSpline"/>. Uses approximation.
		/// </summary>
		public Vector3 GetForwardFast(float splineDistance)
		{
			return GetForwardLocalFast(splineDistance);
		}

		/// <summary>
		/// Returns forward vector at set distance along the <see cref = "Bezier3DSpline"/> where the direction is
		/// transformed based on the passed <see cref="Transform"/> <paramref name="transform"/>. Uses approximation.
		/// </summary>
		internal Vector3 GetForwardFast(float splineDistance, Transform transform)
		{
			return transform.TransformDirection(GetForwardLocal(splineDistance));
		}

		/// <summary>
		/// Returns forward vector at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates.
		/// </summary>
		internal Vector3 GetForwardLocal(float splineDistance)
		{
			var curve = GetCurveDistance(splineDistance, out var curveDistance);
			return curve.GetForward(curve.ConvertDistanceToTime(curveDistance)).normalized;
		}

		/// <summary>
		/// Returns forward vector at set distance along the <see cref = "Bezier3DSpline"/> in local coordinates. Uses
		/// approximation.
		/// </summary>
		internal Vector3 GetForwardLocalFast(float splineDistance)
		{
			var curve = GetCurveDistance(splineDistance, out var curveDistance);
			return curve.GetForwardFast(curve.ConvertDistanceToTime(curveDistance)).normalized;
		}

		#endregion

		#region Knot

		/// <summary>
		/// Adds a new <see cref="Knot"/> <paramref name="knot"/> to the end of the spline.
		/// </summary>
		/// <param name="knot"></param>
		public void AddKnot(Knot knot)
		{
			var curve = new Bezier3DCurve(
				_curves[CurveCount - 1].EndPoint,
				-_curves[CurveCount - 1].SecondHandle,
				knot.handleIn,
				knot.position,
				InterpolationStepsPerCurve);

			var curveList = new List<Bezier3DCurve>(_curves);
			curveList.Add(curve);
			_curves = curveList.ToArray();

			_autoKnotsCache.Add(knot.auto);
			_knotRotations.Add(knot.rotation);

			SetKnot(KnotCount - 1, knot);
		}

		/// <summary>
		/// Returns <see cref = "Knot"/> info in local coordinates at the <paramref name="index"/> position in the collection.
		/// </summary>
		public Knot GetKnot(int index)
		{
			if (index == 0)
			{
				if (IsClosed)
				{
					return new Knot(
						_curves[0].StartPoint,
						_curves[CurveCount - 1].SecondHandle,
						_curves[0].FirstHandle,
						_autoKnotsCache[index],
						_knotRotations[index].NullableValue);
				}
				else
				{
					return new Knot(
						_curves[0].StartPoint,
						Vector3.zero,
						_curves[0].FirstHandle,
						_autoKnotsCache[index],
						_knotRotations[index].NullableValue);
				}
			}
			else if (index == CurveCount)
			{
				return new Knot(
					_curves[index - 1].EndPoint,
					_curves[index - 1].SecondHandle,
					Vector3.zero,
					_autoKnotsCache[index],
					_knotRotations[index].NullableValue);
			}
			else
			{
				return new Knot(
					_curves[index].StartPoint,
					_curves[index - 1].SecondHandle,
					_curves[index].FirstHandle,
					_autoKnotsCache[index],
					_knotRotations[index].NullableValue);
			}
		}

		/// <summary>
		/// Inserts a new <see cref="Knot"/> at the <paramref name="index"/> position in the collection.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="knot"></param>
		public void InsertKnot(int index, Knot knot)
		{
			Bezier3DCurve curve;
			if (index == 0)
			{
				curve = new Bezier3DCurve(
					knot.position,
					knot.handleOut,
					-_curves[0].FirstHandle,
					_curves[0].StartPoint,
					InterpolationStepsPerCurve);
			}
			else if (index == CurveCount)
			{
				curve = GetCurve(index - 1);
			}
			else
			{
				curve = GetCurve(index);
			}

			var curveList = new List<Bezier3DCurve>(_curves);
			curveList.Insert(index, curve);
			_curves = curveList.ToArray();

			_autoKnotsCache.Insert(index, knot.auto);
			_knotRotations.Insert(index, knot.rotation);

			SetKnot(index, knot);
		}

		/// <summary>
		/// Removes the <see cref="Knot"/> at the <paramref name="index"/> position in the collection.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveKnot(int index)
		{
			if (index == 0)
			{
				var knot = GetKnot(1);

				var curveList = new List<Bezier3DCurve>(_curves);
				curveList.RemoveAt(0);
				_curves = curveList.ToArray();

				_autoKnotsCache.RemoveAt(0);
				_knotRotations.RemoveAt(0);

				SetKnot(0, knot);
			}
			else if (index == CurveCount)
			{
				var curveList = new List<Bezier3DCurve>(_curves);
				curveList.RemoveAt(index - 1);
				_curves = curveList.ToArray();

				_autoKnotsCache.RemoveAt(index);
				_knotRotations.RemoveAt(index);

				if (Math.Abs(_autoKnotsCache[KnotCount - 1]) > 0.00001f)
				{
					SetKnot(KnotCount - 1, GetKnot(KnotCount - 1));
				}
			}
			else
			{
				int preCurveIndex, postCurveIndex;
				GetCurveIndicesForKnot(index, out preCurveIndex, out postCurveIndex);

				var curve = new Bezier3DCurve(
					_curves[preCurveIndex].StartPoint,
					_curves[preCurveIndex].FirstHandle,
					_curves[postCurveIndex].SecondHandle,
					_curves[postCurveIndex].EndPoint,
					InterpolationStepsPerCurve);

				_curves[preCurveIndex] = curve;

				var curveList = new List<Bezier3DCurve>(_curves);
				curveList.RemoveAt(postCurveIndex);
				_curves = curveList.ToArray();

				_autoKnotsCache.RemoveAt(index);
				_knotRotations.RemoveAt(index);

				int preKnotIndex, postKnotIndex;
				GetKnotIndicesForKnot(index, out preKnotIndex, out postKnotIndex);

				SetKnot(preKnotIndex, GetKnot(preKnotIndex));
			}
		}

		/// <summary>
		/// Set <see cref="Knot"/> <paramref name="knot"/> info in local coordinates at the <paramref name="index"/>
		/// position in the collection.
		/// </summary>
		public void SetKnot(int index, Knot knot)
		{
			//If knot is set to auto, adjust handles accordingly
			_knotRotations[index] = knot.rotation;
			_autoKnotsCache[index] = knot.auto;
			if (knot.IsUsingAutoHandles)
			{
				PositionAutoHandles(index, ref knot);
			}

			//Automate knots around this knot
			int preKnotIndex, postKnotIndex;
			GetKnotIndicesForKnot(index, out preKnotIndex, out postKnotIndex);

			var preKnot = new Knot();
			if (preKnotIndex != -1)
			{
				preKnot = GetKnot(preKnotIndex);
				if (preKnot.IsUsingAutoHandles)
				{
					int preKnotPreCurveIndex, preKnotPostCurveIndex;
					GetCurveIndicesForKnot(preKnotIndex, out preKnotPreCurveIndex, out preKnotPostCurveIndex);
					if (preKnotPreCurveIndex != -1)
					{
						PositionAutoHandles(
							preKnotIndex,
							ref preKnot,
							_curves[preKnotPreCurveIndex].StartPoint,
							knot.position);
						_curves[preKnotPreCurveIndex] = new Bezier3DCurve(
							_curves[preKnotPreCurveIndex].StartPoint,
							_curves[preKnotPreCurveIndex].FirstHandle,
							preKnot.handleIn,
							preKnot.position,
							InterpolationStepsPerCurve);
					}
					else
					{
						PositionAutoHandles(
							preKnotIndex,
							ref preKnot,
							Vector3.zero,
							knot.position);
					}
				}
			}

			var postKnot = new Knot();
			if (postKnotIndex != -1)
			{
				postKnot = GetKnot(postKnotIndex);
				if (postKnot.IsUsingAutoHandles)
				{
					int postKnotPreCurveIndex, postKnotPostCurveIndex;
					GetCurveIndicesForKnot(postKnotIndex, out postKnotPreCurveIndex, out postKnotPostCurveIndex);
					if (postKnotPostCurveIndex != -1)
					{
						PositionAutoHandles(
							postKnotIndex,
							ref postKnot,
							knot.position,
							_curves[postKnotPostCurveIndex].EndPoint);
						_curves[postKnotPostCurveIndex] = new Bezier3DCurve(
							postKnot.position,
							postKnot.handleOut,
							_curves[postKnotPostCurveIndex].SecondHandle,
							_curves[postKnotPostCurveIndex].EndPoint,
							InterpolationStepsPerCurve);
					}
					else
					{
						PositionAutoHandles(
							postKnotIndex,
							ref postKnot,
							knot.position,
							Vector3.zero);
					}
				}
			}

			//Get the curve indices in direct contact with knot
			int preCurveIndex, postCurveIndex;
			GetCurveIndicesForKnot(index, out preCurveIndex, out postCurveIndex);

			//Adjust curves in direct contact with the knot
			if (preCurveIndex != -1)
			{
				_curves[preCurveIndex] = new Bezier3DCurve(
					preKnot.position,
					preKnot.handleOut,
					knot.handleIn,
					knot.position,
					InterpolationStepsPerCurve);
			}

			if (postCurveIndex != -1)
			{
				_curves[postCurveIndex] = new Bezier3DCurve(
					knot.position,
					knot.handleOut,
					postKnot.handleIn,
					postKnot.position,
					InterpolationStepsPerCurve);
			}

			_totalLength = GetTotalLength();
		}

		/// <summary>
		/// Get the knot indices in direct contact with knot. If a knot is not found before and/or after, that index
		/// will be initialized to -1.
		/// </summary>
		public void GetKnotIndicesForKnot(int knotIndex, out int preKnotIndex, out int postKnotIndex)
		{
			//Get the curve index in direct contact with, before the knot
			preKnotIndex = -1;
			if (knotIndex != 0)
			{
				preKnotIndex = knotIndex - 1;
			}
			else if (IsClosed)
			{
				preKnotIndex = KnotCount - 1;
			}

			//Get the curve index in direct contact with, after the knot
			postKnotIndex = -1;
			if (knotIndex != KnotCount - 1)
			{
				postKnotIndex = knotIndex + 1;
			}
			else if (IsClosed)
			{
				postKnotIndex = 0;
			}
		}

		#endregion

		#region Private

		/// <summary>
		/// Returns the appropriate curve based on the passed spline distance where that distance falls on that curve;
		/// <paramref name="curveDist"/> will be initialized to a clamped distance along the returned curve.
		/// </summary>
		/// <param name="splineDist"></param>
		/// <param name="curveDist"></param>
		/// <returns></returns>
		private Bezier3DCurve GetCurveDistance(float splineDist, out float curveDist)
		{
			for (var i = 0; i < CurveCount; i++)
			{
				if (_curves[i].Length < splineDist)
				{
					splineDist -= _curves[i].Length;
				}
				else
				{
					curveDist = splineDist;
					return _curves[i];
				}
			}

			curveDist = _curves[CurveCount - 1].Length;
			return _curves[CurveCount - 1];
		}

		/// <summary>
		/// Position handles automatically based on start and end point positions of the curve.
		/// </summary>
		private void PositionAutoHandles(int index, ref Knot knot)
		{
			// Terminology: Points are referred to as A B and C
			// A = prev point, B = current point, C = next point
			Vector3 prevPos;
			if (index != 0)
			{
				prevPos = _curves[index - 1].StartPoint;
			}
			else if (IsClosed)
			{
				prevPos = _curves[CurveCount - 1].StartPoint;
			}
			else
			{
				prevPos = Vector3.zero;
			}

			Vector3 nextPos;
			if (index != KnotCount - 1)
			{
				nextPos = _curves[index].EndPoint;
			}
			else if (IsClosed)
			{
				nextPos = _curves[0].StartPoint;
			}
			else
			{
				nextPos = Vector3.zero;
			}

			PositionAutoHandles(
				index,
				ref knot,
				prevPos,
				nextPos);
		}

		/// <summary>
		/// Position handles automatically based on start and end point positions of the curve.
		/// </summary>
		private void PositionAutoHandles(int index, ref Knot knot, Vector3 prevPos, Vector3 nextPos)
		{
			// Terminology: Points are referred to as A B and C
			// A = prev point, B = current point, C = next point
			var amount = knot.auto;

			// Calculate directional vectors
			var AB = knot.position - prevPos;
			var CB = knot.position - nextPos;

			// Calculate the across vector
			var AB_CB = (CB.normalized - AB.normalized).normalized;

			if (!IsClosed)
			{
				if (index == 0)
				{
					knot.handleOut = CB * -amount;
				}
				else if (index == CurveCount)
				{
					knot.handleIn = AB * -amount;
				}
				else
				{
					knot.handleOut = -AB_CB * CB.magnitude * amount;
					knot.handleIn = AB_CB * AB.magnitude * amount;
				}
			}
			else
			{
				if (KnotCount == 2)
				{
					var left = new Vector3(AB.z, 0, -AB.x) * amount;
					if (index == 0)
					{
						knot.handleIn = left;
						knot.handleOut = -left;
					}

					if (index == 1)
					{
						knot.handleIn = left;
						knot.handleOut = -left;
					}
				}
				else
				{
					knot.handleIn = AB_CB * AB.magnitude * amount;
					knot.handleOut = -AB_CB * CB.magnitude * amount;
				}
			}
		}

		/// <summary>
		/// Calculates the total length of the spline based on the aggregated length of the curves.
		/// </summary>
		/// <returns></returns>
		private float GetTotalLength()
		{
			var length = 0f;
			for (var i = 0; i < CurveCount; i++)
			{
				length += _curves[i].Length;
			}

			return length;
		}

		#endregion
	}
}
