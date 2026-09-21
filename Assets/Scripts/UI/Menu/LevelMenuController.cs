using System;
using PrimeTween;
using TMPro;
using UnityEngine;

public class LevelMenuController : MonoBehaviour
{
	[Serializable]
	public class LevelEntry
	{
		public LevelSO levelSO;
		public RectTransform button;
	}

	[Header("Level Items")]
	[SerializeField]
	private LevelEntry[] _levelEntries;

	[Header("Back Button")]
	[SerializeField]
	private RectTransform _backButton;

	[Header("Cursor Indicator")]
	[SerializeField]
	private RectTransform _cursor;

	[SerializeField]
	private float _cursorXOffset = -8f;

	[Header("Visual Settings")]
	[SerializeField]
	private Color _unlockedColor = Color.white;

	[SerializeField]
	private Color _lockedColor = new Color(0.45f, 0.45f, 0.45f, 0.5f);

	private TextMeshProUGUI[] _entryTexts;
	private int _currentIndex;
	private bool _isActive;
	private Sequence _cursorTween;

	public event Action<LevelSO> OnLevelSelected;
	public event Action OnBackRequested;

	private int TotalItems => _levelEntries.Length + 1;

	private void Awake()
	{
		CacheEntryTexts();
	}

	private void CacheEntryTexts()
	{
		if (_entryTexts != null)
		{
			return;
		}

		_entryTexts = new TextMeshProUGUI[_levelEntries.Length];
		for (int i = 0; i < _levelEntries.Length; i++)
		{
			_entryTexts[i] = _levelEntries[i].button.GetComponent<TextMeshProUGUI>();
		}
	}

	private void OnDisable()
	{
		_cursorTween.Stop();
	}

	public void Open()
	{
		_isActive = true;
		_currentIndex = 0;
		UpdateLevelVisuals();
		UpdateCursorPosition();
	}

	public void Close()
	{
		_isActive = false;
		_cursorTween.Stop();
	}

	public bool IsLevelUnlocked(int index)
	{
		if (index == 0)
		{
			return true;
		}

		LevelSO prerequisite = _levelEntries[index].levelSO.PrerequisiteLevel;
		if (prerequisite != null)
		{
			return LevelProgression.IsLevelCompleted(prerequisite);
		}

		return LevelProgression.IsLevelCompleted(_levelEntries[index - 1].levelSO);
	}

	public void UpdateLevelVisuals()
	{
		CacheEntryTexts();
		for (int i = 0; i < _levelEntries.Length; i++)
		{
			_entryTexts[i].color = IsLevelUnlocked(i) ? _unlockedColor : _lockedColor;
		}
	}

	public void HandleNavigation(Vector2 direction)
	{
		if (!_isActive)
		{
			return;
		}

		if (direction.y != 0)
		{
			int step = direction.y > 0 ? -1 : 1;
			_currentIndex = (_currentIndex + step + TotalItems) % TotalItems;
			UpdateCursorPosition();
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SelectorClick_Sfx);
		}
	}

	public void HandleConfirm()
	{
		if (!_isActive)
		{
			return;
		}

		if (_currentIndex < _levelEntries.Length)
		{
			if (!IsLevelUnlocked(_currentIndex))
			{
				AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CantClick_Sfx);
				return;
			}

			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CompleteClick_Sfx);
			GameManager.SelectedLevel = _levelEntries[_currentIndex].levelSO;
			OnLevelSelected?.Invoke(_levelEntries[_currentIndex].levelSO);
		}
		else
		{
			OnBackRequested?.Invoke();
		}
	}

	public void HandleCancel()
	{
		if (!_isActive)
		{
			return;
		}

		OnBackRequested?.Invoke();
	}

	private void UpdateCursorPosition()
	{
		RectTransform target = GetTargetRect(_currentIndex);
		_cursorTween.Stop();
		float leftEdge = target.anchoredPosition.x - (target.rect.width * target.pivot.x);
		_cursor.anchoredPosition = new Vector2(leftEdge + _cursorXOffset, target.anchoredPosition.y);

		float originX = _cursor.anchoredPosition.x;
		_cursorTween = Sequence
			.Create(-1, Sequence.SequenceCycleMode.Yoyo, useUnscaledTime: true)
			.Chain(Tween.UIAnchoredPositionX(_cursor, originX - 2f, 0.35f, Ease.InOutSine))
			.Chain(Tween.UIAnchoredPositionX(_cursor, originX, 0.35f, Ease.InOutSine));
	}

	private RectTransform GetTargetRect(int index)
	{
		return index < _levelEntries.Length ? _levelEntries[index].button : _backButton;
	}

	[ContextMenu("Clear Progression")]
	private void ContextClearProgression()
	{
		for (int i = 0; i < _levelEntries.Length; i++)
		{
			LevelProgression.ClearLevel(_levelEntries[i].levelSO);
		}
		UpdateLevelVisuals();
	}

	[ContextMenu("Complete All Levels")]
	private void ContextCompleteAll()
	{
		for (int i = 0; i < _levelEntries.Length; i++)
		{
			LevelProgression.CompleteLevel(_levelEntries[i].levelSO);
		}
		UpdateLevelVisuals();
	}
}
