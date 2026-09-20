using System;
using System.Collections.Generic;
using Modifiers;
using PrimeTween;
using TMPro;
using UnityEngine;

public class ItemSelector : MonoBehaviour
{
	public static ItemSelector Instance { get; private set; }

	[Header("UI Roots")]
	[SerializeField]
	private GameObject _menuRoot;

	[Header("Item Details")]
	[SerializeField]
	private TextMeshProUGUI _nameText;

	[SerializeField]
	private TextMeshProUGUI _descriptionText;

	[Header("Arrows")]
	[SerializeField]
	private RectTransform _leftArrow;

	[SerializeField]
	private RectTransform _rightArrow;

	[Header("Tower Icons")]
	[SerializeField]
	private List<TowerIcon> _towerIcons = new();

	[Header("Visual Settings")]
	[SerializeField]
	private float _arrowPunchScale = 1.25f;

	[SerializeField]
	private float _arrowPunchDuration = 0.05f;

	[Header("Navigation Settings")]
	[SerializeField]
	private float _repeatDelay = 0.25f;

	[SerializeField]
	private float _repeatRate = 0.1f;

	private List<TowerModifierSO> _items = new();
	private int _selectedIndex;
	private bool _isHolding;
	private Vector2Int _currentDirection;
	private float _holdTimer;
	private Sequence _arrowAnim;

	public bool IsOpen { get; private set; }
	public int SelectedIndex => _selectedIndex;
	public IReadOnlyList<TowerModifierSO> Items => _items;
	public TowerModifierSO SelectedItem => _items[_selectedIndex];

	public event Action<TowerModifierSO> OnItemChanged;
	public event Action<TowerModifierSO> OnItemConfirmed;
	public event Action OnOpened;
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

	public void Open(List<TowerModifierSO> items, int initialIndex = 0)
	{
		if (IsOpen)
		{
			return;
		}

		_items = items;
		IsOpen = true;
		_isHolding = false;
		_currentDirection = Vector2Int.zero;
		_holdTimer = 0f;

		Time.timeScale = 0f;
		_menuRoot.SetActive(true);

		SetSelected(Mathf.Clamp(initialIndex, 0, _items.Count - 1));
		SubscribeInput();

		OnOpened?.Invoke();
	}

	public void Open(int choicesCount = 3)
	{
		Open(GenerateRandomItems(choicesCount));
	}

	public void Open()
	{
		Open(GameManager.Instance.Level.DefaultItemChoicesCount);
	}

	public void Close()
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

		Time.timeScale = 1f;
		_menuRoot.SetActive(false);

		UnsubscribeInput();
		OnClosed?.Invoke();
	}

	public List<TowerModifierSO> GenerateRandomItems(int count)
	{
		List<TowerModifierSO> pool = GameManager.Instance.Level.ItemPool;
		int choiceCount = Mathf.Min(count, pool.Count);
		List<TowerModifierSO> copy = new(pool);
		List<TowerModifierSO> selected = new(choiceCount);

		for (int i = 0; i < choiceCount; i++)
		{
			int randomIndex = UnityEngine.Random.Range(0, copy.Count);
			selected.Add(copy[randomIndex]);
			copy.RemoveAt(randomIndex);
		}

		return selected;
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
		InputManager.Instance.OnMovement -= HandleMovement;
		InputManager.Instance.OnConfirm -= HandleConfirm;
		InputManager.Instance.OnCancel -= HandleCancel;
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
				CycleItem(_currentDirection.x);
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

		CycleItem(dir.x);
	}

	public void CycleItem(int offset)
	{
		int nextIndex = _selectedIndex + offset;
		if (nextIndex < 0 || nextIndex >= _items.Count)
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
		_selectedIndex = Mathf.Clamp(index, 0, _items.Count - 1);
		TowerModifierSO selected = _items[_selectedIndex];

		_nameText.text = selected.Name;
		_descriptionText.text = selected.Description;

		_leftArrow.gameObject.SetActive(_selectedIndex > 0);
		_rightArrow.gameObject.SetActive(_selectedIndex < _items.Count - 1);

		UpdateTowerIcons(selected);

		OnItemChanged?.Invoke(selected);
	}

	private void UpdateTowerIcons(TowerModifierSO item)
	{
		List<TowerSO> levelTowers = GameManager.Instance.Level.TowerPool;

		for (int i = 0; i < _towerIcons.Count; i++)
		{
			TowerIcon icon = _towerIcons[i];
			if (i < levelTowers.Count)
			{
				TowerSO tower = levelTowers[i];
				icon.SetTower(tower);
				bool isAffected = IsTowerAffected(tower.TowerType, item.Category);
				icon.SetActive(isAffected);
			}
			else
			{
				icon.SetActive(false);
			}
		}
	}

	private bool IsTowerAffected(TowerType towerType, ModifierTowerCategory category)
	{
		if (category == ModifierTowerCategory.Any)
		{
			return true;
		}

		return category switch
		{
			ModifierTowerCategory.Melee => towerType == TowerType.Melee,
			ModifierTowerCategory.Projectile => towerType == TowerType.Projectile,
			ModifierTowerCategory.Explosive => towerType == TowerType.Explosive,
			_ => false,
		};
	}

	private void HandleConfirm()
	{
		if (!IsOpen)
		{
			return;
		}

		TowerModifierSO selected = SelectedItem;
		OnItemConfirmed?.Invoke(selected);
		Close();
	}

	private void HandleCancel()
	{
		if (!IsOpen)
		{
			return;
		}

		Close();
	}
}
