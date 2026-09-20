using System;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerSelector : MonoBehaviour
{
	public static TowerSelector Instance { get; private set; }

	[Header("UI Roots")]
	[SerializeField]
	private GameObject _menuRoot;

	[Header("Tower Details")]
	[SerializeField]
	private Image _iconImage;

	[SerializeField]
	private TextMeshProUGUI _nameText;

	[SerializeField]
	private TextMeshProUGUI _attackText;

	[SerializeField]
	private TextMeshProUGUI _rangeText;

	[SerializeField]
	private TextMeshProUGUI _priceText;

	[SerializeField]
	private TextMeshProUGUI _descriptionText;

	[Header("Arrows")]
	[SerializeField]
	private RectTransform _leftArrow;

	[SerializeField]
	private RectTransform _rightArrow;

	[Header("Visual Settings")]
	[SerializeField]
	private Color _affordablePriceColor = Color.white;

	[SerializeField]
	private Color _unaffordablePriceColor = new Color(0.85f, 0.25f, 0.25f);

	[SerializeField]
	private float _arrowPunchScale = 1.25f;

	[SerializeField]
	private float _arrowPunchDuration = 0.05f;

	private List<TowerSO> TowerPool => GameManager.Instance.Level.TowerPool;

	[Header("Navigation Settings")]
	[SerializeField]
	private float _repeatDelay = 0.25f;

	[SerializeField]
	private float _repeatRate = 0.1f;

	[SerializeField]
	private bool _pauseTimeWhileOpen = true;

	// State
	private Vector2Int _targetGridPosition;
	private Vector2Int _cursorSize = new Vector2Int(2, 2);
	private int _selectedIndex;
	private bool _isHolding;
	private Vector2Int _currentDirection;
	private float _holdTimer;
	private Sequence _arrowAnim;

	public bool IsOpen { get; private set; }
	public int SelectedIndex => _selectedIndex;
	public List<TowerSO> AvailableTowers => TowerPool;
	public TowerSO SelectedTower => TowerPool[_selectedIndex];

	// Events
	public event Action<TowerSO> OnTowerChanged;
	public event Action<TowerSO> OnTowerConfirmed;
	public event Action<Vector2Int> OnOpened;
	public event Action OnClosed;

	private void Awake()
	{
		Instance = this;
		_menuRoot.SetActive(false);
	}

	private void OnDestroy()
	{
		if (IsOpen)
		{
			Time.timeScale = 1f;
			UnsubscribeInput();
		}
	}

	public void Open(Vector2Int gridPosition, Vector2Int cursorSize, int initialIndex = 0)
	{
		if (IsOpen)
		{
			return;
		}

		_targetGridPosition = gridPosition;
		_cursorSize = cursorSize;

		IsOpen = true;
		_isHolding = false;
		_currentDirection = Vector2Int.zero;
		_holdTimer = 0f;

		if (_pauseTimeWhileOpen)
		{
			Time.timeScale = 0f;
		}

		_menuRoot.SetActive(true);

		SetSelected(Mathf.Clamp(initialIndex, 0, TowerPool.Count - 1));
		SubscribeInput();

		OnOpened?.Invoke(_targetGridPosition);
	}

	public void Close(bool resumeTime = true)
	{
		if (!IsOpen)
		{
			return;
		}

		IsOpen = false;
		_isHolding = false;
		_currentDirection = Vector2Int.zero;
		_arrowAnim.Stop();

		ResetArrowScales();

		if (_pauseTimeWhileOpen && resumeTime)
		{
			Time.timeScale = 1f;
		}

		_menuRoot.SetActive(false);

		UnsubscribeInput();
		OnClosed?.Invoke();
	}

	private void ResetArrowScales()
	{
		_leftArrow.localScale = Vector3.one;
		_rightArrow.localScale = Vector3.one;
	}

	private void SubscribeInput()
	{
		InputManager.Instance.OnMovement += HandleMovement;
		InputManager.Instance.OnConfirm += HandleConfirm;
		InputManager.Instance.OnCancel += HandleCancel;
	}

	private void UnsubscribeInput()
	{
		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnMovement -= HandleMovement;
			InputManager.Instance.OnConfirm -= HandleConfirm;
			InputManager.Instance.OnCancel -= HandleCancel;
		}
	}

	private void Update()
	{
		if (!IsOpen || !_isHolding)
		{
			return;
		}

		_holdTimer += Time.unscaledDeltaTime;
		if (_holdTimer >= _repeatDelay)
		{
			if (_currentDirection.x != 0)
			{
				CycleTower(_currentDirection.x);
			}

			_holdTimer -= _repeatRate;
		}
	}

	private void HandleMovement(Vector2 input)
	{
		if (!IsOpen)
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

		Vector2Int dir = input.x != 0 ? new Vector2Int((int)Mathf.Sign(input.x), 0) : Vector2Int.zero;

		if (dir.x == 0)
		{
			return;
		}

		if (dir == _currentDirection && _isHolding)
		{
			return;
		}

		_currentDirection = dir;
		_isHolding = true;
		_holdTimer = 0f;

		CycleTower(dir.x);
	}

	public void CycleTower(int offset)
	{
		int nextIndex = _selectedIndex + offset;
		if (nextIndex < 0 || nextIndex >= TowerPool.Count)
		{
			return;
		}

		SetSelected(nextIndex);
		AnimateArrow(offset);
	}

	private void AnimateArrow(int direction)
	{
		RectTransform arrow = direction < 0 ? _leftArrow : _rightArrow;
		_arrowAnim.Stop();
		ResetArrowScales();
		_arrowAnim = Sequence
			.Create(useUnscaledTime: true)
			.Chain(Tween.Scale(arrow, _arrowPunchScale, _arrowPunchDuration, Ease.OutQuad))
			.Chain(Tween.Scale(arrow, 1f, _arrowPunchDuration, Ease.InQuad));
	}

	public void SetSelected(int index)
	{
		_selectedIndex = Mathf.Clamp(index, 0, TowerPool.Count - 1);
		TowerSO selected = TowerPool[_selectedIndex];
		bool canAfford = GameManager.Instance.CanAfford(selected.Cost);

		_iconImage.sprite = selected.Icon;
		_nameText.text = selected.Name;
		_attackText.text = GetTowerDamageString(selected);
		_rangeText.text = $"{selected.Range:0.#}";
		_priceText.text = $"{selected.Cost}";
		_priceText.color = canAfford ? _affordablePriceColor : _unaffordablePriceColor;
		_descriptionText.text = selected.Description;

		_leftArrow.gameObject.SetActive(_selectedIndex > 0);
		_rightArrow.gameObject.SetActive(_selectedIndex < TowerPool.Count - 1);

		OnTowerChanged?.Invoke(selected);
	}

	private string GetTowerDamageString(TowerSO tower)
	{
		float damage = tower.TowerType == TowerType.Melee ? tower.Damage : tower.ProjectileData.Damage;
		return $"{damage:0.#}";
	}

	private void HandleConfirm()
	{
		if (!IsOpen)
		{
			return;
		}

		TowerSO selected = SelectedTower;
		if (!GameManager.Instance.CanAfford(selected.Cost))
		{
			return;
		}

		OnTowerConfirmed?.Invoke(selected);
		Close(resumeTime: false);
	}

	private void HandleCancel()
	{
		if (!IsOpen)
		{
			return;
		}

		Close(resumeTime: true);
	}
}
