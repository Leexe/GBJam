using TMPro;
using UnityEngine;

public class ClockUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _text;

	private void Update()
	{
		int totalSeconds = (int)GameManager.Instance.Time;
		_text.SetText("{0:00}:{1:00}", totalSeconds / 60, totalSeconds % 60);
	}
}
