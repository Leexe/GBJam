using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "BloodPearl", menuName = "Game/Modifiers/Projectile/BloodPearl")]
	public class BloodPearlModifierSO : TowerModifierSO
	{
		public override TowerModifierInstance CreateInstance(Tower owner) => new BloodPearlInstance(this, owner);
	}

	public class BloodPearlInstance : TowerModifierInstance
	{
		public BloodPearlInstance(TowerModifierSO data, Tower owner)
			: base(data, owner) { }

		public override bool CanTarget(Enemy target)
		{
			return target.CurrentHealth < target.MaxHealth;
		}

		public override void OnBeforeDealDamage(Enemy target, ref float damage)
		{
			if (target.CurrentHealth <= target.MaxHealth * 0.5f)
			{
				damage *= 2f;
			}
		}
	}
}
