using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "GamblersBullet", menuName = "Game/Modifiers/Projectile/GamblersBullet")]
	public class GamblersBulletModifierSO : TowerModifierSO
	{
		public override TowerModifierInstance CreateInstance(Tower owner) => new GamblersBulletInstance(this, owner);
	}

	public class GamblersBulletInstance : TowerModifierInstance
	{
		public GamblersBulletInstance(TowerModifierSO data, Tower owner)
			: base(data, owner) { }

		public override void OnBeforeDealDamage(Enemy target, ref float damage)
		{
			int roll = Random.Range(1, 7);
			if (roll == 6)
			{
				damage *= 2f;
			}
			else if (roll == 1)
			{
				target.BuffSpeed(1.25f, 3f);
			}
		}
	}
}
