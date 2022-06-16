using System;
using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// A user-configurable control point that can be altered to update a curve.
	/// </summary>
	public struct Knot
	{
		/// <summary>
		/// Returns true if a custom rotation has been specified, otherwise false.
		/// </summary>
		public readonly bool IsUsingRotation => rotation != null;

		/// <summary>
		/// Returns true if a handles are auto-adjusted, otherwise false.
		/// </summary>
		public readonly bool IsUsingAutoHandles => Math.Abs(auto) > .00001f;

		/// <summary>
		/// Position of the knot local to spline.
		/// </summary>
		public Vector3 position;

		/// <summary>
		/// Left handle position local to knot position.
		/// </summary>
		public Vector3 handleIn;

		/// <summary>
		/// Right handle position local to knot position.
		/// </summary>
		public Vector3 handleOut;

		/// <summary>
		/// Any value above 0 will result in an automatically configured knot (ignoring handle inputs).
		/// </summary>
		public float auto;

		/// <summary>
		/// The rotation to influence the any point along the curve before or after this knot.
		/// </summary>
		public Quaternion? rotation;

		/// <summary> Constructor </summary>
		/// <param name = "position"> Position of the knot local to spline </param>
		/// <param name = "handleIn"> Left handle position local to knot position </param>
		/// <param name = "handleOut"> Right handle position local to knot position </param>
		/// <param name = "automatic"> Any value above 0 will result in an automatically configured knot (ignoring handle inputs) </param>
		/// <param name = "rotation"> The rotation to influence the any point along the curve before or after this knot </param>
		public Knot(
			Vector3 position,
			Vector3 handleIn,
			Vector3 handleOut,
			float automatic = 0f,
			Quaternion? rotation = null)
		{
			this.position = position;
			this.handleIn = handleIn;
			this.handleOut = handleOut;
			auto = automatic;
			this.rotation = rotation;
		}
	}
}
