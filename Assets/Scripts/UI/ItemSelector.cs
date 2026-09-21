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

	[SerializeField]
	private TextMeshProUGUI _ownText;

	[Header("Tower Icons")]
	[SerializeField]
	private List<TowerIcon> _towerIcons = new();

	[Header("Navigation Settings")]
	[SerializeField]
	private float _repeatDelay = 0.25f;

	[SerializeField]
	private float _repeatRate = 0.1f;

	private readonly List<TowerModifierSO> _offeredItems = new();
	private List<TowerModifierSO> _items = new();
	private int _selectedIndex;
	private bool _isHolding;
	private Vector2Int _currentDirection;
	private float _holdTimer;
	private Sequence _arrowAnim;
	private bool _canCancel = true;

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
	}

	private void Start()
	{
		GameManager.Instance.WaveController.OnWaveItemsOffered += HandleWaveItemsOffered;
		GameManager.Instance.OnWin += HandleGameEnded;
		GameManager.Instance.OnLose += HandleGameEnded;
		InputManager.Instance.OnSelect += HandleSelect;
		_menuRoot.SetActive(false);
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}

		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnSelect -= HandleSelect;
		}

		if (GameManager.Instance != null)
		{
			if (GameManager.Instance.WaveController != null)
			{
				GameManager.Instance.WaveController.OnWaveItemsOffered -= HandleWaveItemsOffered;
			}
			GameManager.Instance.OnWin -= HandleGameEnded;
			GameManager.Instance.OnLose -= HandleGameEnded;
		}

		if (IsOpen)
		{
			Time.timeScale = 1f;
			UnsubscribeInput();
		}
	}

	private void HandleGameEnded()
	{
		if (IsOpen)
		{
			Close(resumeTime: false);
		}
	}

	private void HandleSelect()
	{
		if (GameManager.Instance.HasWon || GameManager.Instance.HasLost || PauseMenuController.Instance.IsOpen)
		{
			return;
		}

		if (IsOpen)
		{
			if (_canCancel)
			{
				AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CantClick_Sfx);
				Close();
			}
			return;
		}

		OpenWithCurrentItems(canCancel: _offeredItems.Count == 0);
	}

	private void HandleWaveItemsOffered(int waveIndex, List<TowerModifierSO> choices)
	{
		if (GameManager.Instance.HasWon || GameManager.Instance.HasLost)
		{
			return;
		}

		_offeredItems.Clear();
		_offeredItems.AddRange(choices);
		OpenWithCurrentItems(canCancel: false);
	}

	public void OpenWithCurrentItems(bool canCancel)
	{
		if (IsOpen || GameManager.Instance.HasWon || GameManager.Instance.HasLost)
		{
			return;
		}

		if (TowerSelector.Instance.IsOpen)
		{
			TowerSelector.Instance.Close(resumeTime: false);
		}

		_items.Clear();
		_items.AddRange(_offeredItems);
		_items.AddRange(GameManager.Instance.ModifierManager.AcquiredModifiers);

		if (_items.Count == 0)
		{
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CantClick_Sfx);
			return;
		}

		_canCancel = canCancel;
		IsOpen = true;
		_isHolding = false;
		_currentDirection = Vector2Int.zero;
		_holdTimer = 0f;

		Time.timeScale = 0f;
		_menuRoot.SetActive(true);

		SetSelected(0);
		SubscribeInput();

		OnOpened?.Invoke();
	}

	public void Open(List<TowerModifierSO> items, int initialIndex = 0, bool canCancel = true)
	{
		_offeredItems.Clear();
		_offeredItems.AddRange(items);
		OpenWithCurrentItems(canCancel);
		if (initialIndex > 0 && initialIndex < _items.Count)
		{
			SetSelected(initialIndex);
		}
	}

	public void Open(int choicesCount = 3)
	{
		Open(GenerateRandomItems(choicesCount));
	}

	public void Open()
	{
		Open(GameManager.Instance.Level.DefaultItemChoicesCount);
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

		if (resumeTime)
		{
			Time.timeScale = 1f;
		}
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
				CycleItem(_currentDirection.x);
			}

			_holdTimer -= _repeatRate;
		}
	}

	private void HandleMovement(Vector2 input)
	{
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
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SelectorClick_Sfx);
	}

	public void SetSelected(int index)
	{
		_selectedIndex = Mathf.Clamp(index, 0, _items.Count - 1);
		TowerModifierSO selected = _items[_selectedIndex];

		_nameText.text = selected.Name;
		_descriptionText.text = selected.Description;
		_ownText.text = _selectedIndex >= _offeredItems.Count ? "Owned" : "New";

		UpdateTowerIcons(selected);

		OnItemChanged?.Invoke(selected);
	}

	private void UpdateTowerIcons(TowerModifierSO item)
	{
		for (int i = 0; i < _towerIcons.Count; i++)
		{
			TowerIcon icon = _towerIcons[i];
			if (icon.Tower == null)
			{
				icon.SetTower(GameManager.Instance.Level.TowerPool[i]);
			}

			icon.SetActive(IsTowerAffected(icon.TowerType, item.Category));
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
		if (_selectedIndex >= _offeredItems.Count)
		{
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CantClick_Sfx);
			return;
		}

		TowerModifierSO selected = SelectedItem;
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CompleteClick_Sfx);
		OnItemConfirmed?.Invoke(selected);
		GameManager.Instance.ModifierManager.SelectModifier(selected);
		_offeredItems.Clear();
		GameManager.Instance.WaveController.ResumeSpawning();
		Close();
	}

	private void HandleCancel()
	{
		if (!_canCancel)
		{
			return;
		}

		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CantClick_Sfx);
		Close();
	}
}
