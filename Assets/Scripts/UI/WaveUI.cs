using TMPro;
using UnityEngine;

public class WaveUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _waveText;

	private void OnEnable()
	{
		GameManager.Instance.WaveController.OnWaveStarted += UpdateText;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.WaveController.OnWaveStarted -= UpdateText;
		}
	}

	private void Start()
	{
		UpdateText(GameManager.Instance.WaveController.CurrentWaveIndex);
	}

	private void UpdateText(int waveIndex)
	{
		_waveText.text = $"{waveIndex + 1}/{GameManager.Instance.WaveController.TotalWaves}";
	}
}
