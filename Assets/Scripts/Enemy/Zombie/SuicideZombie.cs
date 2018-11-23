using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class SuicideZombie : PatrolAttacker
    {
        [Header("suicide zombie specific")]
        public float explosionRadius = 2;
        public float delayBeforeBoom = 3;
        public float explosionDamage = 1;
        public GameObject explosionEffect;
        public GameObject rageEffect;
        public float regularRunSpeed = 2;
        public float suicideRunSpeed = 3;
        public float rageEffectHeight = 0.7f;
        Transform centerPos;
        public float minDistToPlayer = 1;

        private bool explodingSoon = false;


        protected override void AttackPattern()
        {
            if (!explodingSoon)
            {
                Move(GetPlayerDirection(), regularRunSpeed);
                FacePlayer();
                FaceDirection();
            }
        }


        protected void OnCollisionEnter2D(Collision2D collision)
        {
            if (explodingSoon)
            {
                return;
            }
            if (collision.gameObject.tag == "Player" && IsCollidingWithBody(collision))
            {
                StartCoroutine(AboutToExplode());
            }

            else if (IsFromGround(collision))
            {
                isInGround = true;
            }
        }


        public IEnumerator AboutToExplode()
        {
            if (explodingSoon)
            {
                yield break;
            }
            explodingSoon = true;

            CreateRage();
            StartCoroutine(ManiacRun());

            yield return new WaitForSeconds(delayBeforeBoom);
            ExplodeNow();
        }

        public IEnumerator ManiacRun()
        /*
         * note that it never ends because it ends when enemy is dead
         */
        {
            autoPatrol = false;
            while (true)
            {
                if (!PlayerTooClose())
                {
                    Move(GetPlayerDirection(), suicideRunSpeed);
                }
                yield return null;
            }
        }

        public void CreateRage()
        {
            GameObject temp = Instantiate(rageEffect, transform.up, transform.rotation);
            temp.transform.parent = transform;
            temp.transform.localPosition = new Vector3(0, rageEffectHeight, 0);
        }

        protected void ExplodeNow()
        {
            if(GetDistToPlayer() < explosionRadius)
            {
                DamagePlayer(explosionDamage);
            }
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(this.gameObject);
        }


        public override void DamageEnemy(int damage)
        {
            StartCoroutine(AboutToExplode());
        }

        private bool PlayerTooClose()
        {
            return Mathf.Abs(player.transform.position.x - transform.position.x) < minDistToPlayer;
        }
    }
}
