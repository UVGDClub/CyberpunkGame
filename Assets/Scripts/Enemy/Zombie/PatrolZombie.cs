using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class PatrolZombie : EnemyBehaviour
    {
        [Header("movement")]
        public float walkSpeed = 1;
        public float damagedDuration = 2;
        public int health = 5;
        [Tooltip("how long before body dissapears")]
        public float deathDuration = 1;

        protected bool goingRight = false;
        protected bool isInGround = true;
        protected bool autoPatrol = true;
        protected bool beingDamaged = false;
        protected bool isDead = false;

        public bool damageTest = false;

        protected virtual void Start()
        {
            SetAnimParameter(AnimParams.moveState, 1);
        }


        public override void Update()
        {
            base.Update();
            if (autoPatrol && !beingDamaged && !isDead)
            {
                PatrolMove();
            }

            if (damageTest)//temporary for testing
            {
                damageTest = false;
                DamageEnemy(1);
            }
        }

        public void PatrolMove()
        /*
         * makes the enemy move back and forth in accordance with the goingRight bool
         */
        {
            if (goingRight)
            {
                Move(Vector2.right, walkSpeed);
            }
            else
            {
                Move(Vector2.left, walkSpeed);
            }
            FaceDirection();
        }

        protected void Move(Vector2 Dir, float speedMultiplier)
        {
            transform.position += Dir.ToV3() * Time.deltaTime * speedMultiplier;
            //Debug.Log("moving");
        }

        protected void FaceDirection()
        /*
         * changes the animation to face the specified way
         */
        {

            if (goingRight)
            {
                FacingDirection = Direction.Left;///WHY ARE THESE BACKWARDS
            }
            else
            {
                FacingDirection = Direction.Right;
            }
        }

        protected void OnCollisionExit2D(Collision2D collision)
        /*
         * if enemy collider is stopping overlapping with terrain
         * direction moving is reversed.
         * note: you must have two colliders below enemy
         * on left and right.
         */
        {
            if (collision.gameObject.tag == "Player")
            {
                return;
            }
            else if (IsFromGround(collision))
            {
                isInGround = false;
                ChangeDirection();
            }
        }


        public void ChangeDirection()
        /*
         *reverses the direction
         */
        {
            goingRight = !goingRight;
        }

        protected void DamagePlayer(float damageAmount)
        {
            Debug.Log("Player is damaged");
        }

        public virtual void DamageEnemy(int damage)
        {
            health -= damage;
            if (health <= 0)
            {
                SetAnimParameter(AnimParams.isDead, true);
                SetAnimParameter(AnimParams.takeDamage, true);//just so you can see it, is dead doesn't work rn
                Debug.Log("enemy is dead");
                StartCoroutine(KillEnemy());
            }
            else
            {
                StartCoroutine(DamagedAnim());
            }
        }

        protected virtual IEnumerator KillEnemy()
        {
            isDead = true;
            yield return new WaitForSeconds(deathDuration);
            Destroy(this.gameObject);
        }

        protected IEnumerator DamagedAnim()
        {
            beingDamaged = true;
            SetAnimParameter(AnimParams.takeDamage, true);
            yield return new WaitForSeconds(damagedDuration);
            beingDamaged = false;

        }



        protected bool IsFromGround(Collision2D hitThing)
        /*
         *returns true if the collision is with the terrain below it.
         * 
         * note: the leg colliders must have the physics material 
         * called "zombieEdgeDetector in order for it to work
         */
        {
            if (hitThing.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {//if thing it left is on ground layer
                try
                {
                    if (hitThing.otherCollider.sharedMaterial.name == "zombieEdgeDetector")
                    {//if collider has material zombieEdgeDetector
                        return true;
                    }
                }
                catch
                {//throws error if no material
                    return false;
                }
            }
            return false;

        }
    }
}