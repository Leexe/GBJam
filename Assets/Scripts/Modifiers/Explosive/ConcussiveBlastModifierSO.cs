using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "ConcussiveBlast", menuName = "Game/Modifiers/Explosive/ConcussiveBlast")]
	public class ConcussiveBlastModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		public float PushDistance { get; private set; } = 0.5f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new ConcussiveBlastInstance(this, owner);
	}

	public class ConcussiveBlastInstance : TowerModifierInstance
	{
		private readonly ConcussiveBlastModifierSO _so;

		public ConcussiveBlastInstance(ConcussiveBlastModifierSO data, Tower owner)
			: base(data, owner)
		{
			_so = data;
		}

		public override void OnExplosionHit(Collider2D[] hits, Vector3 explosionCenter)
		{
			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].TryGetComponent<Enemy>(out Enemy enemy))
				{
					enemy.DisplaceAwayFrom(explosionCenter, _so.PushDistance);
				}
			}
		}
	}
}
