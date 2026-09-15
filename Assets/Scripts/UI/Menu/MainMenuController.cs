using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
	private enum MenuState
	{
		Splash,
		Main,
		Settings,
		Credits,
	}

	[Header("Canvas Setup")]
	[SerializeField]
	private Canvas _canvas;

	[Header("Panels")]
	[SerializeField]
	private GameObject _splashPanel;

	[SerializeField]
	private GameObject _mainMenuPanel;

	[SerializeField]
	private GameObject _settingsPanel;

	[SerializeField]
	private GameObject _creditsPanel;

	[Header("Sub Controllers")]
	[SerializeField]
	private SettingsMenuController _settingsController;

	[SerializeField]
	private CreditsMenuController _creditsController;

	[Header("Splash Settings")]
	[SerializeField]
	private float _splashDuration = 1.8f;

	[Header("Main Menu Items")]
	[SerializeField]
	private RectTransform[] _menuItems;

	[SerializeField]
	private RectTransform _mainCursor;

	[SerializeField]
	private float _cursorXOffset = -8f;

	[Header("Navigation Timings")]
	[SerializeField]
	private float _repeatDelay = 0.25f;

	[SerializeField]
	private float _repeatRate = 0.12f;

	[Header("Scene Loading")]
	[SerializeField]
	private string _gameplaySceneName = "Main";

	private MenuState _currentState = MenuState.Splash;
	private int _currentMenuItem;
	private float _splashTimer;
	private bool _hasSkippedSplash;
	private Sequence _cursorTween;
	private bool _isSubscribed;

	private Vector2Int _currentDirection;
	private bool _isHolding;
	private float _holdTimer;

	private const int MenuPlay = 0;
	private const int MenuSettings = 1;
	private const int MenuCredits = 2;
	private const int MenuQuit = 3;

	private void Awake()
	{
		if (_canvas == null)
		{
			_canvas = GetComponent<Canvas>();
		}

		if (_canvas != null)
		{
			_canvas.renderMode = RenderMode.ScreenSpaceCamera;
			if (_canvas.worldCamera == null)
			{
				_canvas.worldCamera = Camera.main;
			}
			_canvas.planeDistance = 10f;
		}
	}

	private void OnEnable()
	{
		SubscribeInput();

		if (_settingsController != null)
		{
			_settingsController.OnBackRequested += ReturnToMainMenu;
		}

		if (_creditsController != null)
		{
			_creditsController.OnBackRequested += ReturnToMainMenu;
		}
	}

	private void OnDisable()
	{
		UnsubscribeInput();

		if (_settingsController != null)
		{
			_settingsController.OnBackRequested -= ReturnToMainMenu;
		}

		if (_creditsController != null)
		{
			_creditsController.OnBackRequested -= ReturnToMainMenu;
		}

		_cursorTween.Stop();
	}

	private void Start()
	{
		// Scene loading order may invoke OnEnable before InputManager awake
		SubscribeInput();
		SetState(MenuState.Splash);
	}

	private void Update()
	{
		if (_currentState == MenuState.Splash && !_hasSkippedSplash)
		{
			_splashTimer += Time.deltaTime;
			if (_splashTimer >= _splashDuration)
			{
				SkipSplash();
			}
		}

		if (!_isSubscribed)
		{
			SubscribeInput();
		}

		if (_isHolding)
		{
			_holdTimer += Time.unscaledDeltaTime;
			if (_holdTimer >= _repeatDelay)
			{
				NavigateDirection(_currentDirection);
				_holdTimer -= _repeatRate;
			}
		}

		// Direct hardware poll ensures menu navigation is functional even if action maps desync
		if (!_isSubscribed)
		{
			PollHardwareFallback();
		}
	}

	private void SubscribeInput()
	{
		if (_isSubscribed || InputManager.Instance == null)
		{
			return;
		}

		InputManager.Instance.OnMovement += HandleMovement;
		InputManager.Instance.OnConfirm += HandleConfirm;
		InputManager.Instance.OnCancel += HandleCancel;
		InputManager.Instance.OnStart += HandleStart;
		_isSubscribed = true;
	}

	private void UnsubscribeInput()
	{
		if (!_isSubscribed || InputManager.Instance == null)
		{
			return;
		}

		InputManager.Instance.OnMovement -= HandleMovement;
		InputManager.Instance.OnConfirm -= HandleConfirm;
		InputManager.Instance.OnCancel -= HandleCancel;
		InputManager.Instance.OnStart -= HandleStart;
		_isSubscribed = false;
	}

	private void HandleMovement(Vector2 direction)
	{
		if (_currentState == MenuState.Splash)
		{
			if (direction != Vector2.zero)
			{
				SkipSplash();
			}
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
			_isHolding = false;
			_currentDirection = Vector2Int.zero;
			_holdTimer = 0f;
			return;
		}

		if (dir == _currentDirection && _isHolding)
		{
			return;
		}

		_currentDirection = dir;
		_isHolding = true;
		_holdTimer = 0f;
		NavigateDirection(dir);
	}

	private void NavigateDirection(Vector2Int dir)
	{
		if (_currentState == MenuState.Settings)
		{
			_settingsController.HandleNavigation(dir);
			return;
		}

		if (_currentState == MenuState.Main && dir.y != 0)
		{
			int step = dir.y > 0 ? -1 : 1;
			_currentMenuItem = (_currentMenuItem + step + _menuItems.Length) % _menuItems.Length;
			UpdateCursorPosition();
		}
	}

	private void PollHardwareFallback()
	{
		Keyboard keyboard = Keyboard.current;
		Gamepad gamepad = Gamepad.current;

		Vector2 dir = Vector2.zero;
		if (keyboard != null)
		{
			if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) dir.y = 1f;
			else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) dir.y = -1f;
			else if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) dir.x = -1f;
			else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) dir.x = 1f;

			if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
			{
				HandleConfirm();
			}
			if (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)
			{
				HandleCancel();
			}
		}

		if (gamepad != null)
		{
			if (gamepad.dpad.up.isPressed || gamepad.leftStick.up.isPressed) dir.y = 1f;
			else if (gamepad.dpad.down.isPressed || gamepad.leftStick.down.isPressed) dir.y = -1f;
			else if (gamepad.dpad.left.isPressed || gamepad.leftStick.left.isPressed) dir.x = -1f;
			else if (gamepad.dpad.right.isPressed || gamepad.leftStick.right.isPressed) dir.x = 1f;

			if (gamepad.buttonSouth.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame)
			{
				HandleConfirm();
			}
			if (gamepad.buttonEast.wasPressedThisFrame)
			{
				HandleCancel();
			}
		}

		HandleMovement(dir);
	}

	private void HandleConfirm()
	{
		if (_currentState == MenuState.Splash)
		{
			SkipSplash();
			return;
		}

		switch (_currentState)
		{
			case MenuState.Main:
				ExecuteMenuItem(_currentMenuItem);
				break;
			case MenuState.Settings:
				_settingsController.HandleConfirm();
				break;
			case MenuState.Credits:
				_creditsController.HandleConfirm();
				break;
		}
	}

	private void HandleStart()
	{
		if (_currentState == MenuState.Splash)
		{
			SkipSplash();
			return;
		}

		if (_currentState == MenuState.Main)
		{
			ExecuteMenuItem(_currentMenuItem);
		}
	}

	private void HandleCancel()
	{
		if (_currentState == MenuState.Splash)
		{
			SkipSplash();
			return;
		}

		switch (_currentState)
		{
			case MenuState.Settings:
				_settingsController.HandleCancel();
				break;
			case MenuState.Credits:
				_creditsController.HandleCancel();
				break;
		}
	}

	private void SkipSplash()
	{
		if (_hasSkippedSplash)
		{
			return;
		}

		_hasSkippedSplash = true;
		SetState(MenuState.Main);
	}

	private void ExecuteMenuItem(int index)
	{
		switch (index)
		{
			case MenuPlay:
				SceneManager.LoadScene(_gameplaySceneName);
				break;
			case MenuSettings:
				SetState(MenuState.Settings);
				break;
			case MenuCredits:
				SetState(MenuState.Credits);
				break;
			case MenuQuit:
				QuitApplication();
				break;
		}
	}

	private void ReturnToMainMenu()
	{
		SetState(MenuState.Main);
	}

	private void SetState(MenuState newState)
	{
		_currentState = newState;

		if (_splashPanel != null)
		{
			_splashPanel.SetActive(newState == MenuState.Splash);
		}

		if (_mainMenuPanel != null)
		{
			_mainMenuPanel.SetActive(newState == MenuState.Main);
		}

		if (_settingsPanel != null)
		{
			_settingsPanel.SetActive(newState == MenuState.Settings);
			if (newState == MenuState.Settings && _settingsController != null)
			{
				_settingsController.Open();
			}
			else if (_settingsController != null)
			{
				_settingsController.Close();
			}
		}

		if (_creditsPanel != null)
		{
			_creditsPanel.SetActive(newState == MenuState.Credits);
			if (newState == MenuState.Credits && _creditsController != null)
			{
				_creditsController.Open();
			}
			else if (_creditsController != null)
			{
				_creditsController.Close();
			}
		}

		if (newState == MenuState.Main)
		{
			UpdateCursorPosition();
		}
		else
		{
			_cursorTween.Stop();
		}
	}

	private void UpdateCursorPosition()
	{
		if (_mainCursor == null || _menuItems == null || _menuItems.Length == 0)
		{
			return;
		}

		RectTransform target = _menuItems[_currentMenuItem];
		if (target == null)
		{
			return;
		}

		_cursorTween.Stop();

		float leftEdge = target.anchoredPosition.x - (target.rect.width * target.pivot.x);
		_mainCursor.anchoredPosition = new Vector2(leftEdge + _cursorXOffset, target.anchoredPosition.y);

		StartCursorPulse();
	}

	private void StartCursorPulse()
	{
		_cursorTween.Stop();
		if (_mainCursor == null)
		{
			return;
		}

		float originX = _mainCursor.anchoredPosition.x;
		float pulseTargetX = originX - 2f;

		_cursorTween = Sequence.Create(-1, Sequence.SequenceCycleMode.Yoyo, useUnscaledTime: true)
			.Chain(Tween.UIAnchoredPositionX(_mainCursor, pulseTargetX, 0.35f, Ease.InOutSine))
			.Chain(Tween.UIAnchoredPositionX(_mainCursor, originX, 0.35f, Ease.InOutSine));
	}

	private void QuitApplication()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
}
