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

	private readonly ModifierManager _modifierManager = new();

	private int _health;
	private float _time;
	private int _gold;
	private bool _hasLost;

	public int Health => _health;
	public int MaxHealth => _levelSO.MaxHealth;
	public float Time => _time;
	public int Gold => _gold;
	public bool HasLost => _hasLost;
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

	private void Start()
	{
		_health = _levelSO.MaxHealth;
		_gold = _levelSO.StartingGold;
		_modifierManager.Initialize();
		_waveController.OnWaveItemsOffered += HandleWaveItemsOffered;
		_waveController.Initialize(_levelSO);
		_waveController.StartNextWave();

		DisablePrimeTween();
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
		if (_hasLost)
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
		OnWin?.Invoke();
	}

	private void LoseGame()
	{
		_hasLost = true;
		_waveController.StopWaves();
		OnLose?.Invoke();
	}
}
