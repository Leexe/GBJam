using System;
using System.Collections.Generic;
using Modifiers;
using PrimeTween;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
	[Header("References")]
	[SerializeField]
	private LevelSO _levelSO;

	[SerializeField]
	private WaveController _waveController;

	[Header("Music Settings")]
	[SerializeField, Range(0f, 1f)]
	private float _preWaveMusicVolume = 0.7f;

	[SerializeField]
	private float _musicLerpDuration = 1f;

	public float PreWaveMusicVolume
	{
		get => _preWaveMusicVolume;
		set => _preWaveMusicVolume = Mathf.Clamp01(value);
	}

	public float MusicLerpDuration
	{
		get => _musicLerpDuration;
		set => _musicLerpDuration = Mathf.Max(0f, value);
	}

	private readonly ModifierManager _modifierManager = new();

	private int _health;
	private float _time;
	private int _gold;
	private bool _hasLost;
	private bool _hasWon;

	public int Health => _health;
	public int MaxHealth => _levelSO.MaxHealth;
	public float Time => _time;
	public int Gold => _gold;
	public bool HasLost => _hasLost;
	public bool HasWon => _hasWon;
	public WaveController WaveController => _waveController;
	public ModifierManager ModifierManager => _modifierManager;
	public LevelSO Level => _levelSO;

	// Events
	[HideInInspector]
	public Action OnLose;

	[HideInInspector]
	public Action OnWin;

	[HideInInspector]
	public Action OnDamage;

	[HideInInspector]
	public Action OnGoldGain;

	[HideInInspector]
	public Action OnGoldSpend;

	[HideInInspector]
	public Action<int, List<TowerModifierSO>> OnWaveItemsOffered;

	public static LevelSO SelectedLevel { get; set; }

	protected override void OnInitialized()
	{
		base.OnInitialized();
		if (SelectedLevel != null)
		{
			_levelSO = SelectedLevel;
		}
	}

	private void Start()
	{
		if (SelectedLevel != null)
		{
			_levelSO = SelectedLevel;
		}
		_health = _levelSO.MaxHealth;
		_gold = _levelSO.StartingGold;
		_time = 0f;
		_hasLost = false;
		_hasWon = false;
		_modifierManager.Initialize();
		_waveController.OnWaveItemsOffered += HandleWaveItemsOffered;
		_waveController.OnWaveStarted += HandleWaveStarted;
		_waveController.OnWaveCompleted += HandleWaveCompleted;
		_waveController.Initialize(_levelSO);

		PlayLevelMusic();
		AudioManager.Instance.SetMusicMultiplier(_preWaveMusicVolume);

		_waveController.StartNextWave();

		DisablePrimeTween();
	}

	private void HandleWaveStarted(int waveIndex)
	{
		AudioManager.Instance.LerpMusicMultiplier(1f, _musicLerpDuration);
	}

	private void HandleWaveCompleted(int waveIndex)
	{
		AudioManager.Instance.LerpMusicMultiplier(_preWaveMusicVolume, _musicLerpDuration);
	}

	private void PlayLevelMusic()
	{
		if (!_levelSO.Music.IsNull)
		{
			AudioManager.Instance.PlayMusic(_levelSO.Music);
		}
	}

	private void OnDisable()
	{
		if (_waveController != null)
		{
			_waveController.OnWaveItemsOffered -= HandleWaveItemsOffered;
			_waveController.OnWaveStarted -= HandleWaveStarted;
			_waveController.OnWaveCompleted -= HandleWaveCompleted;
		}

		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.SetMusicMultiplier(1f);
			AudioManager.Instance.StopMusic();
		}
	}

	private void OnDestroy()
	{
		_modifierManager.Cleanup();
	}

	private void HandleWaveItemsOffered(int waveIndex, List<TowerModifierSO> choices)
	{
		OnWaveItemsOffered?.Invoke(waveIndex, choices);
	}

	private void Update()
	{
		HandleTimer();
	}

	private void DisablePrimeTween()
	{
		PrimeTweenConfig.warnTweenOnDisabledTarget = false;
	}

	private void HandleTimer()
	{
		if (_hasLost || _hasWon)
		{
			return;
		}

		_time += UnityEngine.Time.deltaTime;
	}

	public void DamageHealth(int amount)
	{
		_health -= amount;
		OnDamage?.Invoke();
		if (_health <= 0)
		{
			LoseGame();
		}
	}

	public void GiveGold(int amount)
	{
		_gold += amount;
		OnGoldGain?.Invoke();
	}

	public bool CanAfford(int amount) => _gold >= amount;

	public bool SpendGold(int amount)
	{
		if (!CanAfford(amount))
		{
			return false;
		}

		_gold -= amount;
		OnGoldSpend?.Invoke();
		return true;
	}

	public void WinGame()
	{
		if (_hasWon || _hasLost)
		{
			return;
		}

		_hasWon = true;
		AudioManager.Instance.PauseMusic();
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.WinningJingle_Sfx);
		OnWin?.Invoke();
	}

	private void LoseGame()
	{
		if (_hasLost || _hasWon)
		{
			return;
		}

		_hasLost = true;
		_waveController.StopWaves();
		AudioManager.Instance.PlayMusic(FMODEvents.Instance.LosingJingle_Sfx);
		OnLose?.Invoke();
	}
}
