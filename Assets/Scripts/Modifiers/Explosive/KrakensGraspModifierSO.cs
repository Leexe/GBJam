using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "KrakensGrasp", menuName = "Game/Modifiers/Explosive/KrakensGrasp")]
	public class KrakensGraspModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		public float PullStrength { get; private set; } = 0.6f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new KrakensGraspInstance(this, owner);
	}

	public class KrakensGraspInstance : TowerModifierInstance
	{
		private readonly KrakensGraspModifierSO _so;

		public KrakensGraspInstance(KrakensGraspModifierSO data, Tower owner)
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
					enemy.DisplaceToward(explosionCenter, _so.PullStrength);
				}
			}
		}
	}
}
