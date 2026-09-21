using Sirenix.OdinInspector;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "Tenderizer", menuName = "Game/Modifiers/Melee/Tenderizer")]
	public class TenderizerModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float InitialMultiplier { get; private set; } = 0.5f;

		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float BonusPerHit { get; private set; } = 0.25f;

		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float MaxMultiplier { get; private set; } = 2f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new TenderizerInstance(this, owner);
	}

	public class TenderizerInstance : TowerModifierInstance
	{
		private readonly TenderizerModifierSO _so;
		private Enemy _lastTarget;
		private int _consecutiveHits;

		public TenderizerInstance(TenderizerModifierSO data, Tower owner)
			: base(data, owner)
		{
			_so = data;
		}

		public override void OnBeforeDealDamage(Enemy target, ref float damage)
		{
			if (_lastTarget == target)
			{
				_consecutiveHits++;
			}
			else
			{
				_lastTarget = target;
				_consecutiveHits = 0;
			}

			float multiplier = Mathf.Min(_so.InitialMultiplier + (_consecutiveHits * _so.BonusPerHit), _so.MaxMultiplier);
			damage *= multiplier;
		}
	}
}
