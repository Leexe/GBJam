using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "DistanceMortar", menuName = "Game/Modifiers/Explosive/DistanceMortar")]
	public class DistanceMortarModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		public float MinDistance { get; private set; } = 1f;

		[field: SerializeField]
		public float MaxDistance { get; private set; } = 4f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new DistanceMortarInstance(this, owner);
	}

	public class DistanceMortarInstance : TowerModifierInstance
	{
		private readonly DistanceMortarModifierSO _so;

		public DistanceMortarInstance(DistanceMortarModifierSO data, Tower owner)
			: base(data, owner)
		{
			_so = data;
		}

		public override void OnProjectileCreated(Projectile projectile)
		{
			projectile.SetDistanceDamageScaler(Owner.transform.position, _so.MinDistance, _so.MaxDistance);
		}
	}
}
