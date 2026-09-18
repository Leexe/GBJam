using Sirenix.OdinInspector;
using UnityEngine;

namespace StatusEffects
{
	[CreateAssetMenu(fileName = "StatusEffectSO", menuName = "Game/StatusEffects/StatusEffectSO", order = 0)]
	public class StatusEffectSO : ScriptableObject
	{
		[field: SerializeField]
		[field: TabGroup("General", "Info")]
		public string Id { get; private set; }

		[field: SerializeField]
		[field: TabGroup("General", "Info")]
		public string Name { get; private set; }

		[field: SerializeField]
		[field: TextArea]
		[field: TabGroup("General", "Info")]
		public string Description { get; private set; }

		[field: SerializeField]
		[field: TabGroup("General", "Visuals")]
		public Sprite Icon { get; private set; }

		[field: SerializeField]
		[field: TabGroup("General", "Visuals")]
		public Color StatusColor { get; private set; }

		[field: Header("DoT Settings")]
		[field: SerializeField]
		[field: TabGroup("General", "Combat")]
		public float Duration { get; private set; } = 1f;

		[field: SerializeField]
		[field: TabGroup("General", "Combat")]
		[field: Min(0.05f)]
		public float TickInterval { get; private set; } = 0.5f;

		[field: SerializeField]
		[field: TabGroup("General", "Combat")]
		public float DamagePerTick { get; private set; }

		public virtual StatusEffect CreateInstance(Enemy target, float? duration = null, float? damagePerTick = null)
		{
			return new StatusEffect(this, target, duration ?? Duration, damagePerTick ?? DamagePerTick, TickInterval);
		}
	}
}
