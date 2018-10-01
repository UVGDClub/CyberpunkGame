///<summary>
/// Created by Glen McManus September 27, 2018
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    //it's not redundant, it's caching :)
    public Transform myTransform;
    public float moveSpeed = 3f;
    public float jumpForce = 5f;

    [Header("Collision")]
    public Rigidbody2D rigidbody2d;
    public BoxCollider2D boxCollider2d;
    public float groundingRayDistance = 0.01f;
    public LayerMask terrainLayerMask;

    [Header("States")]
    protected StateMachine stateMachine;
    public PlayerAirState airState = new PlayerAirState();
    public PlayerAttackState attackState = new PlayerAttackState();
    public PlayerDeadState deadState = new PlayerDeadState();
    public PlayerDodgeState dodgeState = new PlayerDodgeState();
    public PlayerStruckState struckState = new PlayerStruckState();
    public PlayerIdleState idleState = new PlayerIdleState();
    public PlayerMoveState moveState = new PlayerMoveState();
    public PlayerStateTransitions stateTransitions = new PlayerStateTransitions();

    private void Awake()
    {
        stateTransitions.player = this;
        airState.player = this;
        attackState.player = this;
        deadState.player = this;
        dodgeState.player = this;
        struckState.player = this;
        idleState.player = this;
        moveState.player = this;
        stateMachine = new StateMachine(idleState);
    }

    private void Start()
    {
        StartCoroutine(RunStateMachine());
    }

    /* @TODO:
     * Make this clean and implement higher jumps depending on duration key is held
     */ 
    public void Jump()
    {
        rigidbody2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public bool isGrounded()
    {
        Debug.DrawRay(myTransform.position, Vector2.down * groundingRayDistance, Color.yellow);

        if (Physics2D.Raycast(myTransform.position, Vector2.down, groundingRayDistance, terrainLayerMask) == true)
        {
            //Debug.Log("is grounded");
            return true;
        }

        Debug.Log("not grounded");
        return false;
    }

    /*
    * You could add functionality within the foreach, checking the type of 'e' and such to do fancy things.
    */
    private IEnumerator RunStateMachine()
    {
        while (true)
        {
            foreach (var e in stateMachine.Execute())
            {
                yield return null;
            }
        }
    }
}
