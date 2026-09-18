using System;
using System.Collections.Generic;
using Modifiers;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class SpawnGroup
{
	[HorizontalGroup("Row", 60)]
	[PreviewField(50, ObjectFieldAlignment.Left)]
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
	[LabelWidth(80)]
	public float StartDelay = 0f;
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

	[ListDrawerSettings(ShowIndexLabels = true)]
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
	[TabGroup("Tabs", "Waves")]
	public string LevelName;

	[TabGroup("Tabs", "Waves")]
	[ListDrawerSettings(ShowIndexLabels = true)]
	public List<WaveData> WaveDataList = new();

	[TabGroup("Tabs", "Items")]
	[MinValue(1)]
	public int DefaultItemChoicesCount = 3;

	[TabGroup("Tabs", "Items")]
	[ListDrawerSettings(ShowIndexLabels = true)]
	public List<TowerModifierSO> ItemPool = new();

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
