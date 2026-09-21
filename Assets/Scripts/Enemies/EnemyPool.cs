using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoSingleton<EnemyPool>
{
	[SerializeField]
	private GameObject _enemyPrefab;

	[SerializeField]
	private int _initialSize = 30;

	private readonly Queue<Enemy> _pool = new();
	public static readonly List<Enemy> ActiveEnemies = new();

	protected override void OnInitialized()
	{
		base.OnInitialized();

		for (int i = 0; i < _initialSize; i++)
		{
			GameObject enemy = Instantiate(_enemyPrefab, transform);
			enemy.SetActive(false);
			_pool.Enqueue(enemy.GetComponent<Enemy>());
		}
	}

	public Enemy Get(EnemySO data, EnemyModifier modifier = default)
	{
		Enemy enemy = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(_enemyPrefab, transform).GetComponent<Enemy>();
		enemy.gameObject.SetActive(true);
		enemy.Initialize(data, modifier);
		ActiveEnemies.Add(enemy);
		return enemy;
	}

	public void Release(Enemy enemy)
	{
		ActiveEnemies.Remove(enemy);
		enemy.gameObject.SetActive(false);
		_pool.Enqueue(enemy);
	}

	private void OnDestroy()
	{
		ActiveEnemies.Clear();
	}
}
