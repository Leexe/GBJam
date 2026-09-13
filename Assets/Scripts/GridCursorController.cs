using PrimeTween;
using UnityEngine;

public class GridCursorController : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	[Tooltip("Movement duration")]
	private float _tweenDuration = 0.06f;

	[SerializeField]
	[Tooltip("Hold for this long before move repeat")]
	private float _repeatDelay = 0.25f;

	[SerializeField]
	[Tooltip("Move repeat rate")]
	private float _repeatRate = 0.1f;

	private Vector2Int _gridCoordinates;
	private Vector2Int _currentDirection;
	private bool _isHolding;
	private float _holdTimer;
	private Tween _cursorTween;

	private void OnEnable()
	{
		InputManager.Instance.OnMovement += HandleMovementInput;
	}

	private void OnDisable()
	{
		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnMovement -= HandleMovementInput;
		}
	}

	private void Start()
	{
		int startX = GridManager.Instance.GetMaxColumns / 2;
		int startY = GridManager.Instance.GetMaxRows / 2;
		_gridCoordinates = new Vector2Int(startX, startY);
		transform.position = GridManager.Instance.GridToWorld(startX, startY);
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
		GridManager.Instance.PlaceTower(_gridCoordinates, null);
	}

	private void HandleCancelPressed()
	{
		GridManager.Instance.RemoveTower(_gridCoordinates);
	}

	public void Move(Vector2Int delta)
	{
		SetPosition(_gridCoordinates.x + delta.x, _gridCoordinates.y + delta.y);
	}

	public void SetPosition(int x, int y)
	{
		if (!GridManager.Instance.IsValidGridPos(x, y))
		{
			return;
		}

		_gridCoordinates = new Vector2Int(x, y);
		Vector3 targetPos = GridManager.Instance.GridToWorld(x, y);

		_cursorTween.Stop();
		_cursorTween = Tween.Position(transform, targetPos, _tweenDuration, Ease.OutQuad);
	}
}
