using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Defs
{
	[CreateAssetMenu(fileName = "NewEnemyDef", menuName = "DataDef/EnemyDef")]
	public class EnemyDef : DataDef
	{
		[HelpBox("[Enemy Def]\n\n" +
			"Relevant information about this def.", HelpBoxMessageType.Info)]
		[Space()]

		[Header("General Info")]
		public string Name;
		public int Health;
		public int Damage;
		public float Speed;

		[Header("Animations")]
		public AnimatorOverrideController Animations;
	}
}