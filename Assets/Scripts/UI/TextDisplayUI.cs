using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TextDisplayUI : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private Camera _camera;

	[Header("Positions")]
	[SerializeField]
	private Vector2 _primaryPosition = new Vector2(-65f, 30f);

	[SerializeField]
	private Vector2 _secondaryPosition = new Vector2(-65f, -45f);

	[Header("Settings")]
	[SerializeField]
	private float _topHalfThreshold = 0.5f;

	public bool IsVisible => _canvasGroup.alpha > 0f;

	private void Awake()
	{
		if (_canvasGroup == null)
		{
			_canvasGroup = GetComponent<CanvasGroup>();
		}
		if (_rectTransform == null)
		{
			_rectTransform = GetComponent<RectTransform>();
		}
		if (_text == null)
		{
			_text = GetComponentInChildren<TextMeshProUGUI>();
		}
		if (_camera == null)
		{
			_camera = Camera.main;
		}

		_canvasGroup.alpha = 0f;
		_rectTransform.anchoredPosition = _primaryPosition;
	}

	public void Show(string message)
	{
		SetText(message);
		_canvasGroup.alpha = 1f;
	}

	public void Show(string message, Vector3 worldPosition)
	{
		SetText(message);
		UpdatePosition(worldPosition);
		_canvasGroup.alpha = 1f;
	}

	public void Hide()
	{
		_canvasGroup.alpha = 0f;
	}

	public void SetText(string message)
	{
		_text.text = message;
	}

	public void UpdatePosition(Vector3 worldPosition)
	{
		Vector3 viewportPoint = _camera.WorldToViewportPoint(worldPosition);
		_rectTransform.anchoredPosition = viewportPoint.y >= _topHalfThreshold
			? _secondaryPosition
			: _primaryPosition;
	}
}
