using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

public enum CursorType
{
	Corners,
	Box,
	Cross,
	Pointer,
	Dot,
}

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

	[Header("Animation Settings")]
	[SerializeField]
	[Tooltip("Time between sprite swaps in seconds")]
	private float _frameRate = 0.25f;

	[Header("Cursor Settings")]
	[SerializeField]
	private Vector2Int _cursorSize = new Vector2Int(2, 2);

	[SerializeField]
	private CursorType _cursorType = CursorType.Corners;

	[SerializeField]
	private CursorType _selectionCursorType = CursorType.Box;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[TabGroup("Cursor Image Types", "Corners")]
	[SerializeField]
	private Sprite[] _cornersDefault;

	[TabGroup("Cursor Image Types", "Corners")]
	[SerializeField]
	private Sprite[] _cornersInvalid;

	[TabGroup("Cursor Image Types", "Cross")]
	[SerializeField]
	private Sprite[] _crossFrames;

	[TabGroup("Cursor Image Types", "Cross")]
	[SerializeField]
	private Sprite[] _crossInvalidFrames;

	[TabGroup("Cursor Image Types", "Box")]
	[SerializeField]
	private Sprite[] _boxFrames;

	[TabGroup("Cursor Image Types", "Pointer")]
	[SerializeField]
	private Sprite[] _pointerFrames;

	[TabGroup("Cursor Image Types", "Dot")]
	[SerializeField]
	private Sprite[] _dotFrames;

	[Header("Available Towers")]
	[SerializeField]
	private List<TowerSO> _availableTowers = new();

	private Vector2Int _gridCoordinates;
	private Vector2Int _currentDirection;
	private bool _isHolding;
	private float _holdTimer;
	private int _currentFrame;
	private int _selectedTower;
	private bool _isSelectingTower;
	private CursorType _defaultCursorType;
	private Tween _cursorTween;
	private Sequence _animTween;

	private void OnEnable()
	{
		InputManager.Instance.OnMovement += HandleMovementInput;
		InputManager.Instance.OnConfirm += HandleConfirmPressed;
		InputManager.Instance.OnCancel += HandleCancelPressed;

		StartAnimation();
	}

	private void OnDisable()
	{
		if (_isSelectingTower)
		{
			SetTowerSelectionMode(false);
		}

		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnMovement -= HandleMovementInput;
			InputManager.Instance.OnConfirm -= HandleConfirmPressed;
			InputManager.Instance.OnCancel -= HandleCancelPressed;
		}

		_cursorTween.Stop();
		_animTween.Stop();
	}

	private void Start()
	{
		_defaultCursorType = _cursorType;
		int startX = (GridManager.Instance.GetMaxColumns - _cursorSize.x) / 2;
		int startY = (GridManager.Instance.GetMaxRows - _cursorSize.y) / 2;
		_gridCoordinates = new Vector2Int(startX, startY);
		transform.position = GridManager.Instance.GridToWorld(startX, startY, _cursorSize.x, _cursorSize.y);
		UpdateVisual();
	}

	private void StartAnimation()
	{
		_animTween.Stop();
		_animTween = Sequence
			.Create(-1, useUnscaledTime: true)
			.Chain(Tween.Delay(_frameRate, useUnscaledTime: true))
			.ChainCallback(this, target => target.AdvanceFrame());
	}

	private void AdvanceFrame()
	{
		_currentFrame++;
		UpdateVisual();
	}

	private void Update()
	{
		if (!_isHolding)
		{
			return;
		}

		_holdTimer += Time.unscaledDeltaTime;
		if (_holdTimer >= _repeatDelay)
		{
			if (_isSelectingTower)
			{
				if (_currentDirection.x != 0)
				{
					CycleTower(_currentDirection.x);
				}
			}
			else
			{
				Move(_currentDirection);
			}
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

		if (_isSelectingTower)
		{
			if (dir.x != 0)
			{
				CycleTower(dir.x);
			}
			return;
		}

		Move(dir);
	}

	private void HandleConfirmPressed()
	{
		if (_isSelectingTower)
		{
			GridManager.Instance.PlaceTower(
				_gridCoordinates,
				_availableTowers[_selectedTower],
				_cursorSize.x,
				_cursorSize.y
			);
			SetTowerSelectionMode(false);
			return;
		}

		if (GridManager.Instance.CanPlaceTower(_gridCoordinates, _cursorSize.x, _cursorSize.y))
		{
			SetTowerSelectionMode(true);
		}
	}

	private void HandleCancelPressed()
	{
		if (_isSelectingTower)
		{
			SetTowerSelectionMode(false);
			return;
		}

		if (GridManager.Instance.RemoveTower(_gridCoordinates, _cursorSize.x, _cursorSize.y))
		{
			UpdateVisual();
		}
	}

	private void SetTowerSelectionMode(bool enable)
	{
		_isSelectingTower = enable;
		Time.timeScale = enable ? 0f : 1f;
		_isHolding = false;
		_currentDirection = Vector2Int.zero;
		_holdTimer = 0f;
		SetCursorType(enable ? _selectionCursorType : _defaultCursorType);
	}

	public void CycleTower(int offset)
	{
		_selectedTower = (_selectedTower + offset + _availableTowers.Count) % _availableTowers.Count;
		UpdateVisual();
	}

	private void Move(Vector2Int delta)
	{
		SetPosition(_gridCoordinates.x + delta.x, _gridCoordinates.y + delta.y);
	}

	private void SetPosition(int x, int y)
	{
		if (!GridManager.Instance.IsValidGridArea(x, y, _cursorSize.x, _cursorSize.y))
		{
			return;
		}

		_gridCoordinates = new Vector2Int(x, y);
		Vector3 targetPos = GridManager.Instance.GridToWorld(x, y, _cursorSize.x, _cursorSize.y);

		_cursorTween.Stop();
		_cursorTween = Tween.Position(transform, targetPos, _movementDuration, Ease.OutQuad);
		UpdateVisual();
	}

	private void SetCursorType(CursorType type)
	{
		_cursorType = type;
		UpdateVisual();
	}

	private void UpdateVisual()
	{
		bool canPlace = GridManager.Instance.CanPlaceTower(_gridCoordinates, _cursorSize.x, _cursorSize.y);
		Sprite[] frames = ChangeCursorVisual(canPlace);
		_spriteRenderer.sprite = frames[_currentFrame % frames.Length];
	}

	private Sprite[] ChangeCursorVisual(bool canPlace)
	{
		return _cursorType switch
		{
			CursorType.Corners => canPlace ? _cornersDefault : _cornersInvalid,
			CursorType.Box => _boxFrames,
			CursorType.Pointer => _pointerFrames,
			CursorType.Cross => canPlace ? _crossFrames : _crossInvalidFrames,
			CursorType.Dot => _dotFrames,
			_ => _cornersDefault,
		};
	}
}
