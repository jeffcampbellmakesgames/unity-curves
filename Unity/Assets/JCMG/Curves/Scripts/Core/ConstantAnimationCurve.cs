using System.Collections.Generic;
using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// Similar to AnimationCurve, except all values are constant. No smoothing applied between keys.
	/// </summary>
	[System.Serializable]
	public class ConstantAnimationCurve
	{
		/// <summary>
		/// The number of keys in the curve.
		/// </summary>
		public int Length
		{
			get { return _time.Count; }
		}

		[SerializeField]
		List<float> _time;

		[SerializeField]
		List<float> _value;

		public ConstantAnimationCurve()
		{
			_value = new List<float>();
			_time = new List<float>();
		}

		/// <summary>
		/// Returns the float value at <paramref name="time"/> in the curve.
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public float Evaluate(float time)
		{
			if (Length == 0)
			{
				return 0;
			}

			var returnValue = GetKeyValue(0);
			for (var i = 0; i < _time.Count; i++)
			{
				if (_time[i] <= time)
				{
					returnValue = _value[i];
				}
				else
				{
					break;
				}
			}

			return returnValue;
		}

		/// <summary>
		/// Adds float <paramref name="value"/> at <paramref name="time"/> on the curve.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="value"></param>
		public void AddKey(float time, float value)
		{
			for (var i = 0; i < _time.Count; i++)
			{
				if (_time[i] > time)
				{
					_time.Insert(i, time);
					_value.Insert(i, value);
					return;
				}
				else if (_time[i] == time)
				{
					_time[i] = time;
					_value[i] = value;
					return;
				}
			}

			_time.Add(time);
			_value.Add(value);
		}

		/// <summary>
		/// Gets the last value of the curve.
		/// </summary>
		public float EvaluateEnd()
		{
			return _value[_value.Count - 1];
		}

		/// <summary>
		/// Returns the time value at the <paramref name="keyIndex"/> position in the curve.
		/// </summary>
		/// <param name="keyIndex"></param>
		/// <returns></returns>
		public float GetKeyTime(int keyIndex)
		{
			return _time[keyIndex];
		}

		/// <summary>
		/// Returns the float value at the <paramref name="keyIndex"/> position in the curve.
		/// </summary>
		/// <param name="keyIndex"></param>
		/// <returns></returns>
		public float GetKeyValue(int keyIndex)
		{
			return _value[keyIndex];
		}
	}
}
