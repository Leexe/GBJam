using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoSingleton<ProjectilePool>
{
	[SerializeField]
	private GameObject _projectilePrefab;

	[SerializeField]
	private int _initialSize = 100;

	private readonly Queue<Projectile> _pool = new();

	protected override void OnInitialized()
	{
		base.OnInitialized();

		for (int i = 0; i < _initialSize; i++)
		{
			GameObject projectile = Instantiate(_projectilePrefab, transform);
			projectile.gameObject.SetActive(false);
			_pool.Enqueue(projectile.GetComponent<Projectile>());
		}
	}

	public Projectile Get(Vector3 position)
	{
		Projectile projectile =
			_pool.Count > 0 ? _pool.Dequeue() : Instantiate(_projectilePrefab, transform).GetComponent<Projectile>();
		projectile.transform.position = position;
		projectile.gameObject.SetActive(true);
		return projectile;
	}

	public Projectile Get(Vector3 position, Transform target, ProjectileSO data)
	{
		Projectile projectile = Get(position);
		projectile.Initialize(target, data);
		return projectile;
	}

	public void Release(Projectile projectile)
	{
		projectile.gameObject.SetActive(false);
		_pool.Enqueue(projectile);
	}
}
