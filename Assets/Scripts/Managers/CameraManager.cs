using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
	[Header("Target")]
	[SerializeField]
	private Transform _shakeTarget;

	[Header("Shake Settings")]
	[SerializeField]
	private Vector3 _defaultShakeStrength = new Vector3(0.2f, 0.2f, 0f);

	[SerializeField]
	private float _defaultShakeDuration = 0.3f;

	[SerializeField]
	private float _defaultShakeFrequency = 25f;

	private Tween _shakeTween;
	private Vector3 _initialLocalPosition;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		if (_shakeTarget == null)
		{
			CinemachineCamera cmCam = FindFirstObjectByType<CinemachineCamera>();
			_shakeTarget = cmCam != null ? cmCam.transform : Camera.main.transform;
		}

		_initialLocalPosition = _shakeTarget.localPosition;
	}

	public void ShakeScreen()
	{
		ShakeScreen(_defaultShakeStrength, _defaultShakeDuration, _defaultShakeFrequency);
	}

	public void ShakeScreen(float strengthFactor, float duration = 0.3f, float frequency = 25f)
	{
		ShakeScreen(new Vector3(strengthFactor, strengthFactor, 0f), duration, frequency);
	}

	public void ShakeScreen(Vector3 strength, float duration = 0.3f, float frequency = 25f)
	{
		if (_shakeTween.isAlive)
		{
			_shakeTween.Stop();
			_shakeTarget.localPosition = _initialLocalPosition;
		}

		_shakeTween = Tween.ShakeLocalPosition(
			_shakeTarget,
			strength,
			duration,
			frequency,
			enableFalloff: true,
			useUnscaledTime: true
		);
	}

	private void OnDisable()
	{
		if (_shakeTween.isAlive)
		{
			_shakeTween.Stop();
			if (_shakeTarget != null)
			{
				_shakeTarget.localPosition = _initialLocalPosition;
			}
		}
	}
}
