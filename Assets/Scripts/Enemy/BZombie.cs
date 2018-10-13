using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//make patrol bot
//put height limit
// on hit do damage while colliding
//or push back on hit
namespace Enemy
{
    public class BZombie : EnemyBehaviour
    {
        public float walkTime;
        public float runSpeed;
        public float agroDist;
        public float knockbackAmount;
        public float overheadThreshold;

        private bool canAttack;
        private float playDist;
        private Vector3 direction;
        private Transform player;
        private bool playerInRange = false;
        private bool patrolOverride = false;

        public void Start()
        {
            StartCoroutine(PatrolMove());
            SetAnimParameter(AnimParams.moveState, 1);
            player = GameObject.FindWithTag("Player").transform;
        }

        public override void Update()
        {
            base.Update();
            playDist = Vector2.Distance(transform.position.ToV2(), player.position.ToV2());
            if (playDist < agroDist)
            {
                AttackPattern();
            }
            else
            {
                transform.position += direction * Time.deltaTime;
            }

        }

        public IEnumerator PatrolMove()
        {
            while (!playerInRange || patrolOverride)
            {
                //move left
                FacingDirection = Direction.Right;
                direction = Vector2.left.ToV3();

                yield return new WaitForSeconds(walkTime);

                //move right
                FacingDirection = Direction.Left;
                direction = Vector2.right.ToV3();

                yield return new WaitForSeconds(walkTime);
            }
        }

        private Vector3 getPlayerDirection()
        {
            if (player.position.x < transform.position.x)
            {
                FaceDirection(false);
                return Vector2.left.ToV3();
            }
            else
            {
                FaceDirection(true);
                return Vector2.right.ToV3();
            }
        }

        private void FaceDirection(bool goingRight)
        //changes the animation to face the specified way
        {
            if (goingRight)
            {
                FacingDirection = Direction.Left;
            }
            else
            {
                FacingDirection = Direction.Right;//WHY ARE THESE BACKWARDS
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player" && canAttack)
            {
                canAttack = false;
                KnockPlayerBack();

            }
        }
        private void OnCollisionExit2D(Collision2D collide)
        {
            if (collide.collider.tag == "Player")
            {
                canAttack = true;
            }
        }

        private bool AboveHead()
        //checks if player is above the enemy a significant amount
        //returns true if it is, false if it is not
        {
            if (player.transform.position.y - transform.position.y > overheadThreshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ForcedPatrol()
        {
                patrolOverride = true;
                StartCoroutine(PatrolMove());
        }

        private void KnockPlayerBack()
        {
            //empty till player script is finished
        }

        private void AttackPattern() {
            if (AboveHead())
            {
                patrolOverride = true;
                transform.position += direction * Time.deltaTime;
            }
            else
            {
                patrolOverride = false;
                transform.position += getPlayerDirection() * Time.deltaTime * runSpeed;
            }
        }
    
    }
}


    
