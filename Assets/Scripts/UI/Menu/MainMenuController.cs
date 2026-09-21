using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
	private enum MenuState
	{
		Splash,
		Main,
		Levels,
		Settings,
		Credits,
	}

	[Header("Canvas & Fade")]
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private CanvasGroup _fadeOverlay;

	[Header("Panels")]
	[SerializeField]
	private GameObject _splashPanel;

	[SerializeField]
	private GameObject _mainMenuPanel;

	[SerializeField]
	private GameObject _levelsPanel;

	[SerializeField]
	private GameObject _settingsPanel;

	[SerializeField]
	private GameObject _creditsPanel;

	[Header("Sub Controllers")]
	[SerializeField]
	private LevelMenuController _levelController;

	[SerializeField]
	private SettingsMenuController _settingsController;

	[SerializeField]
	private CreditsMenuController _creditsController;

	[Header("Menu Items")]
	[SerializeField]
	private RectTransform[] _menuItems;

	[SerializeField]
	private RectTransform _mainCursor;

	[SerializeField]
	private float _cursorXOffset = -8f;

	[Header("Settings")]
	[SerializeField]
	private float _splashDuration = 1.8f;

	[SerializeField]
	private float _fadeDuration = 2f;

	[SerializeField]
	private string _gameplaySceneName = "Game";

	private MenuState _currentState = MenuState.Splash;
	private int _currentMenuItem;
	private float _splashTimer;
	private bool _isTransitioning;
	private bool _isSubscribed;
	private Vector2Int _lastDirection;
	private float _holdTimer;
	private int _menuItemCount;

	private void Awake()
	{
		if (_canvas == null)
		{
			_canvas = GetComponent<Canvas>();
		}
		if (_canvas != null && _canvas.worldCamera == null)
		{
			_canvas.worldCamera = Camera.main;
		}
	}

	private void OnEnable()
	{
		if (InputManager.Instance != null)
		{
			SubscribeInput();
		}
		if (_settingsController != null)
			_settingsController.OnBackRequested += ReturnToMainMenu;
		if (_creditsController != null)
			_creditsController.OnBackRequested += ReturnToMainMenu;
		if (_levelController != null)
		{
			_levelController.OnBackRequested += ReturnToMainMenu;
			_levelController.OnLevelSelected += HandleLevelSelected;
		}
	}

	private void OnDisable()
	{
		UnsubscribeInput();
		if (_settingsController != null)
			_settingsController.OnBackRequested -= ReturnToMainMenu;
		if (_creditsController != null)
			_creditsController.OnBackRequested -= ReturnToMainMenu;
		if (_levelController != null)
		{
			_levelController.OnBackRequested -= ReturnToMainMenu;
			_levelController.OnLevelSelected -= HandleLevelSelected;
		}
	}

	private void Start()
	{
		if (!_isSubscribed)
		{
			SubscribeInput();
		}

#if UNITY_WEBGL
		_menuItems[_menuItems.Length - 1].gameObject.SetActive(false);
		_menuItemCount = _menuItems.Length - 1;
#else
		_menuItemCount = _menuItems.Length;
#endif

		SetState(MenuState.Splash);

		Tween.Alpha(_fadeOverlay, 0f, _fadeDuration, useUnscaledTime: true);
	}

	private void Update()
	{
		if (_currentState == MenuState.Splash)
		{
			_splashTimer += Time.deltaTime;
			if (_splashTimer >= _splashDuration)
			{
				SetState(MenuState.Main);
			}
		}
	}

	private void SubscribeInput()
	{
		if (_isSubscribed || InputManager.Instance == null)
			return;

		InputManager.Instance.OnMovement += HandleMovement;
		InputManager.Instance.OnConfirm += HandleConfirm;
		InputManager.Instance.OnCancel += HandleCancel;
		InputManager.Instance.OnStart += HandleConfirm;
		_isSubscribed = true;
	}

	private void UnsubscribeInput()
	{
		if (!_isSubscribed || InputManager.Instance == null)
			return;

		InputManager.Instance.OnMovement -= HandleMovement;
		InputManager.Instance.OnConfirm -= HandleConfirm;
		InputManager.Instance.OnCancel -= HandleCancel;
		InputManager.Instance.OnStart -= HandleConfirm;
		_isSubscribed = false;
	}

	private void HandleMovement(Vector2 direction)
	{
		if (_isTransitioning)
			return;

		Vector2Int dir = Vector2Int.zero;
		if (Mathf.Abs(direction.y) >= 0.5f)
			dir.y = direction.y > 0 ? 1 : -1;
		else if (Mathf.Abs(direction.x) >= 0.5f)
			dir.x = direction.x > 0 ? 1 : -1;

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
		else if (_currentState == MenuState.Levels && _levelController != null)
		{
			_levelController.HandleNavigation(dir);
		}
		else if (_currentState == MenuState.Main && dir.y != 0 && _menuItemCount > 0)
		{
			int step = dir.y > 0 ? -1 : 1;
			_currentMenuItem = (_currentMenuItem + step + _menuItemCount) % _menuItemCount;
			UpdateCursorPosition();
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SelectorClick_Sfx);
		}
	}

	private void HandleConfirm()
	{
		if (_isTransitioning)
			return;

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
			case MenuState.Levels:
				if (_levelController != null)
					_levelController.HandleConfirm();
				break;
			case MenuState.Settings:
				if (_settingsController != null)
					_settingsController.HandleConfirm();
				break;
			case MenuState.Credits:
				if (_creditsController != null)
					_creditsController.HandleConfirm();
				break;
		}
	}

	private void HandleCancel()
	{
		if (_isTransitioning)
			return;

		if (_currentState == MenuState.Splash)
		{
			SetState(MenuState.Main);
			return;
		}

		switch (_currentState)
		{
			case MenuState.Levels:
				if (_levelController != null)
					_levelController.HandleCancel();
				break;
			case MenuState.Settings:
				if (_settingsController != null)
					_settingsController.HandleCancel();
				break;
			case MenuState.Credits:
				if (_creditsController != null)
					_creditsController.HandleCancel();
				break;
		}
	}

	private void ExecuteMenuItem(int index)
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CompleteClick_Sfx);
		switch (index)
		{
			case 0:
				SetState(MenuState.Levels);
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

	private void HandleLevelSelected(LevelSO level)
	{
		PlayGame(level);
	}

	private void PlayGame(LevelSO level = null)
	{
		if (_isTransitioning)
		{
			return;
		}

		if (level != null)
		{
			GameManager.SelectedLevel = level;
		}
		_isTransitioning = true;
		StartCoroutine(TransitionToSceneRoutine(_gameplaySceneName));
	}

	private IEnumerator TransitionToSceneRoutine(string sceneName)
	{
		AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
		async.allowSceneActivation = false;

		Tween fadeTween = Tween.Alpha(_fadeOverlay, 1f, 0.25f, useUnscaledTime: true);

		while (fadeTween.isAlive || async.progress < 0.9f)
		{
			yield return null;
		}

		async.allowSceneActivation = true;
	}

	private void ReturnToMainMenu()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CantClick_Sfx);
		SetState(MenuState.Main);
	}

	private void SetState(MenuState newState)
	{
		_currentState = newState;

		if (_splashPanel != null)
			_splashPanel.SetActive(newState == MenuState.Splash);
		if (_mainMenuPanel != null)
			_mainMenuPanel.SetActive(newState == MenuState.Main);

		if (_levelsPanel != null)
		{
			_levelsPanel.SetActive(newState == MenuState.Levels);
			if (_levelController != null)
			{
				if (newState == MenuState.Levels)
					_levelController.Open();
				else
					_levelController.Close();
			}
		}

		if (_settingsPanel != null)
		{
			_settingsPanel.SetActive(newState == MenuState.Settings);
			if (_settingsController != null)
			{
				if (newState == MenuState.Settings)
					_settingsController.Open();
				else
					_settingsController.Close();
			}
		}

		if (_creditsPanel != null)
		{
			_creditsPanel.SetActive(newState == MenuState.Credits);
			if (_creditsController != null)
			{
				if (newState == MenuState.Credits)
					_creditsController.Open();
				else
					_creditsController.Close();
			}
		}

		if (newState == MenuState.Main)
		{
			AudioManager.Instance.PlayMusic(FMODEvents.Instance.Title_Bgm);
			UpdateCursorPosition();
		}
	}

	private void UpdateCursorPosition()
	{
		if (_mainCursor == null || _menuItems == null || _menuItems.Length == 0)
			return;

		RectTransform target = _menuItems[_currentMenuItem];
		if (target == null)
			return;

		float leftEdge = target.anchoredPosition.x - (target.rect.width * target.pivot.x);
		_mainCursor.anchoredPosition = new Vector2(leftEdge + _cursorXOffset, target.anchoredPosition.y);
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
