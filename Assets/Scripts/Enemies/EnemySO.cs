using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Game/EnemySO", order = 0)]
public class EnemySO : ScriptableObject
{
	public string Name;

	[TextArea(3, 10)]
	public string Description;
	public float Health;
	public float Speed;
	public int Damage;
	public int GoldReward;

	[PreviewField(50, ObjectFieldAlignment.Right)]
	public List<Sprite> SpriteList;

	public Sprite Icon => SpriteList[0];
}
