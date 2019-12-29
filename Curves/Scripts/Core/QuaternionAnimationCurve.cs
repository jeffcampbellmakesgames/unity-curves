using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// Animation curve which stores quaternions, and can evaluate smoothed values in between keyframes.
	/// </summary>
	[System.Serializable]
	public class QuaternionAnimationCurve
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
			public float[] wT;
			public float[] wV;

			public Serializable(QuaternionAnimationCurve curve)
			{
				curve.xQ.Serialize(out xT, out xV);
				curve.yQ.Serialize(out yT, out yV);
				curve.zQ.Serialize(out zT, out zV);
				curve.wQ.Serialize(out wT, out wV);
			}
		}

		/// <summary>
		/// The number of keys in the curve.
		/// </summary>
		public int Length
		{
			get { return xQ.length; }
		}

		[SerializeField]
		private AnimationCurve xQ;

		[SerializeField]
		private AnimationCurve yQ;

		[SerializeField]
		private AnimationCurve zQ;

		[SerializeField]
		private AnimationCurve wQ;

		public QuaternionAnimationCurve()
		{
			wQ = new AnimationCurve();
			zQ = new AnimationCurve();
			yQ = new AnimationCurve();
			xQ = new AnimationCurve();
		}

		public QuaternionAnimationCurve(Serializable serialized)
		{
			wQ = new AnimationCurve();
			zQ = new AnimationCurve();
			yQ = new AnimationCurve();
			xQ = new AnimationCurve();

			xQ = ExtendedAnimationCurves.Deserialize(serialized.xT, serialized.xV);
			yQ = ExtendedAnimationCurves.Deserialize(serialized.yT, serialized.yV);
			zQ = ExtendedAnimationCurves.Deserialize(serialized.zT, serialized.zV);
			wQ = ExtendedAnimationCurves.Deserialize(serialized.wT, serialized.wV);
		}

		/// <summary>
		/// Returns the <see cref="Quaternion"/> at <paramref name="time"/> in the curve.
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public Quaternion Evaluate(float time)
		{
			return new Quaternion(
				xQ.Evaluate(time),
				yQ.Evaluate(time),
				zQ.Evaluate(time),
				wQ.Evaluate(time));
		}

		/// <summary>
		/// Adds <see cref="Quaternion"/> <paramref name="value"/> at <paramref name="time"/> on the curve.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="value"></param>
		public void AddKey(float time, Quaternion value)
		{
			xQ.AddKey(time, value.x);
			yQ.AddKey(time, value.y);
			zQ.AddKey(time, value.z);
			wQ.AddKey(time, value.w);
		}

		/// <summary>
		/// Gets the rotation of the last key.
		/// </summary>
		public Quaternion EvaluateEnd()
		{
			return GetKeyValue(xQ.length - 1);
		}

		/// <summary>
		/// Returns the time value at the <paramref name="keyIndex"/> position in the curve.
		/// </summary>
		/// <param name="keyIndex"></param>
		/// <returns></returns>
		public float GetKeyTime(int keyIndex)
		{
			return wQ.keys[keyIndex].time;
		}

		/// <summary>
		/// Returns the <see cref="Quaternion"/> value at the <paramref name="keyIndex"/> position in the curve.
		/// </summary>
		/// <param name="keyIndex"></param>
		/// <returns></returns>
		public Quaternion GetKeyValue(int keyIndex)
		{
			return new Quaternion(
				xQ.keys[keyIndex].value,
				yQ.keys[keyIndex].value,
				zQ.keys[keyIndex].value,
				wQ.keys[keyIndex].value);
		}
	}
}
