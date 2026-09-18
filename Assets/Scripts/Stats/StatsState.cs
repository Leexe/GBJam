using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stats
{
	public class StatsState
	{
		private readonly Dictionary<StatType, float> _baseStatsMap;
		private readonly Dictionary<StatType, float> _finalStats = new();
		private readonly List<Modifier> _modifiersList = new();

		public event Action<StatChangedEventArgs> OnStatChanged;

		public float GetFinalStat(StatType statType) => _finalStats.GetValueOrDefault(statType, 0f);

		public StatsState(IReadOnlyDictionary<StatType, float> baseStatsMap)
		{
			_baseStatsMap = new Dictionary<StatType, float>(baseStatsMap);
			UpdateAllStats();
		}

		public Modifier AddFlat(StatType statType, float flatAdd)
		{
			return AddModifier(statType, flatAdd: flatAdd);
		}

		public Modifier PercentIncrease(StatType statType, float percentIncreaseAdd)
		{
			return AddModifier(statType, percentIncreaseAdd: percentIncreaseAdd);
		}

		public Modifier BaseMult(StatType statType, float modifierMult)
		{
			return AddModifier(statType, modifierMult: modifierMult);
		}

		public Modifier FinalMult(StatType statType, float finalMult)
		{
			return AddModifier(statType, finalMult: finalMult);
		}

		public Modifier AddModifier(
			StatType statType,
			float flatAdd = 0f,
			float percentIncreaseAdd = 0f,
			float modifierMult = 1f,
			float finalMult = 1f
		)
		{
			var newModifier = new Modifier
			{
				StatName = statType,
				FlatAdd = flatAdd,
				PercentIncreaseAdd = percentIncreaseAdd,
				ModifierMult = modifierMult,
				FinalMult = finalMult,
			};
			_modifiersList.Add(newModifier);
			UpdateStat(statType);
			return newModifier;
		}

		public Modifier AddModifier(Modifier modifier)
		{
			return AddModifier(
				modifier.StatName,
				modifier.FlatAdd,
				modifier.PercentIncreaseAdd,
				modifier.ModifierMult,
				modifier.FinalMult
			);
		}

		public void RemoveModifier(Modifier modifier)
		{
			StatType affectedStat = modifier.StatName;
			_modifiersList.Remove(modifier);
			UpdateStat(affectedStat);
		}

		public void ChangeModifier(
			Modifier modifier,
			float? flatAdd = null,
			float? percentIncreaseAdd = null,
			float? modifierMult = null,
			float? finalMult = null
		)
		{
			if (flatAdd.HasValue)
			{
				modifier.FlatAdd = flatAdd.Value;
			}
			if (percentIncreaseAdd.HasValue)
			{
				modifier.PercentIncreaseAdd = percentIncreaseAdd.Value;
			}
			if (modifierMult.HasValue)
			{
				modifier.ModifierMult = modifierMult.Value;
			}
			if (finalMult.HasValue)
			{
				modifier.FinalMult = finalMult.Value;
			}

			UpdateStat(modifier.StatName);
		}

		public void SetBaseStat(StatType statType, float baseValue)
		{
			_baseStatsMap[statType] = baseValue;
			UpdateStat(statType);
		}

		private void UpdateAllStats()
		{
			foreach (StatType statType in (StatType[])Enum.GetValues(typeof(StatType)))
			{
				UpdateStat(statType);
			}
		}

		private void UpdateStat(StatType statType)
		{
			float flatAdd = 0;
			float percentIncrease = 0;
			float baseModifierMult = 1;
			float finalMult = 1;

			for (int i = 0; i < _modifiersList.Count; i++)
			{
				Modifier modifier = _modifiersList[i];
				if (modifier.StatName == statType)
				{
					flatAdd += modifier.FlatAdd;
					percentIncrease += modifier.PercentIncreaseAdd * 0.01f;
					baseModifierMult *= modifier.ModifierMult;
					finalMult *= modifier.FinalMult;
				}
			}

			float baseValue = _baseStatsMap.TryGetValue(statType, out float val) ? val : 0f;
			float oldValue = _finalStats.GetValueOrDefault(statType, baseValue);
			float newValue = ((baseValue * baseModifierMult) + flatAdd) * (1 + percentIncrease) * finalMult;
			_finalStats[statType] = newValue;

			if (!Mathf.Approximately(oldValue, newValue))
			{
				OnStatChanged?.Invoke(new StatChangedEventArgs(statType, newValue - oldValue, baseValue, newValue));
			}
		}
	}
}
