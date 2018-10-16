﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class PatrolZombie : EnemyBehaviour
    {
        [Header("Basic Settings")]
        public float walkSpeed = 1;
        [Tooltip("for when it sees the player")]
        public float runSpeed = 3;
        public float agroDist;

        [Header("vision and attack")]
        [Tooltip("how far the zombie can 'see' above it")]
        public float overHeadThreshold = 1.2f;
        [Tooltip("how far the zombie can 'see' below it")]
        public float underFootThreshold = 2.5f;
        [Tooltip("how slowed the zombie is while attacking")]
        public float attackSlowAmount = 2f;
        [Tooltip("how much the zombie is teleported back after attacking")]
        public float displacementAmount = 0.9f;
        [Tooltip("how much time is allowed for attack animation, careful here")]
        public float attackRecoilTime = 0.35f;
        [Tooltip("BLOOD FOR THE BLOOD GOD")]
        public GameObject bloodEffect;
    
        [Header("legit options")]
        public bool checkIfFacingPlayer = true;
        [Tooltip("LEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEROY JENKINS")]
        public bool suicidesForPlayer = true;
        public bool useBloodEffect = true;

        private bool canAttack = true;
        private float playerDist;
        private Transform player;
        private bool goingRight = false;
        private bool chasingPlayer;
        private bool isInGround = true;

        public void Start()
        /*
         * starts zombie moving and initializes player
         * note: player must have the "Player" tag
         */
        {
            SetAnimParameter(AnimParams.moveState, 1);
            player = GameObject.FindWithTag("Player").transform;
        }

        public override void Update()
        /*
         * enemy goes into attack mode if within range, otherwise patrols
         */
        {
            base.Update();
            //SetAnimParameter(AnimParams.attackState, (GetAnimParameter<int>(AnimParams.attackState) + 1) % 3);
            if (IsPlayerInRange())
            {
                AttackPattern();
            }
            else
            {
                PatrolMove();
                chasingPlayer = false;
            }

        }

        private bool IsPlayerInRange()
        /*
         *returns true if player is within agro range, otherwise false
         */
        {
            playerDist = Vector2.Distance(transform.position.ToV2(), player.position.ToV2());
            if (playerDist < agroDist)
            {
                return true;
            }
            return false;
        }

        public void PatrolMove()
        /*
         * makes the enemy move back and forth in accordance with the goingRight bool
         */
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

        private void FaceDirection()
        /*
         * changes the animation to face the specified way
         */
        {
            if (canAttack)
            //canAttack is false only during attack animation,
            //this is here so movestate wont override it
            {
                SetAnimParameter(AnimParams.moveState, 1);
            }

            if (goingRight)
            {
                FacingDirection = Direction.Left;///WHY ARE THESE BACKWARDS
            }
            else
            {
                FacingDirection = Direction.Right;
            }
        }

        private Vector3 GetPlayerDirection(bool reversed = false)
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
                goingRight = true;
                FaceDirection();
                if (reversed)
                {
                    return Vector2.left.ToV3();
                }
                return Vector2.right.ToV3();
            }
            else
            {
                goingRight = false;
                FaceDirection();
                if (reversed)
                {
                    return Vector2.right.ToV3();
                }
                return Vector2.left.ToV3();
            }
        }

        private bool IsPlayerToRight()
        /*
         * returns true if player is to the right of enemy, otherwise false
         */
        {
            if (player.position.x < transform.position.x)
            {
                return false;
            }
            return true;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        /*
         * on a collision if it is the player damages player
         */
        {
            if(collision.gameObject.tag == "Player" && canAttack && IsCollidingWithBody(collision))
            {
                DamagePlayer();
            }
            if (IsFromGround(collision))
            {
                isInGround = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        /*
         * if enemy collider is stopping overlapping with terrain
         * direction moving is reversed.
         * note: you must have two colliders below enemy
         * on left and right.
         */
        {
            if(collision.gameObject.tag == "Player")
            {
                return;
            }
            if (IsFromGround(collision))
            {
                isInGround = false;
                if ((!chasingPlayer || !suicidesForPlayer))
                {
                    ChangeDirection();
                }
            }
        }

        private bool AboveHead()
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

        private bool BelowFeet()
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

        private bool IsFacingPlayer()
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

        public void ChangeDirection()
        /*
         *reverses the direction
         */
        {
            goingRight = !goingRight;
        }

        private void AttackPattern()
        /*
         *is called whenever player is within agro range
         * can check whichever of the three overriding favtors you want
         */
        {
            if (AboveHead() || BelowFeet() || !IsFacingPlayer())
            {
                PatrolMove();
                chasingPlayer = false;
            }
            else
            {
                ChasePlayer();
                chasingPlayer = true;
            }
        }

        private void ChasePlayer()
        /*
         *moves enemy closer to the player
         */
        {
            checkIfFacingPlayer = true;
            if (suicidesForPlayer || IsFacingPlayer() || isInGround)//XXX
            {
                transform.position += GetPlayerDirection() * Time.deltaTime * runSpeed;
            }
            else
            {
                //FaceDirection();
            }
            checkIfFacingPlayer = false;

        }

        private bool IsFromGround(Collision2D hitThing)
        /*
         *returns true if the collision is with the terrain below it.
         * 
         * note: the leg colliders must have the physics material 
         * called "zombieEdgeDetector in order for it to work
         */
        {
             if(hitThing.gameObject.layer == LayerMask.NameToLayer("Ground"))
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

        private bool IsCollidingWithBody(Collision2D hitObj)
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

        private void DamagePlayer()
        /*
         *is activated when enemy makes contact with player
         * should be filled out more once player script is done
         */
        {
            StartCoroutine(AttackAnimation());
            Debug.Log("PLAYER DAMAGED");
        }

        private IEnumerator AttackAnimation()
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
            while (timer <= attackRecoilTime)
            {
                timer += Time.deltaTime;
                SetAnimParameter(AnimParams.attackState, (GetAnimParameter<int>(AnimParams.attackState) + 1) % 3);
                yield return null;
            }

            SetAnimParameter(AnimParams.moveState, 1);
            canAttack = true;
            runSpeed *= attackSlowAmount;
            if (useBloodEffect)
            {
                //temporary effect,replace with better alternative later
                Instantiate(bloodEffect, player.transform.position, player.transform.rotation);
            }

            transform.position += GetPlayerDirection(true) * displacementAmount;
            //teleports enemy away from player, prevents bugs from never leaving
            //contact with player, should be replaced with better solution or deleted if not needed
        }

    }
}