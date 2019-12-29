using System;
using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// Immutable Bezier curve between two points.
	/// </summary>
	[System.Serializable]
	public class Bezier3DCurve
	{
		/// <summary>
		/// Start point.
		/// </summary>
		public Vector3 StartPoint
		{
			get { return _startPoint; }
		}

		/// <summary>
		/// First handle. Local to start point.
		/// </summary>
		public Vector3 FirstHandle
		{
			get { return _firstHandle; }
		}

		/// <summary>
		/// Second handle. Local to end point.
		/// </summary>
		public Vector3 SecondHandle
		{
			get { return _secondHandle; }
		}

		/// <summary>
		/// End point
		/// </summary>
		public Vector3 EndPoint
		{
			get { return _endPoint; }
		}

		/// <summary>
		/// Total length of the curve
		/// .</summary>
		public float Length
		{
			get { return _length; }
		}

		/// <summary>
		/// True if the curve is defined as a straight line.
		/// </summary>
		public bool IsLinear
		{
			get { return _isLinear; }
		}

		public AnimationCurve DistanceCache
		{
			get { return _distanceCache; }
		}

		[SerializeField]
		private Vector3 _startPoint;

		[SerializeField]
		private Vector3 _firstHandle;

		[SerializeField]
		private Vector3 _startHandleWorldPosition;

		[SerializeField]
		private Vector3 _endHandleWorldPosition;

		[SerializeField]
		private Vector3 _secondHandle;

		[SerializeField]
		private AnimationCurve _distanceCache;

		[SerializeField]
		private Vector3 _endPoint;

		[SerializeField]
		private bool _isLinear;

		[SerializeField]
		private float _length;

		[SerializeField]
		private Vector3AnimationCurve _tangentCache;

		/// <summary> Constructor </summary>
		/// <param name = "startPoint"> Start point </param>
		/// <param name = "firstHandle"> First handle. Local to start point </param>
		/// <param name = "secondHandle"> Second handle. Local to end point </param>
		/// <param name = "endPoint"> End point </param>
		public Bezier3DCurve(Vector3 startPoint, Vector3 firstHandle, Vector3 secondHandle, Vector3 endPoint, int steps)
		{
			_startPoint = startPoint;
			_firstHandle = firstHandle;
			_secondHandle = secondHandle;
			_endPoint = endPoint;
			_startHandleWorldPosition = startPoint + firstHandle;
			_endHandleWorldPosition = endPoint + secondHandle;
			_isLinear = Math.Abs(firstHandle.sqrMagnitude) < 0.00001f &&
			            Math.Abs(secondHandle.sqrMagnitude) < 0.00001f;

			_distanceCache = GetDistanceCache(
				startPoint,
				startPoint + firstHandle,
				secondHandle + endPoint,
				endPoint,
				steps);

			_tangentCache = GetTangentCache(
				startPoint,
				startPoint + firstHandle,
				secondHandle + endPoint,
				endPoint,
				steps);

			_length = _distanceCache.keys[_distanceCache.keys.Length - 1].time;
		}

		#region Public methods

		public Vector3 GetPoint(float t)
		{
			return GetPoint(
				_startPoint,
				_startHandleWorldPosition,
				_endHandleWorldPosition,
				_endPoint,
				t);
		}

		public void GetPoint(float t, out Vector3 point)
		{
			GetPoint(
				ref _startPoint,
				ref _startHandleWorldPosition,
				ref _endHandleWorldPosition,
				ref _endPoint,
				t,
				out point);
		}

		public void GetForward(float t, out Vector3 forward)
		{
			GetForward(
				ref _startPoint,
				ref _startHandleWorldPosition,
				ref _endHandleWorldPosition,
				ref _endPoint,
				t,
				out forward);
		}


		public Vector3 GetForward(float t)
		{
			return GetForward(
				_startPoint,
				_startHandleWorldPosition,
				_endHandleWorldPosition,
				_endPoint,
				t);
		}

		public Vector3 GetForwardFast(float t)
		{
			return _tangentCache.Evaluate(t);
		}

		public float ConvertDistanceToTime(float distance)
		{
			return _distanceCache.Evaluate(distance);
		}

		#endregion

		#region Private methods

		private static Vector3AnimationCurve GetTangentCache(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int steps)
		{
			var curve = new Vector3AnimationCurve(); //time = distance, value = time
			var delta = 1f / steps;
			for (var i = 0; i < steps + 1; i++)
			{
				curve.AddKey(
					delta * i,
					GetForward(
							p0,
							p1,
							p2,
							p3,
							delta * i)
						.normalized);
			}

			return curve;
		}

		private static AnimationCurve GetDistanceCache(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int steps)
		{
			var curve = new AnimationCurve(); //time = distance, value = time
			var prevPos = Vector3.zero;
			var totalLength = 0f;
			for (var i = 0; i <= steps; i++)
			{
				//Normalize i
				var t = (float)i / (float)steps;

				//Get position from t
				var newPos = GetPoint(
					p0,
					p1,
					p2,
					p3,
					t);

				//First step
				if (i == 0)
				{
					//Add point at (0,0)
					prevPos = GetPoint(
						p0,
						p1,
						p2,
						p3,
						0);
					curve.AddKey(0, 0);
				}
				//Per step
				else
				{
					//Get distance from previous point
					var segmentLength = Vector3.Distance(prevPos, newPos);

					//Accumulate total distance traveled
					totalLength += segmentLength;

					//Save current position for next iteration
					prevPos = newPos;

					//Cache data
					curve.AddKey(totalLength, t);
				}
			}

			return curve;
		}

		public static Vector3 GetPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			t = Mathf.Clamp01(t);
			var oneMinusT = 1f - t;
			return
				oneMinusT * oneMinusT * oneMinusT * a +
				3f * oneMinusT * oneMinusT * t * b +
				3f * oneMinusT * t * t * c +
				t * t * t * d;
		}

		private static Vector3 GetForward(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			//Also known as first derivative
			t = Mathf.Clamp01(t);
			var oneMinusT = 1f - t;
			return
				3f * oneMinusT * oneMinusT * (b - a) +
				6f * oneMinusT * t * (c - b) +
				3f * t * t * (d - c);
		}

		private static void GetForward(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 d, float t, out Vector3 result)
		{
			//Also known as first derivative
			var oneMinusT = 1f - t;
			var baScale = 3f * oneMinusT * oneMinusT;
			var cbScale = 6f * oneMinusT * t;
			var dcScale = 3f * t * t;

			result.x = baScale * (b.x - a.x) + cbScale * (c.x - b.x) + dcScale * (d.x - c.x);
			result.y = baScale * (b.y - a.y) + cbScale * (c.y - b.y) + dcScale * (d.y - c.y);
			result.z = baScale * (b.z - a.z) + cbScale * (c.z - b.z) + dcScale * (d.z - c.z);
		}

		private static void GetPoint(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 d, float t, out Vector3 result)
		{
			var oneMinusT = 1f - t;
			var aScale = oneMinusT * oneMinusT * oneMinusT;
			var bScale = 3f * oneMinusT * oneMinusT * t;
			var cScale = 3f * oneMinusT * t * t;
			var dScale = t * t * t;

			result.x = aScale * a.x + bScale * b.x + cScale * c.x + dScale * d.x;
			result.y = aScale * a.y + bScale * b.y + cScale * c.y + dScale * d.y;
			result.z = aScale * a.z + bScale * b.z + cScale * c.z + dScale * d.z;
		}

		#endregion
	}
}
