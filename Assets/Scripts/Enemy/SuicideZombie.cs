using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class SuicideZombie : PatrolZombie
    {
        [Header("suicide zombie specific")]
        public float explosionRadius = 2;
        public float delayBeforeBoom = 3;
        public GameObject explosionEffect;
        public GameObject rageEffect;
        public float minimumDist;
        public float rageEffectHeight = 0.7f;
        public float redShift = 5;
        Transform centerPos;

        private bool explodingSoon = false;

        new void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            base.Update();
        }

        public IEnumerator AboutToExplode()
        {
            if (explodingSoon)
            {
                yield break;
            }
            explodingSoon = true;

            StartCoroutine(EnragedAnim());
            CreateRage();

            yield return new WaitForSeconds(delayBeforeBoom);
            ExplodeNow();
        }

        public void CreateRage()
        {
            GameObject temp = Instantiate(rageEffect, transform.up, transform.rotation);
            temp.transform.parent = transform;
            temp.transform.localPosition = new Vector3(0, rageEffectHeight, 0);
        }

        private IEnumerator EnragedAnim()
        {
            SetAnimParameter(AnimParams.moveState, 0);
            while (true)
            {
                SetAnimParameter(AnimParams.moveState, 1);
                yield return null;
            }
        }

        protected void ExplodeNow()
        {
            if(GetPlayerDist() < explosionRadius && IsLineOfSight())
            {
                DamagePlayer();
            }
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(this.gameObject);
        }

        private bool IsLineOfSight()
        {
            return true;//fix this mess at some point
            int layermask = (LayerMask.GetMask("Enemy"));
            RaycastHit2D hitPlayer = Physics2D.Raycast(centerPos.position, GetPlayerDir(), explosionRadius+1, layermask);
            Debug.Log(hitPlayer.transform.name);
            if(hitPlayer.collider.tag == "player")
            {
                return true;
            }
            return false;
        }
        private Vector2 GetPlayerDir()
        {
            Vector2 playerDir = Vector2.one;
            playerDir.x *= transform.position.x - player.transform.position.x;
            playerDir.y *= transform.position.y - player.transform.position.y;
            playerDir.Normalize();
            return playerDir;
        }

        protected override void OnCollisionEnter2D(Collision2D collision)
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

        public override bool DamageZombie(int damage)
        {
            StartCoroutine(AboutToExplode());
            return true;
        }

        protected override void ChasePlayer()
        {
            transform.position += GetPlayerDirection() * Time.deltaTime * runSpeed;
        }
    }
}
