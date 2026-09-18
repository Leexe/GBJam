using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "WildFlail", menuName = "Game/Modifiers/Melee/WildFlail")]
	public class WildFlailModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		public float DamageVariationRange { get; private set; } = 5f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new WildFlailInstance(this, owner);
	}

	public class WildFlailInstance : TowerModifierInstance
	{
		private readonly WildFlailModifierSO _so;

		public WildFlailInstance(WildFlailModifierSO data, Tower owner)
			: base(data, owner)
		{
			_so = data;
		}

		public override void OnBeforeDealDamage(Enemy target, ref float damage)
		{
			damage += Random.Range(-_so.DamageVariationRange, _so.DamageVariationRange);
		}
	}
}
