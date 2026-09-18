using System;

namespace Stats
{
	[Serializable]
	public enum StatType
	{
		Range = 0,
		Damage = 1,
		AttackRate = 2,
		ExplosionRadius = 3,
		MaxHealth = 10,
		Speed = 11,
		IncomingDamage = 12,
	}

	public readonly struct StatChangedEventArgs
	{
		public StatType StatType { get; }
		public float Delta { get; }
		public float BaseValue { get; }
		public float FinalValue { get; }

		public StatChangedEventArgs(StatType statType, float delta, float baseValue, float finalValue)
		{
			StatType = statType;
			Delta = delta;
			BaseValue = baseValue;
			FinalValue = finalValue;
		}
	}
}
