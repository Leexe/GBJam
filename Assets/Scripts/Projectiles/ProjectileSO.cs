using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSO", menuName = "Game/ProjectileSO", order = 0)]
public class ProjectileSO : ScriptableObject
{
	[Header("Context")]
	[field: SerializeField]
	[field: TabGroup("Tab/Context")]
	public string Name { get; private set; }

	[field: SerializeField]
	[field: TabGroup("Tab/Context")]
	public string Id { get; private set; }

	[Header("Data")]
	[field: SerializeField]
	[field: TabGroup("Tab/Data")]
	public float Speed { get; private set; } = 8f;

	[field: SerializeField]
	[field: TabGroup("Tab/Data")]
	public float Damage { get; private set; } = 10f;

	[field: SerializeField]
	[field: TabGroup("Tab/Data")]
	public float Lifetime { get; private set; } = 4f;

	[field: SerializeField]
	[field: TabGroup("Tab/Data")]
	public float AoeRadius { get; private set; }

	[field: SerializeField]
	[field: TabGroup("Tab/Data")]
	public int PierceCount { get; private set; } = 1;

	[Header("Visuals")]
	[field: SerializeField]
	[field: TabGroup("Tab/Visuals")]
	public Sprite Sprite { get; private set; }
}
