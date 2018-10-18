using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class RangedZombie : PatrolZombie
    {
        [Header("RangedThings")]
        public float fireRate = 0.75f;
        public float bulletSpeed = 120;
        public float baseVertSpeed = 1;
        public float baseHorizSpeed = 2;
        public GameObject projectile;


        private Transform leftFirePos;
        private Transform rightFirePos;
        private Quaternion rotator;

        private new void Start()
        {
            base.Start();
            foreach (Transform child in transform)
            {
                if(child.name == "leftFirePoint")
                {
                    leftFirePos = child;
                }
                else if(child.name == "rightFirePoint")
                {
                    rightFirePos = child;
                }
            }
            fireRate = 1 / fireRate;
            rotator.SetEulerAngles(0, 0, 1.57f);
        }

        public override void Update()
        {
            if (IsPlayerInRange())
            {
                canAttack = false;
                //^otherwise will keep walking while firing 
                //and isn't use for anything else here
            }
            else
            {
                canAttack = true;
            }
            base.Update();
        }

        protected override void AttackPattern()
        /*
         *is called whenever player is within agro range
         * can check whichever of the three overriding favtors you want
         */
        {
            if (AboveHead() || BelowFeet() || !IsFacingPlayer())
            {
                canAttack = true;
                timeKeeper = 0;
                PatrolMove();
            }
            else
            {
                RangedAttack();
            }
        }

        private float timeKeeper = 0;
        public void RangedAttack()
        {
            timeKeeper += Time.deltaTime;
            SetAnimParameter(AnimParams.moveState, 0);
            SetAnimParameter(AnimParams.attackState, (GetAnimParameter<int>(AnimParams.attackState) + 1) % 3);

            bool playerToRight = IsPlayerToRight();
            if (timeKeeper > fireRate)
            {
                FireProjectile(playerToRight);
                timeKeeper = 0;
                SetAnimParameter(AnimParams.attackState, 0);
            }

            goingRight = playerToRight;
            FaceDirection();

        }

        private GameObject tempObj;
        private void FireProjectile(bool rightSide)
        {
            if (rightSide)
            {
                tempObj = Instantiate(projectile, rightFirePos.transform.position, rotator);
            }
            else
            {
                tempObj = Instantiate(projectile, leftFirePos.transform.position, rotator);
            }
            tempObj.GetComponent<ZombProjectile>().
                SetVars(bulletSpeed, damage, rightSide, baseHorizSpeed, baseVertSpeed, 
                useBloodEffect, bloodEffect);
        }


        protected override void OnCollisionEnter2D(Collision2D collision)
        /*
         * on a collision this enemy dies
         */
        {

            if (collision.gameObject.tag == "Player")
            {
                ThisEnemyDead();
            }
        }
       
    }
}
