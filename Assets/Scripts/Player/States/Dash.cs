using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/DashState")]
public class Dash : APlayerState {

    public float distance;
    public float speed;

    public float minTimeBetweenDashes;

    private float timeToNext = 0;

    public override void OnEnter( Player player ) {
        player.canDash = false;
        player.StartCoroutine(DoDash(player));
        timeToNext = Time.time + minTimeBetweenDashes;
    }

    IEnumerator DoDash(Player p) {
        CanTransitionOutOf = false;
        Vector3 vec = p.rigidbody2d.velocity;
        Vector3 start = p.transform.position;
        float end = start.x + distance;
        float endTime = Time.time + distance / speed; 

        while(p.transform.position.x < end && Time.time < endTime) {
            vec.x = (p.facing == Direction.Right ? 1 : -1) * speed;
            vec.y = 0;
            p.rigidbody2d.velocity = vec;
            yield return null;
        }

        CanTransitionOutOf = true;
    }

    public override bool CanTransitionInto( Player player ) {
        Debug.Log("Time Good: " + (Time.time > timeToNext) + "Time: " + Time.time + "Time to next: " + timeToNext);
        return player.canDash && Input.GetKey(KeyCode.E) && Time.time > timeToNext;
    }

}
