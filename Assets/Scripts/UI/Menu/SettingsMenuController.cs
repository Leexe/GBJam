using System;
using TMPro;
using UnityEngine;

public class SettingsMenuController : MonoBehaviour
{
	[Header("Text Displays")]
	[SerializeField]
	private TextMeshProUGUI _bgmText;

	[SerializeField]
	private TextMeshProUGUI _sfxText;

	[SerializeField]
	private TextMeshProUGUI _backText;

	[Header("Cursor Indicator")]
	[SerializeField]
	private RectTransform _cursor;

	[SerializeField]
	private float _cursorXOffset = -8f;

	private const int RowBgm = 0;
	private const int RowSfx = 1;
	private const int RowBack = 2;
	private const int TotalRows = 3;
	private const float VolumeStep = 0.1f;

	private int _currentRow;
	private bool _isActive;

	public event Action OnBackRequested;

	public void Open()
	{
		_isActive = true;
		_currentRow = 0;
		UpdateDisplays();
		UpdateCursorPosition();
	}

	public void Close()
	{
		_isActive = false;
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.SaveAudioPref();
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
			_currentRow = (_currentRow + step + TotalRows) % TotalRows;
			UpdateCursorPosition();
			return;
		}

		if (direction.x != 0)
		{
			AdjustVolume(direction.x > 0 ? VolumeStep : -VolumeStep);
		}
	}

	public void HandleConfirm()
	{
		if (!_isActive)
		{
			return;
		}

		if (_currentRow == RowBack)
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

	private void AdjustVolume(float delta)
	{
		if (AudioManager.Instance == null)
		{
			return;
		}

		AudioManager.AudioBusType busType = _currentRow == RowBgm
			? AudioManager.AudioBusType.Music
			: AudioManager.AudioBusType.Game;

		if (_currentRow != RowBgm && _currentRow != RowSfx)
		{
			return;
		}

		float current = AudioManager.Instance.GetVolume(busType);
		float next = Mathf.Clamp01((float)Math.Round(current + delta, 1));
		AudioManager.Instance.SetVolume(busType, next);
		UpdateDisplays();
	}

	private void UpdateDisplays()
	{
		float bgm = AudioManager.Instance != null ? AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Music) : 0.7f;
		float sfx = AudioManager.Instance != null ? AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Game) : 0.7f;

		if (_bgmText != null)
		{
			_bgmText.text = $"BGM {FormatVolumeBar(bgm)}";
		}

		if (_sfxText != null)
		{
			_sfxText.text = $"SFX {FormatVolumeBar(sfx)}";
		}
	}

	private string FormatVolumeBar(float volume)
	{
		int notches = Mathf.RoundToInt(volume * 10f);
		int percent = notches * 10;
		return $"{percent,3}%";
	}

	private void UpdateCursorPosition()
	{
		if (_cursor == null)
		{
			return;
		}

		RectTransform target = _currentRow switch
		{
			RowBgm => _bgmText != null ? _bgmText.rectTransform : null,
			RowSfx => _sfxText != null ? _sfxText.rectTransform : null,
			_ => _backText != null ? _backText.rectTransform : null,
		};

		if (target != null)
		{
			float leftEdge = target.anchoredPosition.x - (target.rect.width * target.pivot.x);
			_cursor.anchoredPosition = new Vector2(leftEdge + _cursorXOffset, target.anchoredPosition.y);
		}
	}
}
