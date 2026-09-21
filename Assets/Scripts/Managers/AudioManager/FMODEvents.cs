using System.Diagnostics.CodeAnalysis;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "Odin.OdinUnknownGroupingPath")]
public class FMODEvents : MonoSingleton<FMODEvents>
{
	#region Music

	[field: SerializeField]
	[field: FoldoutGroup("Music", true)]
	public EventReference Gameplay1_Bgm { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Music")]
	public EventReference Gameplay2_Bgm { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Music")]
	public EventReference Gameplay3_Bgm { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Music")]
	public EventReference Storytime_Bgm { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Music")]
	public EventReference Title_Bgm { get; private set; }

	#endregion

	#region Ambience

	[field: SerializeField]
	[field: FoldoutGroup("Ambience", true)]
	public EventReference TheSeaIsReallyMad_Amb { get; private set; }

	#endregion

	#region SFX

	[field: SerializeField]
	[field: FoldoutGroup("SFX/Combat", true)]
	public EventReference Balista_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/Combat")]
	public EventReference Cannon_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/Combat")]
	public EventReference Gun_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/Combat")]
	public EventReference Melee_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/Creature Enters", true)]
	public EventReference CreatureEnters_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/Kills", true)]
	public EventReference CrackyKill_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/Kills")]
	public EventReference SquishKill_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/Mechanics", true)]
	public EventReference PickUp_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/Mechanics")]
	public EventReference Settle_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/UI", true)]
	public EventReference CantClick_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/UI")]
	public EventReference CompleteClick_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/UI")]
	public EventReference SelectorClick_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/YouWin YouLose", true)]
	public EventReference LosingJingle_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX/YouWin YouLose")]
	public EventReference WinningJingle_Sfx { get; private set; }

	#endregion
}
