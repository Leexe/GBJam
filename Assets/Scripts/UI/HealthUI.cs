using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _healthText;

	private void OnEnable()
	{
		GameManager.Instance.OnDamage += UpdateText;
		GameManager.Instance.OnLose += UpdateText;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnDamage -= UpdateText;
			GameManager.Instance.OnLose -= UpdateText;
		}
	}

	private void Start()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		_healthText.text = Mathf.Clamp(GameManager.Instance.Health, 0, GameManager.Instance.MaxHealth).ToString();
	}
}
