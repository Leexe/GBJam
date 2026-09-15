using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSO", menuName = "Game/ProjectileSO", order = 0)]
public class ProjectileSO : ScriptableObject
{
	[Header("Context")]
	[TabGroup("Tab", "Context")]
	[field: SerializeField]
	public string Name { get; private set; }

	[TabGroup("Tab", "Context")]
	[field: SerializeField]
	public string Id { get; private set; }

	[Header("Data")]
	[TabGroup("Tab", "Data")]
	[field: SerializeField]
	public float Speed { get; private set; } = 8f;

	[TabGroup("Tab", "Data")]
	[field: SerializeField]
	public float Damage { get; private set; } = 10f;

	[TabGroup("Tab", "Data")]
	[field: SerializeField]
	public float Lifetime { get; private set; } = 4f;

	[TabGroup("Tab", "Data")]
	[field: SerializeField]
	public int PierceCount { get; private set; } = 1;

	[TabGroup("Tab", "Data")]
	[field: SerializeField]
	public bool IsAoe { get; private set; }

	[TabGroup("Tab", "Data")]
	[ShowIf(nameof(IsAoe))]
	[field: SerializeField]
	public float AoeRadius { get; private set; } = 1.5f;

	[Header("Visuals")]
	[TabGroup("Tab", "Visuals")]
	[field: SerializeField]
	public Sprite Sprite { get; private set; }
}
