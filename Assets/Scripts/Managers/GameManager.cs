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

	private int _health;
	private int _gold;

	public int Health => _health;
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

	private void Start()
	{
		_health = _maxHealth;
		_waveController.Initialize(_levelSO);
		_waveController.StartNextWave();
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
		OnGoldGain?.Invoke();
		_gold += amount;
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
