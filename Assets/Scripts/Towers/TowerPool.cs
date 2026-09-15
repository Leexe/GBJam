using System.Collections.Generic;
using UnityEngine;

public class TowerPool : MonoSingleton<TowerPool>
{
	[SerializeField]
	private GameObject _towerPrefab;

	[SerializeField]
	private int _initialSize = 20;

	private readonly Queue<Tower> _pool = new();

	protected override void OnInitialized()
	{
		base.OnInitialized();

		for (int i = 0; i < _initialSize; i++)
		{
			GameObject tower = Instantiate(_towerPrefab, transform);
			tower.SetActive(false);
			_pool.Enqueue(tower.GetComponent<Tower>());
		}
	}

	public Tower Get(Vector3 position, TowerSO data)
	{
		Tower tower = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(_towerPrefab, transform).GetComponent<Tower>();
		tower.transform.position = position;
		tower.gameObject.SetActive(true);
		tower.Initialize(data);
		return tower;
	}

	public void Release(Tower tower)
	{
		tower.gameObject.SetActive(false);
		_pool.Enqueue(tower);
	}
}
