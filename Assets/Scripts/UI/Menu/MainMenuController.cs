using PrimeTween;
using UnityEngine;
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

	[Header("Canvas & Fade")]
	[SerializeField] private Canvas _canvas;
	[SerializeField] private CanvasGroup _fadeOverlay;

	[Header("Panels")]
	[SerializeField] private GameObject _splashPanel;
	[SerializeField] private GameObject _mainMenuPanel;
	[SerializeField] private GameObject _settingsPanel;
	[SerializeField] private GameObject _creditsPanel;

	[Header("Sub Controllers")]
	[SerializeField] private SettingsMenuController _settingsController;
	[SerializeField] private CreditsMenuController _creditsController;

	[Header("Menu Items")]
	[SerializeField] private RectTransform[] _menuItems;
	[SerializeField] private RectTransform _mainCursor;
	[SerializeField] private float _cursorXOffset = -8f;

	[Header("Settings")]
	[SerializeField] private float _splashDuration = 1.8f;
	[SerializeField] private string _gameplaySceneName = "Main";

	private MenuState _currentState = MenuState.Splash;
	private int _currentMenuItem;
	private float _splashTimer;
	private bool _isTransitioning;
	private bool _isSubscribed;
	private Sequence _cursorTween;
	private Vector2Int _lastDirection;
	private float _holdTimer;

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
		TrySubscribe();
		if (_settingsController != null) _settingsController.OnBackRequested += ReturnToMainMenu;
		if (_creditsController != null) _creditsController.OnBackRequested += ReturnToMainMenu;
	}

	private void OnDisable()
	{
		Unsubscribe();
		if (_settingsController != null) _settingsController.OnBackRequested -= ReturnToMainMenu;
		if (_creditsController != null) _creditsController.OnBackRequested -= ReturnToMainMenu;
		_cursorTween.Stop();
	}

	private void Start()
	{
		TrySubscribe();
		SetState(MenuState.Splash);

		if (_fadeOverlay != null)
		{
			_fadeOverlay.alpha = 1f;
			Tween.Alpha(_fadeOverlay, 0f, 0.25f, useUnscaledTime: true);
		}
	}

	private void Update()
	{
		if (!_isSubscribed)
		{
			TrySubscribe();
		}

		if (_currentState == MenuState.Splash)
		{
			_splashTimer += Time.deltaTime;
			if (_splashTimer >= _splashDuration)
			{
				SetState(MenuState.Main);
			}
		}
	}

	private void TrySubscribe()
	{
		if (_isSubscribed || InputManager.Instance == null) return;

		InputManager.Instance.OnMovement += HandleMovement;
		InputManager.Instance.OnConfirm += HandleConfirm;
		InputManager.Instance.OnCancel += HandleCancel;
		InputManager.Instance.OnStart += HandleConfirm;
		_isSubscribed = true;
	}

	private void Unsubscribe()
	{
		if (!_isSubscribed || InputManager.Instance == null) return;

		InputManager.Instance.OnMovement -= HandleMovement;
		InputManager.Instance.OnConfirm -= HandleConfirm;
		InputManager.Instance.OnCancel -= HandleCancel;
		InputManager.Instance.OnStart -= HandleConfirm;
		_isSubscribed = false;
	}

	private void HandleMovement(Vector2 direction)
	{
		if (_isTransitioning) return;

		Vector2Int dir = Vector2Int.zero;
		if (Mathf.Abs(direction.y) >= 0.5f) dir.y = direction.y > 0 ? 1 : -1;
		else if (Mathf.Abs(direction.x) >= 0.5f) dir.x = direction.x > 0 ? 1 : -1;

		if (dir == Vector2Int.zero)
		{
			_lastDirection = Vector2Int.zero;
			_holdTimer = 0f;
			return;
		}

		if (_currentState == MenuState.Splash)
		{
			SetState(MenuState.Main);
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
		if (_currentState == MenuState.Settings && _settingsController != null)
		{
			_settingsController.HandleNavigation(dir);
		}
		else if (_currentState == MenuState.Main && dir.y != 0 && _menuItems != null && _menuItems.Length > 0)
		{
			int step = dir.y > 0 ? -1 : 1;
			_currentMenuItem = (_currentMenuItem + step + _menuItems.Length) % _menuItems.Length;
			UpdateCursorPosition();
		}
	}

	private void HandleConfirm()
	{
		if (_isTransitioning) return;

		if (_currentState == MenuState.Splash)
		{
			SetState(MenuState.Main);
			return;
		}

		switch (_currentState)
		{
			case MenuState.Main:
				ExecuteMenuItem(_currentMenuItem);
				break;
			case MenuState.Settings:
				if (_settingsController != null) _settingsController.HandleConfirm();
				break;
			case MenuState.Credits:
				if (_creditsController != null) _creditsController.HandleConfirm();
				break;
		}
	}

	private void HandleCancel()
	{
		if (_isTransitioning) return;

		if (_currentState == MenuState.Splash)
		{
			SetState(MenuState.Main);
			return;
		}

		switch (_currentState)
		{
			case MenuState.Settings:
				if (_settingsController != null) _settingsController.HandleCancel();
				break;
			case MenuState.Credits:
				if (_creditsController != null) _creditsController.HandleCancel();
				break;
		}
	}

	private void ExecuteMenuItem(int index)
	{
		switch (index)
		{
			case 0:
				PlayGame();
				break;
			case 1:
				SetState(MenuState.Settings);
				break;
			case 2:
				SetState(MenuState.Credits);
				break;
			case 3:
				QuitApplication();
				break;
		}
	}

	private void PlayGame()
	{
		_isTransitioning = true;
		if (_fadeOverlay != null)
		{
			Tween.Alpha(_fadeOverlay, 1f, 0.25f, useUnscaledTime: true)
				.OnComplete(() => SceneManager.LoadScene(_gameplaySceneName));
		}
		else
		{
			SceneManager.LoadScene(_gameplaySceneName);
		}
	}

	private void ReturnToMainMenu() => SetState(MenuState.Main);

	private void SetState(MenuState newState)
	{
		_currentState = newState;

		if (_splashPanel != null) _splashPanel.SetActive(newState == MenuState.Splash);
		if (_mainMenuPanel != null) _mainMenuPanel.SetActive(newState == MenuState.Main);

		if (_settingsPanel != null)
		{
			_settingsPanel.SetActive(newState == MenuState.Settings);
			if (_settingsController != null)
			{
				if (newState == MenuState.Settings) _settingsController.Open();
				else _settingsController.Close();
			}
		}

		if (_creditsPanel != null)
		{
			_creditsPanel.SetActive(newState == MenuState.Credits);
			if (_creditsController != null)
			{
				if (newState == MenuState.Credits) _creditsController.Open();
				else _creditsController.Close();
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
		if (_mainCursor == null || _menuItems == null || _menuItems.Length == 0) return;

		RectTransform target = _menuItems[_currentMenuItem];
		if (target == null) return;

		_cursorTween.Stop();
		float leftEdge = target.anchoredPosition.x - (target.rect.width * target.pivot.x);
		_mainCursor.anchoredPosition = new Vector2(leftEdge + _cursorXOffset, target.anchoredPosition.y);

		float originX = _mainCursor.anchoredPosition.x;
		_cursorTween = Sequence.Create(-1, Sequence.SequenceCycleMode.Yoyo, useUnscaledTime: true)
			.Chain(Tween.UIAnchoredPositionX(_mainCursor, originX - 2f, 0.35f, Ease.InOutSine))
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
