using Sirenix.OdinInspector;
using StatusEffects;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "InfernalHarpoon", menuName = "Game/Modifiers/Melee/InfernalHarpoon")]
	public class InfernalHarpoonModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public int AttacksToProc { get; private set; } = 5;

		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float OverheatDuration { get; private set; } = 2f;

		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public StatusEffectSO BurnEffectSO { get; private set; }

		public override TowerModifierInstance CreateInstance(Tower owner) => new InfernalHarpoonInstance(this, owner);
	}

	public class InfernalHarpoonInstance : TowerModifierInstance
	{
		private readonly InfernalHarpoonModifierSO _so;
		private int _attackCounter;

		public InfernalHarpoonInstance(InfernalHarpoonModifierSO data, Tower owner)
			: base(data, owner)
		{
			_so = data;
		}

		public override void OnAttack(Enemy target)
		{
			_attackCounter++;
			if (_attackCounter >= _so.AttacksToProc)
			{
				_attackCounter = 0;
				Owner.ApplyStatusEffectToAllEnemiesInRange(_so.BurnEffectSO);
				Owner.Stun(_so.OverheatDuration);
			}
		}
	}
}
