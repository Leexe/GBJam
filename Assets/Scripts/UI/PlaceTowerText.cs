using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PlaceTowerText : MonoBehaviour
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
	private Vector2 _primaryPosition = new Vector2(-60f, 30f);

	[SerializeField]
	private Vector2 _secondaryPosition = new Vector2(-60f, -45f);

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
		_cursorController.OnPlacementModeChanged += HandlePlacementModeChanged;
		_cursorController.OnCursorMoved += HandleCursorMoved;
		UpdateState();
	}

	private void OnDisable()
	{
		_cursorController.OnPlacementModeChanged -= HandlePlacementModeChanged;
		_cursorController.OnCursorMoved -= HandleCursorMoved;
	}

	private void HandlePlacementModeChanged(bool isPlacing)
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
		bool isPlacementMode = _cursorController.IsPlacingTower && !_cursorController.IsSelectingTower;
		_canvasGroup.alpha = isPlacementMode ? 1f : 0f;

		if (isPlacementMode)
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
		Camera cam = _camera != null ? _camera : Camera.main;
		Vector3 viewportPoint = cam.WorldToViewportPoint(_cursorController.TargetWorldPosition);
		return viewportPoint.y >= _topHalfThreshold;
	}
}
