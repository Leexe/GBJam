using System;
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
	public bool IsPlacingTower => _isPlacingTower;
	public bool IsSelectingTower => _isSelectingTower;
	public Vector2Int GridCoordinates => _gridCoordinates;
	public Vector3 TargetWorldPosition =>
		GridManager.Instance.GridToWorld(_gridCoordinates.x, _gridCoordinates.y, _cursorSize.x, _cursorSize.y);

	public event Action<bool> OnPlacementModeChanged;
	public event Action<Vector2Int> OnCursorMoved;

	[Header("Ghost Preview")]
	[SerializeField]
	private SpriteRenderer _ghostTowerRenderer;

	[SerializeField]
	private SpriteRenderer _ghostRangeIndicator;

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

	private List<TowerSO> TowerPool => GameManager.Instance.Level.TowerPool;

	[Header("Tower Selector")]
	[SerializeField]
	private TowerSelector _towerSelector;

	[Header("Item Selector")]
	[SerializeField]
	private ItemSelector _itemSelector;

	private bool IsSelectorOpen => _towerSelector.IsOpen || _itemSelector.IsOpen;

	private Vector2Int _gridCoordinates;
	private Vector2Int _currentDirection;
	private bool _isHolding;
	private float _holdTimer;
	private int _currentFrame;
	private int _selectedTower;
	private bool _isSelectingTower;
	private bool _isPlacingTower;
	private CursorType _defaultCursorType;
	private Tween _cursorTween;
	private Sequence _animTween;
	private Tower _currentHoveredTower;

	private void OnEnable()
	{
		InputManager.Instance.OnMovement += HandleMovementInput;
		InputManager.Instance.OnConfirm += HandleConfirmPressed;
		InputManager.Instance.OnCancel += HandleCancelPressed;

		_towerSelector.OnTowerChanged += HandleTowerSelectorChanged;
		_towerSelector.OnTowerConfirmed += HandleTowerConfirmed;
		_towerSelector.OnClosed += HandleTowerSelectorClosed;
		_itemSelector.OnClosed += HandleItemSelectorClosed;

		StartAnimation();
	}

	private void OnDisable()
	{
		if (_isSelectingTower || _isPlacingTower)
		{
			SetPlacingTowerMode(false);
		}

		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnMovement -= HandleMovementInput;
			InputManager.Instance.OnConfirm -= HandleConfirmPressed;
			InputManager.Instance.OnCancel -= HandleCancelPressed;
		}

		_towerSelector.OnTowerChanged -= HandleTowerSelectorChanged;
		_towerSelector.OnTowerConfirmed -= HandleTowerConfirmed;
		_towerSelector.OnClosed -= HandleTowerSelectorClosed;
		_itemSelector.OnClosed -= HandleItemSelectorClosed;

		_cursorTween.Stop();
		_animTween.Stop();

		if (_currentHoveredTower != null)
		{
			_currentHoveredTower.OnCursorExit();
			_currentHoveredTower = null;
		}
	}

	private void Start()
	{
		_defaultCursorType = _cursorType;
		int startX = (GridManager.Instance.GetMaxColumns - _cursorSize.x) / 2;
		int startY = (GridManager.Instance.GetMaxRows - _cursorSize.y) / 2;
		_gridCoordinates = new Vector2Int(startX, startY);
		transform.position = GridManager.Instance.GridToWorld(startX, startY, _cursorSize.x, _cursorSize.y);
		UpdateVisual();
		OnCursorMoved?.Invoke(_gridCoordinates);
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
		if (!_isHolding || IsSelectorOpen)
		{
			return;
		}

		_holdTimer += Time.unscaledDeltaTime;
		if (_holdTimer >= _repeatDelay)
		{
			Move(_currentDirection);
			_holdTimer -= _repeatRate;
		}
	}

	private void HandleMovementInput(Vector2 input)
	{
		if (IsSelectorOpen)
		{
			return;
		}

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
		if (IsSelectorOpen)
		{
			return;
		}

		if (_isPlacingTower)
		{
			TowerSO selected = TowerPool[_selectedTower];
			if (
				GameManager.Instance.CanAfford(selected.Cost)
				&& GridManager.Instance.PlaceTower(_gridCoordinates, selected, _cursorSize.x, _cursorSize.y)
			)
			{
				GameManager.Instance.SpendGold(selected.Cost);
				SetPlacingTowerMode(false);
			}
			return;
		}

		if (GridManager.Instance.CanPlaceTower(_gridCoordinates, _cursorSize.x, _cursorSize.y))
		{
			SetTowerSelectionMode(true);
			_towerSelector.Open(_gridCoordinates, _cursorSize, _selectedTower);
			return;
		}

		if (_currentHoveredTower)
		{
			_towerSelector.OpenInspect(_currentHoveredTower);
		}
	}

	private void HandleCancelPressed()
	{
		if (IsSelectorOpen)
		{
			return;
		}

		if (_isPlacingTower)
		{
			SetPlacingTowerMode(false);
			SetTowerSelectionMode(true);
			_towerSelector.Open(_gridCoordinates, _cursorSize, _selectedTower);
			return;
		}

		if (GridManager.Instance.RemoveTower(_gridCoordinates, _cursorSize.x, _cursorSize.y))
		{
			UpdateVisual();
		}
	}

	private void HandleTowerSelectorChanged(TowerSO tower)
	{
		int index = TowerPool.IndexOf(tower);
		if (index >= 0)
		{
			_selectedTower = index;
		}

		UpdateGhostVisual();
	}

	private void HandleTowerConfirmed(TowerSO tower)
	{
		int index = TowerPool.IndexOf(tower);
		if (index >= 0)
		{
			_selectedTower = index;
		}
		SetPlacingTowerMode(true);
	}

	private void HandleTowerSelectorClosed()
	{
		if (!_isPlacingTower)
		{
			SetTowerSelectionMode(false);
		}
	}

	private void HandleItemSelectorClosed()
	{
		_isHolding = false;
		_currentDirection = Vector2Int.zero;
		_holdTimer = 0f;
	}

	private void SetTowerSelectionMode(bool enable)
	{
		_isSelectingTower = enable;
		if (!enable)
		{
			_isPlacingTower = false;
		}
		Time.timeScale = enable ? 0f : 1f;
		_isHolding = false;
		_currentDirection = Vector2Int.zero;
		_holdTimer = 0f;
		SetCursorType(enable ? _selectionCursorType : _defaultCursorType);
		UpdateGhostVisual();
		OnPlacementModeChanged?.Invoke(_isPlacingTower);
	}

	private void SetPlacingTowerMode(bool enable)
	{
		_isPlacingTower = enable;
		_isSelectingTower = false;
		Time.timeScale = enable ? 0f : 1f;
		_isHolding = false;
		_currentDirection = Vector2Int.zero;
		_holdTimer = 0f;
		SetCursorType(enable ? _selectionCursorType : _defaultCursorType);
		UpdateVisual();
		OnPlacementModeChanged?.Invoke(_isPlacingTower);
	}

	private void UpdateGhostVisual()
	{
		bool show = _isSelectingTower || _isPlacingTower;
		_ghostTowerRenderer.gameObject.SetActive(show);
		_ghostRangeIndicator.gameObject.SetActive(show);

		if (!show)
		{
			return;
		}

		TowerSO selected = TowerPool[_selectedTower];
		_ghostTowerRenderer.sprite = selected.Icon;
		bool canPlace = GridManager.Instance.CanPlaceTower(_gridCoordinates, _cursorSize.x, _cursorSize.y);
		bool canAfford = GameManager.Instance.CanAfford(selected.Cost);
		_ghostTowerRenderer.color =
			(canPlace && canAfford) ? new Color(1f, 1f, 1f, 0.5f) : new Color(0.8f, 0.3f, 0.3f, 0.5f);

		float diameter = selected.Range * 2f;
		_ghostRangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
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
		_cursorTween = Tween.Position(transform, targetPos, _movementDuration, Ease.OutQuad, useUnscaledTime: true);
		UpdateVisual();
		OnCursorMoved?.Invoke(_gridCoordinates);
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
		UpdateHoveredTower();
		UpdateGhostVisual();
	}

	private void UpdateHoveredTower()
	{
		Tower hovered = GetTowerAtCursor();
		if (_currentHoveredTower != hovered)
		{
			_currentHoveredTower?.OnCursorExit();
			_currentHoveredTower = hovered;
			_currentHoveredTower?.OnCursorEnter();
		}
	}

	private Tower GetTowerAtCursor() => GridManager.Instance.GetTower(_gridCoordinates, _cursorSize.x, _cursorSize.y);

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
