using PrimeTween;
using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _healthText;

	[Header("Shake Settings")]
	[SerializeField]
	private Vector3 _shakeStrength = new Vector3(3f, 3f, 0f);

	[SerializeField]
	private float _shakeDuration = 0.25f;

	[SerializeField]
	private float _shakeFrequency = 25f;

	private Tween _shakeTween;
	private Vector3 _initialLocalPosition;

	private void Awake()
	{
		_initialLocalPosition = transform.localPosition;
	}

	private void OnEnable()
	{
		GameManager.Instance.OnDamage += HandleDamage;
		GameManager.Instance.OnLose += UpdateText;
	}

	private void OnDisable()
	{
		if (_shakeTween.isAlive)
		{
			_shakeTween.Stop();
			transform.localPosition = _initialLocalPosition;
		}

		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnDamage -= HandleDamage;
			GameManager.Instance.OnLose -= UpdateText;
		}
	}

	private void Start()
	{
		UpdateText();
	}

	private void HandleDamage()
	{
		UpdateText();
		Shake();
	}

	private void Shake()
	{
		if (_shakeTween.isAlive)
		{
			_shakeTween.Stop();
		}

		transform.localPosition = _initialLocalPosition;
		_shakeTween = Tween.ShakeLocalPosition(
			transform,
			_shakeStrength,
			_shakeDuration,
			_shakeFrequency,
			enableFalloff: true,
			useUnscaledTime: true
		);
	}

	private void UpdateText()
	{
		_healthText.text = Mathf.Clamp(GameManager.Instance.Health, 0, GameManager.Instance.MaxHealth).ToString();
	}
}
