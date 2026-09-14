using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _goldText;

	private void OnEnable()
	{
		GameManager.Instance.OnGoldGain += UpdateText;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGoldGain -= UpdateText;
		}
	}

	private void Start()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		_goldText.text = GameManager.Instance.Gold.ToString();
	}
}
