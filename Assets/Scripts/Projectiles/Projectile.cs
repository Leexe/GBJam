using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField]
	private LayerMask _enemyLayerMask;

	private Transform _target;
	private float _damage;
	private float _speed;
	private int _remainingPierce;
	private float _aoeRadius;
	private float _lifetimeTimer;

	public void Initialize(Transform target, ProjectileSO projectileData)
	{
		_target = target;
		_damage = projectileData.Damage;
		_speed = projectileData.Speed;
		_remainingPierce = projectileData.PierceCount;
		_aoeRadius = projectileData.IsAoe ? projectileData.AoeRadius : 0f;
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
		if ((_enemyLayerMask.value & (1 << other.gameObject.layer)) == 0)
		{
			return;
		}

		if (_aoeRadius > 0f)
		{
			Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _aoeRadius, _enemyLayerMask);
			foreach (Collider2D hit in hits)
			{
				hit.GetComponent<Enemy>()?.TakeDamage(_damage);
			}
		}
		else if (other.TryGetComponent<Enemy>(out var enemy))
		{
			enemy.TakeDamage(_damage);
		}

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
