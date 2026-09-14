using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _healthText;

	private void OnEnable()
	{
		GameManager.Instance.OnDamage += UpdateText;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnDamage -= UpdateText;
		}
	}

	private void Start()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		_healthText.text = GameManager.Instance.Health.ToString();
	}
}
