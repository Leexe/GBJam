using TMPro;
using UnityEngine;

public class WinLoseMenuController : MonoBehaviour
{
	[Header("Menu Panels")]
	[SerializeField]
	private GameObject _menuRoot;

	[Header("Title")]
	[SerializeField]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	private string _winTitle = "YOU WIN";

	[SerializeField]
	private string _loseTitle = "YOU LOSE";

	[Header("Buttons")]
	[SerializeField]
	private RectTransform _nextLevelButton;

	[SerializeField]
	private RectTransform _retryButton;

	[SerializeField]
	private RectTransform _mainMenuButton;

	[Header("Cursor Indicator")]
	[SerializeField]
	private RectTransform _cursor;

	[SerializeField]
	private CursorAnimator _cursorAnimator;

	[SerializeField]
	private float _cursorXOffset = -8f;

	private RectTransform[] _activeButtons;
	private int _currentIndex;
	private bool _isOpen;
	private Vector2Int _lastDirection;
	private float _holdTimer;

	private void Awake()
	{
		_menuRoot.SetActive(false);
	}

	private void Start()
	{
		GameManager.Instance.OnWin += HandleWin;
		GameManager.Instance.OnLose += HandleLose;
	}

	private void OnDestroy()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnWin -= HandleWin;
			GameManager.Instance.OnLose -= HandleLose;
		}

		UnsubscribeInput();
	}

	private void HandleWin()
	{
		Open(true);
	}

	private void HandleLose()
	{
		Open(false);
	}

	public void Open(bool isWin)
	{
		_isOpen = true;
		Time.timeScale = 0f;
		_menuRoot.SetActive(true);

		_titleText.text = isWin ? _winTitle : _loseTitle;

		if (isWin)
		{
			_nextLevelButton.gameObject.SetActive(true);
			_activeButtons = new[] { _nextLevelButton, _retryButton, _mainMenuButton };
		}
		else
		{
			_nextLevelButton.gameObject.SetActive(false);
			_activeButtons = new[] { _retryButton, _mainMenuButton };
		}

		_currentIndex = 0;
		UpdateCursorPosition();
		SubscribeInput();
	}

	public void Close()
	{
		_isOpen = false;
		UnsubscribeInput();
		_menuRoot.SetActive(false);
	}

	private void SubscribeInput()
	{
		InputManager.Instance.OnMovement += HandleMovement;
		InputManager.Instance.OnConfirm += HandleConfirm;
		InputManager.Instance.OnStart += HandleConfirm;
	}

	private void UnsubscribeInput()
	{
		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnMovement -= HandleMovement;
			InputManager.Instance.OnConfirm -= HandleConfirm;
			InputManager.Instance.OnStart -= HandleConfirm;
		}
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
			int step = dir.y > 0 ? -1 : 1;
			_currentIndex = (_currentIndex + step + _activeButtons.Length) % _activeButtons.Length;
			UpdateCursorPosition();
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SelectorClick_Sfx);
		}
	}

	private void HandleConfirm()
	{
		if (!_isOpen)
		{
			return;
		}

		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CompleteClick_Sfx);
		RectTransform selected = _activeButtons[_currentIndex];

		if (selected == _nextLevelButton)
		{
			Close();
			GameManager.Instance.LoadNextLevel();
		}
		else if (selected == _retryButton)
		{
			Close();
			GameManager.Instance.RestartLevel();
		}
		else if (selected == _mainMenuButton)
		{
			Close();
			GameManager.Instance.ReturnToMainMenu();
		}
	}

	private void UpdateCursorPosition()
	{
		RectTransform target = _activeButtons[_currentIndex];
		float leftEdge = target.anchoredPosition.x - (target.rect.width * target.pivot.x);
		_cursor.anchoredPosition = new Vector2(leftEdge + _cursorXOffset, target.anchoredPosition.y);
		_cursorAnimator.Play();
	}
}
