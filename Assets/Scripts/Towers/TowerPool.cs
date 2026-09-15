using System.Collections.Generic;
using UnityEngine;

public class TowerPool : MonoSingleton<TowerPool>
{
	[SerializeField]
	private GameObject _towerPrefab;

	[SerializeField]
	private int _initialSize = 20;

	[SerializeField]
	private List<TowerSO> _availableTowers = new();

	private int _currentTower;

	private readonly Queue<Tower> _pool = new();
	public static readonly List<Tower> ActiveTowers = new();

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

	public Tower Get(Vector3 position)
	{
		Tower tower = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(_towerPrefab, transform).GetComponent<Tower>();
		tower.transform.position = position;
		tower.gameObject.SetActive(true);
		tower.Initialize(_availableTowers[_currentTower]);
		ActiveTowers.Add(tower);
		return tower;
	}

	public Tower Get(Vector3 position, TowerSO data)
	{
		Tower tower = Get(position);
		tower.Initialize(data);
		return tower;
	}

	public void Release(Tower tower)
	{
		ActiveTowers.Remove(tower);
		tower.gameObject.SetActive(false);
		_pool.Enqueue(tower);
	}

	private void OnDestroy()
	{
		ActiveTowers.Clear();
	}

	public void CycleTower(int offset)
	{
		_currentTower += offset;
		if (_currentTower >= _availableTowers.Count)
		{
			_currentTower = 0;
		}
		if (_currentTower < 0)
		{
			_currentTower = _availableTowers.Count - 1;
		}
	}
}
