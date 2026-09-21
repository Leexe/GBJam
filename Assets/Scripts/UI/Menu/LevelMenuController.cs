using System;
using PrimeTween;
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

	private int _currentIndex;
	private bool _isActive;
	private Sequence _cursorTween;

	public event Action<LevelSO> OnLevelSelected;
	public event Action OnBackRequested;

	private int TotalItems => _levelEntries.Length + 1;

	private void OnDisable()
	{
		_cursorTween.Stop();
	}

	public void Open()
	{
		_isActive = true;
		_currentIndex = 0;
		UpdateCursorPosition();
	}

	public void Close()
	{
		_isActive = false;
		_cursorTween.Stop();
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
}
