using UnityEngine;

public static class LevelProgression
{
	private const string CompletedKeyPrefix = "LevelCompleted_";

	public static bool IsLevelCompleted(LevelSO level)
	{
		return PlayerPrefs.GetInt(CompletedKeyPrefix + level.name, 0) == 1;
	}

	public static void CompleteLevel(LevelSO level)
	{
		PlayerPrefs.SetInt(CompletedKeyPrefix + level.name, 1);
		PlayerPrefs.Save();
	}

	public static void ClearLevel(LevelSO level)
	{
		PlayerPrefs.DeleteKey(CompletedKeyPrefix + level.name);
		PlayerPrefs.Save();
	}

#if UNITY_EDITOR
	[UnityEditor.MenuItem("Game/Progression/Clear All Level Progress")]
	public static void ClearAllProgressEditor()
	{
		string[] guids = UnityEditor.AssetDatabase.FindAssets("t:LevelSO");
		for (int i = 0; i < guids.Length; i++)
		{
			string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
			LevelSO level = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelSO>(path);
			PlayerPrefs.DeleteKey(CompletedKeyPrefix + level.name);
		}
		PlayerPrefs.Save();
		Debug.Log("[LevelProgression] Cleared all level progress.");
	}

	[UnityEditor.MenuItem("Game/Progression/Complete All Levels")]
	public static void CompleteAllLevelsEditor()
	{
		string[] guids = UnityEditor.AssetDatabase.FindAssets("t:LevelSO");
		for (int i = 0; i < guids.Length; i++)
		{
			string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
			LevelSO level = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelSO>(path);
			PlayerPrefs.SetInt(CompletedKeyPrefix + level.name, 1);
		}
		PlayerPrefs.Save();
		Debug.Log("[LevelProgression] Marked all levels as completed.");
	}
#endif
}
