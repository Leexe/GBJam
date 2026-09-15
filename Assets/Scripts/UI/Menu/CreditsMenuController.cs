using System;
using UnityEngine;

public class CreditsMenuController : MonoBehaviour
{
	[Header("Cursor Indicator")]
	[SerializeField]
	private RectTransform _cursor;

	[SerializeField]
	private RectTransform _backButton;

	[SerializeField]
	private float _cursorXOffset = -8f;

	private bool _isActive;

	public Action OnBackRequested;

	public void Open()
	{
		_isActive = true;
		UpdateCursorPosition();
	}

	public void Close()
	{
		_isActive = false;
	}

	public void HandleConfirm()
	{
		if (!_isActive)
		{
			return;
		}

		OnBackRequested?.Invoke();
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
		if (_cursor != null && _backButton != null)
		{
			float leftEdge = _backButton.anchoredPosition.x - (_backButton.rect.width * _backButton.pivot.x);
			_cursor.anchoredPosition = new Vector2(leftEdge + _cursorXOffset, _backButton.anchoredPosition.y);
		}
	}
}
