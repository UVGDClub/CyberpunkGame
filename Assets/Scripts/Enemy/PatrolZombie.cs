using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    //122 quiz tuesday
namespace Enemy
{
    public class PatrolZombie : EnemyBehaviour
    {
        public float walkSpeed = 1;
        public float runSpeed = 3;
        public float agroDist;
        public float overHeadThreshold;
        public GameObject bloodEffect;
        //public float underFootThreshold = 0;

        private bool canAttack = true;
        private float playerDist;
        private Transform player;
        private bool goingRight = false;

        public void Start()
        {
            SetAnimParameter(AnimParams.moveState, 1);
            player = GameObject.FindWithTag("Player").transform;
        }

        public override void Update()
        {
            base.Update();

            if (IsPlayerInRange())
            {
                AttackPattern();
            }
            else
            {
                PatrolMove();
            }

        }

        public void PatrolMove()
        //makes the enemy move back and forth, changing direction if it hits an edge
        {
            if (goingRight)
            {
                transform.position += Vector2.right.ToV3() * Time.deltaTime * walkSpeed;
            }
            else
            {
                transform.position += Vector2.left.ToV3() * Time.deltaTime * walkSpeed;
            }
            FaceDirection();
        }

        //returns vector3 pointing in direction of player from enemy
        private Vector3 GetPlayerDirection(bool reversed = false)
        {
            if (player.position.x < transform.position.x)
            {
                goingRight = false;
                FaceDirection();
                if (reversed)
                {
                    return Vector2.right.ToV3();
                }
                return Vector2.left.ToV3();
            }
            else
            {
                goingRight = true;
                FaceDirection();
                if (reversed)
                {
                    return Vector2.left.ToV3();
                }
                return Vector2.right.ToV3();
            }
        }

        private void FaceDirection()
        //changes the animation to face the specified way
        {
            SetAnimParameter(AnimParams.moveState, 1);
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
                DamagePlayer();

            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.collider.tag == "Player")
            {
                canAttack = true;
            }
            else if (IsFromGround(collision))
            {
                ChangeDirection();
            }
        }

        private bool AboveHead()
        //checks if player is above the enemy a significant amount
        //returns true if it is, false if it is not
        {
            if (player.transform.position.y - transform.position.y > overHeadThreshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void DamagePlayer()
        {
            //fill in once player script is finished
            Debug.Log("PLAYER IS DAMAGED");
            Instantiate(bloodEffect, transform.position, transform.rotation);
            //do stuff with movement
            transform.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.right);
        }

        private void AttackPattern()
        {
            if (AboveHead())
            {
                PatrolMove();
            }
            else
            {
                ChasePlayer();
            }
        }

        private void ChasePlayer()
        {
            transform.position += GetPlayerDirection() * Time.deltaTime * runSpeed;
        }


        public void ChangeDirection()
        {
            goingRight = !goingRight;
        }

        private bool IsFromGround(Collision2D hitThing)
        {
             if(hitThing.gameObject.layer == LayerMask.NameToLayer("Ground"))
             {
                /*
                ContactPoint2D hitPosition = hitThing.GetContact(0);
                float heightDiff = transform.position.y - hitPosition.point.y;

                if(heightDiff > underFootThreshold)
                {
                    return true;
                }*/
                return true;
            }
            return false;//is called only if other one isn't

        }

        private bool IsPlayerInRange()
        {
            playerDist = Vector2.Distance(transform.position.ToV2(), player.position.ToV2());
            if (playerDist < agroDist)
            {
                return true;
            }
            return false;
        }

    }
}
