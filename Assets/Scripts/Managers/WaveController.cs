using System;
using System.Collections;
using System.Collections.Generic;
using Modifiers;
using UnityEngine;

public class WaveController : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private float _timeBetweenWaves = 3f;

	private LevelSO _levelSO;
	private int _currentWaveIndex;
	private int _activeEnemies;
	private int _activeSpawners;
	private bool _isStopped;
	private bool _isSpawningPaused;

	public int CurrentWaveIndex => _currentWaveIndex;
	public int TotalWaves => _levelSO.WaveDataList.Count;
	public bool IsWaveInProgress => _activeEnemies > 0 || _activeSpawners > 0;
	public bool IsSpawningPaused => _isSpawningPaused;

	[HideInInspector]
	public Action<int> OnWaveStarted;

	[HideInInspector]
	public Action<int, List<TowerModifierSO>> OnWaveItemsOffered;

	[HideInInspector]
	public Action<int, List<TowerModifierSO>> OnWaveModifiersOffered;

	[HideInInspector]
	public Action<int> OnWaveCompleted;

	[HideInInspector]
	public Action OnAllWavesCompleted;

	public void Initialize(LevelSO levelSO)
	{
		_levelSO = levelSO;
		_currentWaveIndex = 0;
		_isStopped = false;
		_isSpawningPaused = false;
	}

	public void PauseSpawning() => _isSpawningPaused = true;

	public void ResumeSpawning() => _isSpawningPaused = false;

	public void StartNextWave()
	{
		if (_currentWaveIndex >= _levelSO.WaveDataList.Count || IsWaveInProgress)
		{
			return;
		}

		WaveData waveData = _levelSO.WaveDataList[_currentWaveIndex];
		if (waveData.TotalEnemies == 0)
		{
			return;
		}

		_activeEnemies = waveData.TotalEnemies;
		_activeSpawners = waveData.SpawnGroups.Count;

		if (_levelSO.DoesWaveGiveItems(_currentWaveIndex))
		{
			PauseSpawning();
			List<TowerModifierSO> choices = _levelSO.GetItemChoicesForWave(_currentWaveIndex);
			OnWaveItemsOffered?.Invoke(_currentWaveIndex, choices);
			OnWaveModifiersOffered?.Invoke(_currentWaveIndex, choices);
		}

		foreach (SpawnGroup t in waveData.SpawnGroups)
		{
			StartCoroutine(SpawnGroupRoutine(t));
		}

		OnWaveStarted?.Invoke(_currentWaveIndex);
	}

	public void StopWaves()
	{
		StopAllCoroutines();
		_isStopped = true;
		_activeSpawners = 0;
		_activeEnemies = 0;
	}

	private IEnumerator SpawnGroupRoutine(SpawnGroup group)
	{
		if (group.StartDelay > 0f)
		{
			yield return new WaitForSeconds(group.StartDelay);
		}

		for (int i = 0; i < group.Count; i++)
		{
			while (_isSpawningPaused)
			{
				yield return null;
			}

			SpawnEnemy(group.Enemy);
			if (i < group.Count - 1)
			{
				yield return new WaitForSeconds(group.Interval);
			}
		}

		_activeSpawners--;
	}

	private void SpawnEnemy(EnemySO enemySO)
	{
		Enemy enemy = EnemyPool.Instance.Get(enemySO);
		enemy.OnDeath = HandleEnemyDeactivated;
	}

	private void HandleEnemyDeactivated(Enemy enemy)
	{
		if (_isStopped)
		{
			return;
		}

		_activeEnemies--;
		if (_activeEnemies > 0)
		{
			return;
		}

		OnWaveCompleted?.Invoke(_currentWaveIndex);
		_currentWaveIndex++;

		if (_currentWaveIndex >= _levelSO.WaveDataList.Count)
		{
			OnAllWavesCompleted?.Invoke();
			GameManager.Instance.WinGame();
		}
		else
		{
			StartCoroutine(AutoStartNextWaveRoutine());
		}
	}

	private IEnumerator AutoStartNextWaveRoutine()
	{
		yield return new WaitForSeconds(_timeBetweenWaves);
		StartNextWave();
	}
}
