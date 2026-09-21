using System;
using FMOD.Studio;
using FMODUnity;
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
	private TextMeshProUGUI _ambienceText;

	[SerializeField]
	private TextMeshProUGUI _backText;

	[Header("Cursor Indicator")]
	[SerializeField]
	private RectTransform _cursor;

	[SerializeField]
	private float _cursorXOffset = -8f;

	private const int RowBgm = 0;
	private const int RowSfx = 1;
	private const int RowAmb = 2;
	private const int RowBack = 3;
	private const int TotalRows = 4;
	private const float VolumeStep = 0.1f;

	private int _currentRow;
	private bool _isActive;
	private EventInstance _testSoundInstance;

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
		StopTestSound();
		AudioManager.Instance.SaveAudioPref();
	}

	private void OnDisable()
	{
		StopTestSound();
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
			StopTestSound();
			int step = direction.y > 0 ? -1 : 1;
			_currentRow = (_currentRow + step + TotalRows) % TotalRows;
			UpdateCursorPosition();
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SelectorClick_Sfx);
			return;
		}

		if (direction.x != 0)
		{
			if (_currentRow != RowBack)
			{
				AdjustVolume(direction.x > 0 ? VolumeStep : -VolumeStep);
				PlaySliderTestSound(_currentRow);
			}
		}
	}

	private void PlaySliderTestSound(int row)
	{
		StopTestSound();

		EventReference sound = row switch
		{
			RowBgm => FMODEvents.Instance.BgmTest_Bgm,
			RowSfx => FMODEvents.Instance.SfxTest_Sfx,
			RowAmb => FMODEvents.Instance.AmbTest_Amb,
			_ => default,
		};

		if (!sound.IsNull)
		{
			_testSoundInstance = AudioManager.Instance.CreateInstance(sound);
			_testSoundInstance.start();
		}
	}

	private void StopTestSound()
	{
		if (_testSoundInstance.isValid())
		{
			_testSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			_testSoundInstance.release();
			_testSoundInstance.clearHandle();
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
		AudioManager.AudioBusType busType;
		switch (_currentRow)
		{
			case RowBgm:
				busType = AudioManager.AudioBusType.Music;
				break;
			case RowSfx:
				busType = AudioManager.AudioBusType.Game;
				break;
			case RowAmb:
				busType = AudioManager.AudioBusType.Ambience;
				break;
			default:
				return;
		}

		float current = AudioManager.Instance.GetVolume(busType);
		float next = Mathf.Clamp01((float)Math.Round(current + delta, 1));
		AudioManager.Instance.SetVolume(busType, next);
		UpdateDisplays();
	}

	private void UpdateDisplays()
	{
		float bgm = AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Music);
		float sfx = AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Game);
		float amb = AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Ambience);

		if (_bgmText != null)
		{
			_bgmText.text = $"BGM {FormatVolumeBar(bgm)}";
		}

		if (_sfxText != null)
		{
			_sfxText.text = $"SFX {FormatVolumeBar(sfx)}";
		}

		if (_ambienceText != null)
		{
			_ambienceText.text = $"AMB {FormatVolumeBar(amb)}";
		}
	}

	private string FormatVolumeBar(float volume)
	{
		int notches = Mathf.RoundToInt(volume * 10f);
		int percent = notches * 10;
		return $"{percent, 3}%";
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
			RowAmb => _ambienceText != null ? _ambienceText.rectTransform : null,
			_ => _backText != null ? _backText.rectTransform : null,
		};

		if (target != null)
		{
			float leftEdge = target.anchoredPosition.x - (target.rect.width * target.pivot.x);
			_cursor.anchoredPosition = new Vector2(leftEdge + _cursorXOffset, target.anchoredPosition.y);
		}
	}
}
