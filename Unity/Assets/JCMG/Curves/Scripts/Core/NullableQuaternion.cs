using System;
using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// A serializable version of a nullable-Quaternion.
	/// </summary>
	[Serializable]
	public struct NullableQuaternion
	{
		/// <summary>
		/// Returns the <see cref="Quaternion"/> value.
		/// </summary>
		public Quaternion Value
		{
			get { return rotation; }
		}

		/// <summary>
		/// Returns the <see cref="Quaternion"/> value if present, otherwise null.
		/// </summary>
		public Quaternion? NullableValue
		{
			get { return hasValue ? (Quaternion?)rotation : null; }
		}

		/// <summary>
		/// Returns true if a <see cref="Quaternion"/> value is present, otherwise false.
		/// </summary>
		public bool HasValue
		{
			get { return hasValue; }
		}

		[SerializeField]
		private Quaternion rotation;

		[SerializeField]
		private bool hasValue;

		public NullableQuaternion(Quaternion? rot)
		{
			rotation = rot.HasValue ? rot.Value : Quaternion.identity;
			hasValue = rot.HasValue;
		}

		/// <summary>
		/// User-defined conversion from nullable type to NullableQuaternion
		/// </summary>
		/// <param name="r"></param>
		public static implicit operator NullableQuaternion(Quaternion? r)
		{
			return new NullableQuaternion(r);
		}
	}
}
