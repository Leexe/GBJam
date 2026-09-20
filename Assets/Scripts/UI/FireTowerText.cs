using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FireTowerText : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private GridCursorController _cursorController;

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

	private void Awake()
	{
		_canvasGroup.alpha = 0f;
		_rectTransform.anchoredPosition = _primaryPosition;
	}

	private void OnEnable()
	{
		_cursorController.OnRemoveConfirmationChanged += HandleRemoveConfirmationChanged;
		_cursorController.OnCursorMoved += HandleCursorMoved;
		UpdateState();
	}

	private void OnDisable()
	{
		_cursorController.OnRemoveConfirmationChanged -= HandleRemoveConfirmationChanged;
		_cursorController.OnCursorMoved -= HandleCursorMoved;
	}

	private void HandleRemoveConfirmationChanged(bool isConfirming)
	{
		UpdateState();
	}

	private void HandleCursorMoved(Vector2Int gridCoords)
	{
		if (_canvasGroup.alpha > 0f)
		{
			UpdatePosition();
		}
	}

	private void UpdateState()
	{
		bool isConfirming = _cursorController.IsConfirmingRemove;
		_canvasGroup.alpha = isConfirming ? 1f : 0f;

		if (isConfirming)
		{
			UpdatePosition();
		}
	}

	private void UpdatePosition()
	{
		_rectTransform.anchoredPosition = IsCursorOnTopHalf() ? _secondaryPosition : _primaryPosition;
	}

	private bool IsCursorOnTopHalf()
	{
		Vector3 viewportPoint = _camera.WorldToViewportPoint(_cursorController.TargetWorldPosition);
		return viewportPoint.y >= _topHalfThreshold;
	}
}
