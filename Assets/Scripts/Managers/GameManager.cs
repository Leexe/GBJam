using System;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
	[Header("References")]
	[SerializeField]
	private LevelSO _levelSO;

	[Header("Settings")]
	[SerializeField]
	private int _maxHealth = 100;

	private int _health;
	private int _gold;

	public int Health => _health;

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

	private void LoseGame()
	{
		OnLose?.Invoke();
	}
}
