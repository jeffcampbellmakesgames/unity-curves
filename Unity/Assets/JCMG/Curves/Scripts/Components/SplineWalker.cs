using UnityEngine;

namespace JCMG.Curves
{
	/// <summary>
	/// Moves a transform along a spline at either a constant speed or over a fixed time duration.
	/// </summary>
	[AddComponentMenu("JCMG/Curves/SplineWalker")]
	public sealed class SplineWalker : MonoBehaviour
	{
		private enum MoveType
		{
			UseConstantSpeed,
			UseFixedDuration
		}

		private enum LoopType
		{
			Clamp,
			Loop,
			PingPong
		}

		#pragma warning disable 0649

		[Header("Scene References")]
		[SerializeField]
		private Bezier3DSpline _spline;

		[Space]
		[Header("Movement Settings")]
		[SerializeField]
		private LoopType _loopType;

		[SerializeField]
		private MoveType _moveType;

		[SerializeField]
		private float _startingSplineDistance;

		[Space]
		[Header("Constant Speed")]

		[SerializeField]
		private float _speed = 1;

		[Space]
		[Header("Over Fixed Duration")]
		[SerializeField]
		private float _duration;

		[Space]
		[Header("Debugging")]
		[SerializeField]
		private float _currentTime;

		[SerializeField]
		private float _currentSplineDistance;

		#pragma warning restore 0649

		private bool _isMovingForward;
		private Vector3 _currentPosition;
		private Quaternion _currentRotation;

		private void Start()
		{
			_isMovingForward = true;

			if (_spline == null)
			{
				Debug.LogError("Please assign a spline to this SplineWalker.", this);
				enabled = false;
			}
			else
			{
				_startingSplineDistance = Mathf.Clamp(_startingSplineDistance, 0, _spline.TotalLength);
				_currentSplineDistance = _startingSplineDistance;
				_currentTime = _currentSplineDistance / _spline.TotalLength;
				_currentPosition = _spline.GetPosition(_currentSplineDistance);
				_currentRotation = _spline.GetRotation(_currentSplineDistance);

				transform.SetPositionAndRotation(_currentPosition, _currentRotation);
			}
		}

		private void Update()
		{
			SetTargetPositionAndRotation();

			var lerpPosition = Vector3.Lerp(transform.position, _currentPosition, Time.deltaTime * 25f);
			var lerpRotation = Quaternion.Lerp(transform.rotation, _currentRotation, Time.deltaTime * 25f);

			transform.SetPositionAndRotation(lerpPosition, lerpRotation);
		}

		private void SetTargetPositionAndRotation()
		{
			if (_moveType == MoveType.UseConstantSpeed)
			{
				var length = _spline.TotalLength;

				switch (_loopType)
				{
					case LoopType.Clamp:
						_currentSplineDistance += Time.unscaledDeltaTime * _speed;
						_currentSplineDistance = Mathf.Clamp(_currentSplineDistance, 0, length);
						break;
					case LoopType.Loop:
						_currentSplineDistance += Time.unscaledDeltaTime * _speed;
						_currentSplineDistance = Mathf.Repeat(_currentSplineDistance, length);
						break;
					case LoopType.PingPong:
						if (_isMovingForward)
						{
							_currentSplineDistance += Time.unscaledDeltaTime * _speed;
						}
						else if (!_isMovingForward)
						{
							_currentSplineDistance -= Time.unscaledDeltaTime * _speed;
						}

						_currentSplineDistance = Mathf.Clamp(_currentSplineDistance, 0, length);
						if (_currentSplineDistance <= 0 && !_isMovingForward ||
						    _currentSplineDistance >= length && _isMovingForward)
						{
							_isMovingForward = !_isMovingForward;
						}

						break;
				}

				_currentPosition = _spline.GetPosition(_currentSplineDistance);
				_currentRotation = _spline.GetRotation(_currentSplineDistance);
			}
			else if(_moveType == MoveType.UseFixedDuration)
			{
				switch (_loopType)
				{
					case LoopType.Clamp:
						_currentTime += Time.unscaledDeltaTime;
						_currentTime = Mathf.Clamp(_currentTime, 0, _duration);
						break;
					case LoopType.Loop:
						_currentTime += Time.unscaledDeltaTime;
						_currentTime = Mathf.Repeat(_currentTime, _duration);
						break;
					case LoopType.PingPong:
						if (_isMovingForward)
						{
							_currentTime += Time.unscaledDeltaTime;
						}
						else if (!_isMovingForward)
						{
							_currentTime -= Time.unscaledDeltaTime;
						}

						_currentTime = Mathf.Clamp(_currentTime, 0, _duration);
						if (_currentTime <= 0 && !_isMovingForward ||
						    _currentTime >= _duration && _isMovingForward)
						{
							_isMovingForward = !_isMovingForward;
						}

						break;
				}

				var progress = _currentTime / _duration;

				_currentPosition = _spline.GetNormalizedPosition(progress);
				_currentRotation = _spline.GetNormalizedRotation(progress);
			}
		}

		#if UNITY_EDITOR

		private void OnValidate()
		{
			if (_spline == null)
			{
				return;
			}

			_startingSplineDistance = Mathf.Clamp(_startingSplineDistance, 0, _spline.TotalLength);
			_currentSplineDistance = _startingSplineDistance;
			_currentTime = _currentSplineDistance / _spline.TotalLength;
			_currentPosition = _spline.GetPosition(_currentSplineDistance);
			_currentRotation = _spline.GetRotation(_currentSplineDistance);

			transform.SetPositionAndRotation(_currentPosition, _currentRotation);
		}

		#endif
	}
}
