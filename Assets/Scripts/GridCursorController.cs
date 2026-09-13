using PrimeTween;
using UnityEngine;

public class GridCursorController : MonoBehaviour
{
	[Header("Movement Tween")]
	[SerializeField]
	[Tooltip("Movement duration")]
	private float _movementDuration = 0.06f;

	[SerializeField]
	[Tooltip("Hold for this long before move repeat")]
	private float _repeatDelay = 0.25f;

	[SerializeField]
	[Tooltip("Move repeat rate")]
	private float _repeatRate = 0.1f;

	[Header("Scale Tween")]
	[SerializeField]
	[Tooltip("Scale Tween")]
	private float _idleDuration = 2f;

	[SerializeField]
	[Range(0, 1)]
	[Tooltip("Scale Tween")]
	private float _scaleRange = 0.1f;

	[SerializeField]
	private Ease _scaleEase = Ease.InOutSine;

	[Header("Cursor Settings")]
	[SerializeField]
	private Vector2Int _cursorSize = new Vector2Int(2, 2);

	private Vector2Int _gridCoordinates;
	private Vector2Int _currentDirection;
	private bool _isHolding;
	private float _holdTimer;
	private Tween _cursorTween;
	private Tween _scaleTween;
	private Vector3 _initialScale = Vector3.one;

	private void Awake()
	{
		_initialScale = transform.localScale;
	}

	private void OnEnable()
	{
		InputManager.Instance.OnMovement += HandleMovementInput;
		InputManager.Instance.OnConfirm += HandleConfirmPressed;
		InputManager.Instance.OnCancel += HandleCancelPressed;

		StartScaleTween();
	}

	private void OnDisable()
	{
		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnMovement -= HandleMovementInput;
			InputManager.Instance.OnConfirm -= HandleConfirmPressed;
			InputManager.Instance.OnCancel -= HandleCancelPressed;
		}

		_cursorTween.Stop();
		_scaleTween.Stop();
		transform.localScale = _initialScale;
	}

	private void Start()
	{
		int startX = (GridManager.Instance.GetMaxColumns - _cursorSize.x) / 2;
		int startY = (GridManager.Instance.GetMaxRows - _cursorSize.y) / 2;
		_gridCoordinates = new Vector2Int(startX, startY);
		transform.position = GridManager.Instance.GridToWorld(startX, startY, _cursorSize.x, _cursorSize.y);
	}

	private void StartScaleTween()
	{
		_scaleTween.Stop();
		transform.localScale = _initialScale;
		_scaleTween = Tween.Scale(
			transform,
			startValue: _initialScale,
			endValue: _initialScale * (1f + _scaleRange),
			duration: _idleDuration * 0.5f,
			ease: _scaleEase,
			cycles: -1,
			cycleMode: CycleMode.Yoyo
		);
	}

	private void StopScaleTween()
	{
		_scaleTween.Stop();
		transform.localScale = _initialScale;
	}

	private void Update()
	{
		if (!_isHolding)
		{
			return;
		}

		_holdTimer += Time.deltaTime;
		if (_holdTimer >= _repeatDelay)
		{
			Move(_currentDirection);
			_holdTimer -= _repeatRate;
		}
	}

	private void HandleMovementInput(Vector2 input)
	{
		if (input == Vector2.zero)
		{
			_isHolding = false;
			_currentDirection = Vector2Int.zero;
			_holdTimer = 0f;
			return;
		}

		Vector2Int dir = input.x != 0 ? new Vector2Int((int)input.x, 0) : new Vector2Int(0, (int)input.y);

		if (dir == _currentDirection && _isHolding)
		{
			return;
		}

		_currentDirection = dir;
		_isHolding = true;
		_holdTimer = 0f;
		Move(dir);
	}

	private void HandleConfirmPressed()
	{
		GridManager.Instance.PlaceTower(_gridCoordinates, null, _cursorSize.x, _cursorSize.y);
	}

	private void HandleCancelPressed()
	{
		GridManager.Instance.RemoveTower(_gridCoordinates, _cursorSize.x, _cursorSize.y);
	}

	public void Move(Vector2Int delta)
	{
		SetPosition(_gridCoordinates.x + delta.x, _gridCoordinates.y + delta.y);
	}

	public void SetPosition(int x, int y)
	{
		if (!GridManager.Instance.IsValidGridArea(x, y, _cursorSize.x, _cursorSize.y))
		{
			return;
		}

		_gridCoordinates = new Vector2Int(x, y);
		Vector3 targetPos = GridManager.Instance.GridToWorld(x, y, _cursorSize.x, _cursorSize.y);

		_cursorTween.Stop();
		_cursorTween = Tween.Position(transform, targetPos, _movementDuration, Ease.OutQuad);
	}
}
