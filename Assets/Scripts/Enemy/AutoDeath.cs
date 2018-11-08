using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDeath : MonoBehaviour {

    public float deathDelay;

	void Start () {
        StartCoroutine(DeathsEmbrace());
	}
	
    private IEnumerator DeathsEmbrace()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(this.gameObject);
    }
}
