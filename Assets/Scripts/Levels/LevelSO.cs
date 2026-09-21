using System;
using System.Collections.Generic;
using Modifiers;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class SpawnGroup
{
	[HorizontalGroup("Row", 50)]
	[ShowInInspector, HideLabel, PreviewField(50, ObjectFieldAlignment.Left), ReadOnly]
	public Sprite Preview => Enemy != null ? Enemy.Icon : null;

	[VerticalGroup("Row/Details")]
	[HideLabel]
	public EnemySO Enemy;

	[VerticalGroup("Row/Details")]
	[HorizontalGroup("Row/Details/H1")]
	[LabelWidth(50)]
	public int Count = 5;

	[HorizontalGroup("Row/Details/H1")]
	[LabelWidth(60)]
	public float Interval = 0.5f;

	[VerticalGroup("Row/Details")]
	[HorizontalGroup("Row/Details/H2")]
	[LabelWidth(50)]
	[LabelText("Delay")]
	public float StartDelay = 0f;

	[HorizontalGroup("Row/Details/H2")]
	[LabelWidth(50)]
	[LabelText("Speed x")]
	[MinValue(0.1f)]
	public float SpeedMultiplier = 1f;

	[VerticalGroup("Row/Details")]
	[FoldoutGroup("Row/Details/Extra Modifiers", false)]
	[LabelWidth(70)]
	[LabelText("Health x")]
	[MinValue(0.1f)]
	public float HealthMultiplier = 1f;

	[FoldoutGroup("Row/Details/Extra Modifiers")]
	[LabelWidth(70)]
	[LabelText("Damage x")]
	[MinValue(0.1f)]
	public float DamageMultiplier = 1f;

	[FoldoutGroup("Row/Details/Extra Modifiers")]
	[LabelWidth(70)]
	[LabelText("Gold x")]
	[MinValue(0f)]
	public float GoldMultiplier = 1f;

	[FoldoutGroup("Row/Details/Extra Modifiers")]
	[LabelWidth(70)]
	[LabelText("Scale x")]
	[MinValue(0.1f)]
	public float ScaleMultiplier = 1f;

	public EnemyModifier Modifier =>
		new()
		{
			SpeedMultiplier = SpeedMultiplier > 0f ? SpeedMultiplier : 1f,
			HealthMultiplier = HealthMultiplier > 0f ? HealthMultiplier : 1f,
			DamageMultiplier = DamageMultiplier > 0f ? DamageMultiplier : 1f,
			GoldMultiplier = GoldMultiplier > 0f ? GoldMultiplier : 1f,
			ScaleMultiplier = ScaleMultiplier > 0f ? ScaleMultiplier : 1f,
		};

	public string ElementLabel
	{
		get
		{
			if (Enemy == null)
			{
				return "Empty";
			}

			string label = $"{Enemy.Name} (x{Count})";
			if (SpeedMultiplier > 0f && !Mathf.Approximately(SpeedMultiplier, 1f))
			{
				label += $" [{SpeedMultiplier:0.#}x Spd]";
			}
			if (HealthMultiplier > 0f && !Mathf.Approximately(HealthMultiplier, 1f))
			{
				label += $" [{HealthMultiplier:0.#}x HP]";
			}

			return label;
		}
	}
}

[Serializable]
public class WaveData
{
	[BoxGroup("Item Reward", centerLabel: false)]
	[LabelText("Offer Items At Wave Start")]
	public bool GivesItems;

	[BoxGroup("Item Reward")]
	[ShowIf(nameof(GivesItems))]
	[MinValue(1)]
	public int ItemChoicesCount = 3;

	[BoxGroup("Item Reward")]
	[ShowIf(nameof(GivesItems))]
	public bool OverrideLevelItemPool;

	[BoxGroup("Item Reward")]
	[ShowIf(nameof(CanShowCustomItemPool))]
	public List<TowerModifierSO> CustomItemPool = new();

	private bool CanShowCustomItemPool => GivesItems && OverrideLevelItemPool;

	[ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = nameof(SpawnGroup.ElementLabel))]
	public List<SpawnGroup> SpawnGroups = new();

	[ShowInInspector, ReadOnly]
	public int TotalEnemies
	{
		get
		{
			int sum = 0;
			for (int i = 0; i < SpawnGroups.Count; i++)
			{
				sum += SpawnGroups[i].Count;
			}
			return sum;
		}
	}
}

[CreateAssetMenu(fileName = "LevelSO", menuName = "Game/LevelSO", order = 0)]
public class LevelSO : ScriptableObject
{
	[TabGroup("Tabs", "General")]
	public string LevelName;

	[TabGroup("Tabs", "General")]
	[MinValue(1)]
	public int MaxHealth = 100;

	[TabGroup("Tabs", "General")]
	[MinValue(0)]
	public int StartingGold = 100;

	public int Gold => StartingGold;

	[TabGroup("Tabs", "Waves")]
	[ListDrawerSettings(ShowIndexLabels = true)]
	public List<WaveData> WaveDataList = new();

	[TabGroup("Tabs", "Items")]
	[MinValue(1)]
	public int DefaultItemChoicesCount = 3;

	[TabGroup("Tabs", "Items")]
	[ListDrawerSettings(ShowIndexLabels = true)]
	public List<TowerModifierSO> ItemPool = new();

	[TabGroup("Tabs", "Towers")]
	[ListDrawerSettings(ShowIndexLabels = true)]
	public List<TowerSO> TowerPool = new();

	public List<TowerSO> Towers => TowerPool;
	public List<TowerSO> CharacterPool => TowerPool;

	public bool DoesWaveGiveItems(int waveIndex)
	{
		if (waveIndex < 0 || waveIndex >= WaveDataList.Count)
		{
			return false;
		}

		return WaveDataList[waveIndex].GivesItems;
	}

	public List<TowerModifierSO> GetItemChoicesForWave(int waveIndex, int count = -1)
	{
		if (waveIndex < 0 || waveIndex >= WaveDataList.Count)
		{
			return new List<TowerModifierSO>();
		}

		WaveData wave = WaveDataList[waveIndex];
		List<TowerModifierSO> sourcePool =
			wave.OverrideLevelItemPool && wave.CustomItemPool != null && wave.CustomItemPool.Count > 0
				? wave.CustomItemPool
				: ItemPool;

		if (sourcePool == null || sourcePool.Count == 0)
		{
			return new List<TowerModifierSO>();
		}

		int choiceCount =
			count > 0 ? count : (wave.ItemChoicesCount > 0 ? wave.ItemChoicesCount : DefaultItemChoicesCount);
		choiceCount = Mathf.Min(choiceCount, sourcePool.Count);

		List<TowerModifierSO> copy = new List<TowerModifierSO>(sourcePool);
		List<TowerModifierSO> selected = new List<TowerModifierSO>(choiceCount);

		for (int i = 0; i < choiceCount; i++)
		{
			int randomIndex = UnityEngine.Random.Range(0, copy.Count);
			selected.Add(copy[randomIndex]);
			copy.RemoveAt(randomIndex);
		}

		return selected;
	}

	public bool DoesWaveGiveModifiers(int waveIndex) => DoesWaveGiveItems(waveIndex);

	public List<TowerModifierSO> GetModifierChoicesForWave(int waveIndex, int count = -1) =>
		GetItemChoicesForWave(waveIndex, count);
}
