using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class SpawnerZombie : RushZombie
    {
        [Header("Spawner specific Things")]
        [Tooltip("time between birth effect and thing spawning")]
        public float birthDelay = 0.1f;
        [Tooltip("name in def of enemy you want to spawn")]
        public string zombToSpawn = "SpawnerZombie";
        public GameObject birthEffect;

        private GameObject spriteChild;

        protected new void Start()
        {
            base.Start();
            spriteChild = transform.GetChild(0).gameObject;
        }

        public override void Update()
        {
            base.Update();
        }

        protected override IEnumerator KillEnemy()
        {
            isDead = true;
            yield return new WaitForSeconds(deathDuration);

            spriteChild.SetActive(false);
            Instantiate(birthEffect, transform.position, transform.rotation);

            yield return new WaitForSeconds(birthDelay);
            SpawnEnemy();
            Destroy(this.gameObject);
        }

        private void SpawnEnemy()
        {
            EnemyManager.SpawnEnemy(zombToSpawn, gameObject.transform.position, Direction.Right);
        }
    }
}
