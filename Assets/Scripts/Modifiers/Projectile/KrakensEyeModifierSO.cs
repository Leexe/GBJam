using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "KrakensEye", menuName = "Game/Modifiers/Projectile/KrakensEye")]
	public class KrakensEyeModifierSO : TowerModifierSO
	{
		public override TowerModifierInstance CreateInstance(Tower owner) => new KrakensEyeInstance(this, owner);
	}

	public class KrakensEyeInstance : TowerModifierInstance
	{
		public KrakensEyeInstance(TowerModifierSO data, Tower owner)
			: base(data, owner) { }

		public override bool CanTarget(Enemy target)
		{
			return target.CurrentHealth / target.Data.Health > 0.5f;
		}
	}
}
