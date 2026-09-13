using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WaveData
{
	public List<EnemySO> EnemiesList;
}

[CreateAssetMenu(fileName = "LevelSO", menuName = "Game/LevelSO", order = 0)]
public class LevelSO : ScriptableObject
{
	public string LevelName;
	public List<WaveData> WaveDataList;
}
