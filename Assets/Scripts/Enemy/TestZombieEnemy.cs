using UnityEngine;
using Utils;

namespace Enemy
{
	public class TestZombieEnemy : EnemyBehaviour
	{
		public override void Update()
		{
			base.Update();

			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				SetAnimParameter(AnimParams.isGrounded, !GetAnimParameter<bool>(AnimParams.isGrounded));
			}
			else if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				SetAnimParameter(AnimParams.isDead, !GetAnimParameter<bool>(AnimParams.isDead));
			}
			else if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				SetAnimParameter(AnimParams.jump, true);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				SetAnimParameter(AnimParams.takeDamage, true);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha8))
			{
				SetAnimParameter(AnimParams.moveState, (GetAnimParameter<int>(AnimParams.moveState) + 1) % 3);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha9))
			{
				SetAnimParameter(AnimParams.attackState, (GetAnimParameter<int>(AnimParams.attackState) + 1) % 3);
			}

			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				FacingDirection = Direction.Left;
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				FacingDirection = Direction.Right;
			}
		}
	}
}