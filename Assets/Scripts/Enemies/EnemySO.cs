using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Game/EnemySO", order = 0)]
public class EnemySO : ScriptableObject
{
	public string Name;
	public float Health;
	public float Speed;

	[PreviewField(50, ObjectFieldAlignment.Right)]
	public List<Sprite> SpriteList;
}
