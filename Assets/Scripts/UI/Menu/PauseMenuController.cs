using System;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
	private enum MenuMode
	{
		Main,
		Settings,
	}

	[Header("Menu Panels")]
	[SerializeField]
	private GameObject _menuRoot;

	[Header("Buttons")]
	[SerializeField]
	private RectTransform[] _buttons;

	[SerializeField]
	private TextMeshProUGUI[] _buttonTexts;

	[Header("Cursor Indicator")]
	[SerializeField]
	private RectTransform _cursor;

	[SerializeField]
	private CursorAnimator _cursorAnimator;

	[SerializeField]
	private float _cursorXOffset = -8f;

	private const int RowBgm = 0;
	private const int RowSfx = 1;
	private const int RowAmb = 2;
	private const int RowBack = 3;
	private const float VolumeStep = 0.1f;

	public static PauseMenuController Instance { get; private set; }
	public bool IsOpen => _isOpen;

	private MenuMode _currentMode = MenuMode.Main;
	private int _currentIndex;
	private bool _isOpen;
	private Vector2Int _lastDirection;
	private float _holdTimer;
	private EventInstance _testSoundInstance;

	private void Awake()
	{
		Instance = this;
		_menuRoot.SetActive(false);
	}

	private void Start()
	{
		GameManager.Instance.OpenPauseMenu += TogglePauseMenu;
		GameManager.Instance.OnWin += Close;
		GameManager.Instance.OnLose += Close;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}

		if (GameManager.Instance != null)
		{
			GameManager.Instance.OpenPauseMenu -= TogglePauseMenu;
			GameManager.Instance.OnWin -= Close;
			GameManager.Instance.OnLose -= Close;
		}

		UnsubscribeInput();
		StopTestSound();
	}

	public void TogglePauseMenu()
	{
		if (GameManager.Instance.HasWon || GameManager.Instance.HasLost)
		{
			return;
		}

		if (_isOpen)
		{
			if (_currentMode == MenuMode.Settings)
			{
				AudioManager.Instance.SaveAudioPref();
				SetMode(MenuMode.Main);
			}
			else
			{
				HandleConfirm();
			}
		}
		else
		{
			Open();
		}
	}

	public void Open()
	{
		if (GameManager.Instance.HasWon || GameManager.Instance.HasLost)
		{
			return;
		}

		if (TowerSelector.Instance.IsOpen)
		{
			TowerSelector.Instance.Close(resumeTime: false);
		}
		if (ItemSelector.Instance.IsOpen)
		{
			ItemSelector.Instance.Close(resumeTime: false);
		}

		_isOpen = true;
		Time.timeScale = 0f;
		_menuRoot.SetActive(true);
		SetMode(MenuMode.Main);
		SubscribeInput();
	}

	public void Close()
	{
		_isOpen = false;
		StopTestSound();
		AudioManager.Instance.SaveAudioPref();
		UnsubscribeInput();
		_menuRoot.SetActive(false);
	}

	public void Resume()
	{
		Close();
		GameManager.Instance.ResumeGame();
	}

	private void SetMode(MenuMode mode)
	{
		_currentMode = mode;
		_currentIndex = 0;
		UpdateDisplays();
		UpdateCursorPosition();
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

	private void UpdateDisplays()
	{
		if (_currentMode == MenuMode.Main)
		{
			_buttonTexts[0].text = "RESUME";
			_buttonTexts[1].text = "SETTINGS";
			_buttonTexts[2].text = "RESTART";
			_buttonTexts[3].text = "MAIN MENU";
		}
		else
		{
			float bgm = AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Music);
			float sfx = AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Game);
			float amb = AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Ambience);

			_buttonTexts[RowBgm].text = $"BGM    {FormatVolumeBar(bgm)}";
			_buttonTexts[RowSfx].text = $"SFX    {FormatVolumeBar(sfx)}";
			_buttonTexts[RowAmb].text = $"AMB    {FormatVolumeBar(amb)}";
			_buttonTexts[RowBack].text = "BACK";
		}
	}

	private string FormatVolumeBar(float volume)
	{
		int notches = Mathf.RoundToInt(volume * 10f);
		int percent = notches * 10;
		return $"{percent}%";
	}

	private void HandleMovement(Vector2 direction)
	{
		if (!_isOpen)
		{
			return;
		}

		Vector2Int dir = Vector2Int.zero;
		if (Mathf.Abs(direction.y) >= 0.5f)
		{
			dir.y = direction.y > 0 ? 1 : -1;
		}
		else if (Mathf.Abs(direction.x) >= 0.5f)
		{
			dir.x = direction.x > 0 ? 1 : -1;
		}

		if (dir == Vector2Int.zero)
		{
			_lastDirection = Vector2Int.zero;
			_holdTimer = 0f;
			return;
		}

		if (dir != _lastDirection)
		{
			_lastDirection = dir;
			_holdTimer = 0.25f;
			NavigateDirection(dir);
		}
		else
		{
			_holdTimer -= Time.unscaledDeltaTime;
			if (_holdTimer <= 0f)
			{
				_holdTimer = 0.12f;
				NavigateDirection(dir);
			}
		}
	}

	private void NavigateDirection(Vector2Int dir)
	{
		if (dir.y != 0)
		{
			StopTestSound();
			int step = dir.y > 0 ? -1 : 1;
			_currentIndex = (_currentIndex + step + _buttons.Length) % _buttons.Length;
			UpdateCursorPosition();
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SelectorClick_Sfx);
			return;
		}

		if (dir.x != 0 && _currentMode == MenuMode.Settings)
		{
			if (_currentIndex != RowBack)
			{
				AdjustVolume(dir.x > 0 ? VolumeStep : -VolumeStep);
				PlaySliderTestSound(_currentIndex);
			}
		}
	}

	private void HandleConfirm()
	{
		if (!_isOpen)
		{
			return;
		}

		if (_currentMode == MenuMode.Main)
		{
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CompleteClick_Sfx);
			switch (_currentIndex)
			{
				case 0:
					Resume();
					break;
				case 1:
					SetMode(MenuMode.Settings);
					break;
				case 2:
					Close();
					GameManager.Instance.RestartLevel();
					break;
				case 3:
					Close();
					GameManager.Instance.ReturnToMainMenu();
					break;
			}
		}
		else
		{
			if (_currentIndex == RowBack)
			{
				AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CantClick_Sfx);
				AudioManager.Instance.SaveAudioPref();
				SetMode(MenuMode.Main);
			}
		}
	}

	private void HandleCancel()
	{
		if (!_isOpen)
		{
			return;
		}

		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CantClick_Sfx);

		if (_currentMode == MenuMode.Settings)
		{
			AudioManager.Instance.SaveAudioPref();
			SetMode(MenuMode.Main);
		}
		else
		{
			Resume();
		}
	}

	private void AdjustVolume(float delta)
	{
		AudioManager.AudioBusType busType = _currentIndex switch
		{
			RowBgm => AudioManager.AudioBusType.Music,
			RowSfx => AudioManager.AudioBusType.Game,
			RowAmb => AudioManager.AudioBusType.Ambience,
			_ => AudioManager.AudioBusType.Game,
		};

		float current = AudioManager.Instance.GetVolume(busType);
		float next = Mathf.Clamp01((float)Math.Round(current + delta, 1));
		AudioManager.Instance.SetVolume(busType, next);
		UpdateDisplays();
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

	private void UpdateCursorPosition()
	{
		RectTransform target = _buttons[_currentIndex];
		float leftEdge = target.anchoredPosition.x - (target.rect.width * target.pivot.x);
		_cursor.anchoredPosition = new Vector2(leftEdge + _cursorXOffset, target.anchoredPosition.y);
		_cursorAnimator.Play();
	}
}
