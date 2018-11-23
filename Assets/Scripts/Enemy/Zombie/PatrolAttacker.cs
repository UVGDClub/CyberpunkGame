using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class PatrolAttacker : PatrolZombie
    {
        [Header("vision and attack")]
        public float agroDist = 3;

        [Tooltip("how far the zombie can 'see' above it")]
        public float overHeadThreshold = 1.2f;
        [Tooltip("how far the zombie can 'see' below it")]
        public float underFootThreshold = 2.5f;

        [Header("legit options")]
        public bool checkIfFacingPlayer = true;

        protected Transform player;
        protected bool InAttackMode;


        protected new void Start()
        /*
        * starts zombie moving and initializes player
        * note: player must have the "Player" tag
        */
        {
            base.Start();
            player = GameObject.FindWithTag("Player").transform;
        }

        public override void Update()
        /*
        * enemy goes into attack mode if within range, otherwise patrols
        */
        {
            if(beingDamaged || isDead)
            {
                base.Update();
                return;
            }

            if (IsPlayerInRange() && ShouldAttack())
            {
                AttackPattern();
                autoPatrol = false;
            }
            else
            {
                autoPatrol = true;//happens in base update
                SetAnimParameter(AnimParams.moveState, 1);
            }
            base.Update();
        }

        protected virtual bool ShouldAttack()
        {
            if(AboveHead() || BelowFeet() || !IsFacingPlayer())
            {
                InAttackMode = false;
                return false;
            }
            InAttackMode = true;
            return true;
        }

        protected virtual void AttackPattern()
        /*
         * left blank so that it can be overridden by child classes
         */
        {
            Debug.Log("Attack pattern initializing");
        }


        protected bool IsPlayerInRange()
        /*
         *returns true if player is within agro range, otherwise false
         */
        {
            if (GetDistToPlayer() < agroDist)
            {
                return true;
            }
            return false;
        }

        protected float GetDistToPlayer()
        {
            return Vector2.Distance(transform.position.ToV2(), player.position.ToV2());
        }

        protected bool AboveHead()
        /*
         * checks if player is above the enemy a significant amount
         * returns true if it is, false if it is not
         */
        {
            if (player.transform.position.y - transform.position.y > overHeadThreshold)
            {
                return true;
            }
            return false;
        }

        protected bool BelowFeet()
        /*
         * checks if player is a significant amount below enemy
         * returns true if they are out of range
         */
        {
            if (transform.position.y - player.transform.position.y > underFootThreshold)
            {
                return true;
            }
            return false;
        }

        protected bool IsFacingPlayer(bool makeFacePlayer = false)
        /*
         * returns true if zombie is looking towards player, otherwase false
         */
        {
            if (!checkIfFacingPlayer)
            {
                return true;
            }

            bool actuallyToRight = IsPlayerToRight();
            return ((goingRight && actuallyToRight) || (!goingRight && !actuallyToRight));
        }

        protected bool IsPlayerToRight(bool setState = false)
        /*
         * returns true if player is to the right of enemy, otherwise false
         */
        {
            if (player.position.x < transform.position.x)
            {
                if (setState)
                {
                    goingRight = false;
                }
                return false;
            }
            if (setState)
            {
                goingRight = true;
            }
            return true;
        }

        protected bool IsCollidingWithBody(Collision2D hitObj)
        /*
         * returns true if object colliding with enemy is colliding
         * on the main body collider
        */
        {
            try
            {
                if (hitObj.otherCollider.sharedMaterial.name == "zombieEdgeDetector")
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
            return false;
        }

        protected Vector3 GetPlayerDirection(bool reversed = false)
        /*
         * returns vector3 pointing in direction of the player from the enemy.
         * if you pass it a bool with the value of true it will give the
         * direction away from the player
         * 
         * also sets going right to the approrpiote value and makes animation
         * match direction it is going.
         */
        {
            if (IsPlayerToRight())
            {
                FaceDirection();
                if (reversed)
                {
                    return Vector2.left;
                }
                return Vector2.right;
            }
            else
            {
                FaceDirection();
                if (reversed)
                {
                    return Vector2.right;
                }
                return Vector2.left;
            }
        }

        protected void FacePlayer()
        {
            goingRight = IsPlayerToRight();
        }

    }

}
