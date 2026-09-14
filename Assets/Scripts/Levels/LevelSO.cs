using System;
using System.Collections.Generic;
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
	public string LevelName;

	[ListDrawerSettings(ShowIndexLabels = true)]
	public List<WaveData> WaveDataList = new();
}
