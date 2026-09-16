using System;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
	[Header("References")]
	[SerializeField]
	private LevelSO _levelSO;

	[SerializeField]
	private WaveController _waveController;

	[Header("Settings")]
	[SerializeField]
	private int _maxHealth = 100;

	[SerializeField]
	private int _startingGold = 100;

	private int _health;
	private float _time;
	private int _gold;

	public int Health => _health;
	public float Time => _time;
	public int Gold => _gold;
	public WaveController WaveController => _waveController;

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

	private void Start()
	{
		_health = _maxHealth;
		_gold = _startingGold;
		_waveController.Initialize(_levelSO);
		_waveController.StartNextWave();
	}

	private void Update()
	{
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
		_waveController.StopWaves();
		OnLose?.Invoke();
	}
}
