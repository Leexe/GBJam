using System;

namespace Stats
{
	[Serializable]
	public class Modifier
	{
		public StatType StatName;
		public float FlatAdd;
		public float PercentIncreaseAdd;
		public float ModifierMult = 1f;
		public float FinalMult = 1f;

		public Modifier() { }

		public Modifier(
			StatType statName,
			float flatAdd = 0f,
			float percentIncreaseAdd = 0f,
			float modifierMult = 1f,
			float finalMult = 1f
		)
		{
			StatName = statName;
			FlatAdd = flatAdd;
			PercentIncreaseAdd = percentIncreaseAdd;
			ModifierMult = modifierMult;
			FinalMult = finalMult;
		}
	}
}
