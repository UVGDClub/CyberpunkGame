using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player : MonoBehaviour {

    public Rigidbody2D rigidbody2d;
    public APlayerState currentState;

    public float groundedDistance = 1f;
	public float groundCheckOffset = -0.3201f;
    public bool grounded = false;

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

	    var start = new Vector2(transform.position.x - 0.32f, transform.position.y + groundCheckOffset);

	    var direction = new Vector2(0.9f, -0.1f).normalized;
	    var length = groundedDistance;

		Debug.DrawRay(start, direction*length);

		if (hit = Physics2D.Raycast(start, direction, length)) {
		    grounded = true;
	    }
	    else {
		    grounded = false;
	    }

        currentState.Execute(this);
	}
}
