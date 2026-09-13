using UnityEngine;

public class Projectile : MonoBehaviour
{
	private Transform _target;
	private float _damage;
	private float _speed;
	private int _remainingPierce;
	private float _lifetimeTimer;

	public void Initialize(Transform target, ProjectileSO projectileData)
	{
		_target = target;
		_damage = projectileData.Damage;
		_speed = projectileData.Speed;
		_remainingPierce = projectileData.PierceCount;
		_lifetimeTimer = projectileData.Lifetime;
	}

	private void Update()
	{
		_lifetimeTimer -= Time.deltaTime;
		if (_lifetimeTimer <= 0f)
		{
			Deactivate();
			return;
		}

		transform.position = Vector3.MoveTowards(transform.position, _target.position, _speed * Time.deltaTime);
		transform.rotation = Quaternion.LookRotation(transform.position - _target.position);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		_remainingPierce--;
		if (_remainingPierce <= 0)
		{
			Deactivate();
		}
	}

	private void Deactivate()
	{
		ProjectilePool.Instance.Release(this);
	}
}
