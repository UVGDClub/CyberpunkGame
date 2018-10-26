using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class RangedZombie : PatrolAttacker
    {
        [Header("RangedThings")]
        [Tooltip("rate of shooting bullets, higher number means faster shooting. *Note changing it mid game won't change it")]
        public float fireRate = 0.75f;
        [Tooltip("speed multiplier of bullet, keep it pretty big, like more than 100")]
        public float bulletSpeed = 120;
        public int bulletDamage = 1;
        [Tooltip("vertical component of speed of bullet")]
        public float baseVertSpeed = 1;
        [Tooltip("horizontal component of speed of bullet")]
        public float baseHorizSpeed = 2;
        [Tooltip("used to check if player was recently in range, probably don't change this")]
        public float smallTimeAmount = 0.1f;
        public GameObject projectile;


        private Transform leftFirePos;
        private Transform rightFirePos;
        private Quaternion rotator;
        private float lastTime = -1;
        private float trueFireRate;
        private float timeKeeper = 0;

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
            trueFireRate = 1 / fireRate;
            rotator.SetEulerAngles(0, 0, 1.57f);
        }

        public override void Update()
        {
            base.Update();
        }

        protected override void AttackPattern()
        /*
         *is called whenever player is within agro range
         * 
         */
        {
            if((Time.time - lastTime > smallTimeAmount) || lastTime == -1)
            {
                timeKeeper = 0;
            }
            lastTime = Time.time;

            RangedAttack();
        }

        public void RangedAttack()
        {

            timeKeeper += Time.deltaTime;
            SetAnimParameter(AnimParams.moveState, 0);
            SetAnimParameter(AnimParams.attackState, (GetAnimParameter<int>(AnimParams.attackState) + 1) % 3);

            bool playerToRight = IsPlayerToRight();
            if (timeKeeper > trueFireRate)
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
                SetVars(bulletSpeed, bulletDamage, rightSide, baseHorizSpeed, baseVertSpeed);
        }
    }
}
