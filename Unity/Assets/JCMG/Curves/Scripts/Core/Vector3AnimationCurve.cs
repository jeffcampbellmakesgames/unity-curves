using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// Animation curve which stores <see cref="Vector3"/>, and can evaluate smoothed values in between keyframes.
	/// </summary>
	[System.Serializable]
	public class Vector3AnimationCurve
	{
		[System.Serializable]
		public class Serializable
		{
			public float[] xT;
			public float[] xV;
			public float[] yT;
			public float[] yV;
			public float[] zT;
			public float[] zV;

			public Serializable(Vector3AnimationCurve curve)
			{
				curve.xV.Serialize(out xT, out xV);
				curve.yV.Serialize(out yT, out yV);
				curve.zV.Serialize(out zT, out zV);
			}
		}

		/// <summary>
		/// The number of keys in the curve.
		/// </summary>
		public int Length
		{
			get { return xV.length; }
		}

		[SerializeField]
		private AnimationCurve xV;

		[SerializeField]
		private AnimationCurve yV;

		[SerializeField]
		private AnimationCurve zV;

		public Vector3AnimationCurve()
		{
			xV = new AnimationCurve();
			yV = new AnimationCurve();
			zV = new AnimationCurve();
		}

		public Vector3AnimationCurve(Serializable serialized)
		{
			xV = new AnimationCurve();
			yV = new AnimationCurve();
			zV = new AnimationCurve();

			xV = ExtendedAnimationCurves.Deserialize(serialized.xT, serialized.xV);
			yV = ExtendedAnimationCurves.Deserialize(serialized.yT, serialized.yV);
			zV = ExtendedAnimationCurves.Deserialize(serialized.zT, serialized.zV);
		}

		/// <summary>
		/// Returns the <see cref="Vector3"/> at <paramref name="time"/> in the curve.
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public Vector3 Evaluate(float time)
		{
			return new Vector3(xV.Evaluate(time), yV.Evaluate(time), zV.Evaluate(time));
		}

		/// <summary>
		/// Adds <see cref="Vector3"/> <paramref name="value"/> at <paramref name="time"/> on the curve.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="value"></param>
		public void AddKey(float time, Vector3 value)
		{
			xV.AddKey(time, value.x);
			yV.AddKey(time, value.y);
			zV.AddKey(time, value.z);
		}

		/// <summary>
		/// Gets the <see cref="Vector3"/> of the last key.
		/// </summary>
		public Vector3 EvaluateEnd()
		{
			return GetKeyValue(xV.length - 1);
		}

		/// <summary>
		/// Returns the time value at the <paramref name="keyIndex"/> position in the curve.
		/// </summary>
		/// <param name="keyIndex"></param>
		/// <returns></returns>
		public float GetKeyTime(int keyIndex)
		{
			return xV.keys[keyIndex].time;
		}

		/// <summary>
		/// Returns the <see cref="Vector3"/> value at the <paramref name="keyIndex"/> position in the curve.
		/// </summary>
		/// <param name="keyIndex"></param>
		/// <returns></returns>
		public Vector3 GetKeyValue(int keyIndex)
		{
			return new Vector3(xV.keys[keyIndex].value, yV.keys[keyIndex].value, zV.keys[keyIndex].value);
		}
	}
}
