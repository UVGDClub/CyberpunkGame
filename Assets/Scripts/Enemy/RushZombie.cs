using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class RushZombie : PatrolAttacker
    {
        [Tooltip("how slowed the zombie is while attacking")]
        public float attackSlowAmount = 2f;
        [Tooltip("how much the zombie is teleported back after attacking")]
        public float displacementAmount = 0.9f;
        [Tooltip("how long the attack animation has to go for (probably don't change this)")]
        public float timeForAttackAnim = 0.35f;
        [Tooltip("how much time is allowed for attack animation, careful here")]
        public float attackRecoilTime = 0.35f;
        public float damageAmount;

        [Tooltip("for when it sees the player")]
        public float runSpeed = 3;


        [Tooltip("LEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEROY JENKINS")]
        public bool suicidesForPlayer = true;

        protected bool canAttack = true;

        public override void Update()
        {
            base.Update();
        }

        protected override void AttackPattern()
        {
            if ((suicidesForPlayer || isInGround) && canAttack)
            {
                ChasePlayer();
            }
        }

        protected void ChasePlayer()
        /*
         *moves enemy closer to the player
         */
        {
            transform.position += GetPlayerDirection() * Time.deltaTime * runSpeed;
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        /*
         * on a collision if it is the player damages player
         */
        {
            if (collision.gameObject.tag == "Player" && canAttack && IsCollidingWithBody(collision))
            {
                if (!IsFacingPlayer())
                {
                    ChangeDirection();
                }
                StartCoroutine(AttackAnimation());
            }
            else if (IsFromGround(collision))
            {
                isInGround = true;
            }
        }



        protected IEnumerator AttackAnimation()
        /*
         * activates the attack animation, slows enemy while it is attacking,
         * creates blood effect at player and knocks itself back after hit.
         */
        {
            SetAnimParameter(AnimParams.moveState, 0);
            canAttack = false;
            runSpeed /= attackSlowAmount;

            float timer = 0;
            SetAnimParameter(AnimParams.attackState, 0);//so animation starts from same position every time
            while (timer <= timeForAttackAnim)
            {
                timer += Time.deltaTime;
                SetAnimParameter(AnimParams.attackState, (GetAnimParameter<int>(AnimParams.attackState) + 1) % 3);
                yield return null;
            }

            SetAnimParameter(AnimParams.moveState, 1);
            runSpeed *= attackSlowAmount;

            DamagePlayer(damageAmount);
            transform.position += GetPlayerDirection(true) * displacementAmount;

            yield return new WaitForSeconds(attackRecoilTime);
            canAttack = true;
            //teleports enemy away from player, prevents bugs from never leaving
            //contact with player, should be replaced with better solution or deleted if not needed
        }
    }
}
