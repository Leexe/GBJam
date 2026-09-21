using FMODUnity;
using UnityEngine;

public class FMODEvents : MonoSingleton<FMODEvents>
{
	[Header("Music")]
	public EventReference Gameplay1_Bgm;
	public EventReference Gameplay2_Bgm;
	public EventReference Gameplay3_Bgm;
	public EventReference Storytime_Bgm;
	public EventReference Title_Bgm;
	public EventReference BgmTest_Bgm;

	[Header("Ambience")]
	public EventReference TheSeaIsReallyMad_Amb;
	public EventReference AmbTest_Amb;

	[Header("SFX - Combat")]
	public EventReference Balista_Sfx;
	public EventReference Cannon_Sfx;
	public EventReference Gun_Sfx;
	public EventReference Melee_Sfx;

	[Header("SFX - Creature Enters")]
	public EventReference CreatureEnters_Sfx;

	[Header("SFX - Kills")]
	public EventReference CrackyKill_Sfx;
	public EventReference SquishKill_Sfx;

	[Header("SFX - Mechanics")]
	public EventReference PickUp_Sfx;
	public EventReference Settle_Sfx;

	[Header("SFX - UI")]
	public EventReference CantClick_Sfx;
	public EventReference CompleteClick_Sfx;
	public EventReference SelectorClick_Sfx;

	[Header("SFX - Test")]
	public EventReference SfxTest_Sfx;

	[Header("SFX - Win / Lose")]
	public EventReference LosingJingle_Sfx;
	public EventReference WinningJingle_Sfx;
}
