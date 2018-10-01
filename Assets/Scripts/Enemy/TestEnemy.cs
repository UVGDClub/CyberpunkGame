using UnityEngine;
using Utils;

namespace Enemy
{
	public class TestEnemy : EnemyBase
	{
		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				SetAnimParameter(AnimParams.isGrounded, !GetAnimParameter<bool>(AnimParams.isGrounded));
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				SetAnimParameter(AnimParams.isDead, !GetAnimParameter<bool>(AnimParams.isDead));
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				SetAnimParameter(AnimParams.jump, true);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				SetAnimParameter(AnimParams.takeDamage, true);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				SetAnimParameter(AnimParams.moveState, (GetAnimParameter<int>(AnimParams.moveState) + 1) % 3);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				SetAnimParameter(AnimParams.attackState, (GetAnimParameter<int>(AnimParams.attackState) + 1) % 3);
			}
		}
	}
}