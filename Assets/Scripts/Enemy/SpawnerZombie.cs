using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class SpawnerZombie : PatrolZombie
    {
        [Header("Spawner specific Things")]
        public GameObject zombToSpawn;
        public GameObject birthEffect;
        public bool testingForDeath = false;

        public override void Update()
        {
            base.Update();
            if (testingForDeath)
            {
                testingForDeath = false;
                EnemyDead();

            }
        }

        protected override IEnumerator DeathAnim()
        {
            yield return new WaitForSeconds(decompositionTime);
            SpawnBeby();
            Destroy(this.gameObject);
        }

        private void SpawnBeby()
        {
            Instantiate(birthEffect, transform.position, transform.rotation);
            Instantiate(zombToSpawn, transform.position, transform.rotation);
        }
    }
}
