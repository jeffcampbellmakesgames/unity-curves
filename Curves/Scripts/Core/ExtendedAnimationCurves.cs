using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// Class Extensions
	/// </summary>
	public static class ExtendedAnimationCurves
	{
		public static void Serialize(this AnimationCurve anim, out float[] times, out float[] values)
		{
			times = new float[anim.length];
			values = new float[anim.length];
			for (var i = 0; i < anim.length; i++)
			{
				times[i] = anim.keys[i].time;
				values[i] = anim.keys[i].value;
			}
		}

		public static AnimationCurve Deserialize(float[] times, float[] values)
		{
			var anim = new AnimationCurve();
			if (times.Length != values.Length)
			{
				Debug.LogWarning("Input data lengths do not match");
			}
			else
			{
				for (var i = 0; i < times.Length; i++)
				{
					anim.AddKey(new Keyframe(times[i], values[i]));
				}
			}

			return anim;
		}
	}
}
