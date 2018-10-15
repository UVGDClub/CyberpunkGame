using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player : MonoBehaviour {

    public Rigidbody2D rigidbody2d;
    public APlayerState currentState;

    public float groundedDistance = 1f;

	// Use this for initialization
	void Start () {
        rigidbody2d = GetComponent<Rigidbody2D>();

    }

    public void Update() {
        currentState.InputCheck(this);
    }

    // Update is called once per frame
    void FixedUpdate () {

        RaycastHit2D hit;
        if (hit = Physics2D.Raycast(transform.position, Vector2.down, groundedDistance)) {
            
        }


        currentState.Execute(this);
	}
}
