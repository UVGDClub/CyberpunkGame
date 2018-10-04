using Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Debugging
{
	public class EnemyDebugger : MonoBehaviour
	{
		public Transform[] spawns;
		private EnemyBehaviour[] enemies;

		private void Start()
		{
			enemies = new EnemyBehaviour[spawns.Length];
		}
		void Update()
		{
			//spawns/destruction
			for (int i = 0, j = (int)KeyCode.Alpha1; i < spawns.Length; i++, j++)
			{
				if (Input.GetKeyDown((KeyCode)j))
				{
					if (enemies[i] == null)
						enemies[i] = EnemyManager.SpawnEnemy("test_enemy_" + ((i % 2) + 1), spawns[i].position, (i % 2 == 0) ? Direction.Right : Direction.Left);
					else
					{
						EnemyManager.DestroyEnemy(enemies[i]);
						enemies[i] = null;
					}
				}
			}
		}
	}
}