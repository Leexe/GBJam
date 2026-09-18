using Sirenix.OdinInspector;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "RingOfTheWildHunt", menuName = "Game/Modifiers/Melee/RingOfTheWildHunt")]
	public class RingOfTheWildHuntModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float DamageVariationRange { get; private set; } = 5f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new RingOfTheWildHuntInstance(this, owner);
	}

	public class RingOfTheWildHuntInstance : TowerModifierInstance
	{
		private readonly RingOfTheWildHuntModifierSO _so;

		public RingOfTheWildHuntInstance(RingOfTheWildHuntModifierSO data, Tower owner)
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
