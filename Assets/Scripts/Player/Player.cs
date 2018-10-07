///<summary>
/// Created by Glen McManus September 27, 2018
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaycastController))]
public class Player : MonoBehaviour
{

    public LevelGrid levelgrid;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float jumpForce = 5f;
    public int maxJumpIterations = 5;

    [Header("Collision")]
    public BoxCollider2D collider;
    public Rigidbody2D rigidbody2d;
    public float maxForwardSlopeCastDistance = 0.3f;
    public float maxDownSlopeCastDistance = 0.1f;

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
        rigidbody2d.gravityScale = 0;
        stateTransitions.player = this;
        airState.player = this;
        attackState.player = this;
        deadState.player = this;
        dodgeState.player = this;
        struckState.player = this;
        idleState.player = this;
        moveState.player = this;
        stateMachine = new StateMachine(idleState);

        //once saving/loading is implemented, initialize the grid from the stored position
        levelgrid.InitializeActiveGrid(Vector2Int.zero);
    }

    private void Start()
    {
        StartCoroutine(RunStateMachine());
        rigidbody2d.gravityScale = 1;
    }

    //should really be a discrete state in the state machine, apart from "air"
    public IEnumerator Jump()
    {
        int i = 0;
        while (Input.GetButton("Jump") && i <= maxJumpIterations)
        {
            rigidbody2d.velocity += Vector2.up * jumpForce;
            //rigidbody2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            i++;
            yield return null;
        }
    }

    public Vector2 GetForwardVelocity(float dir)
    {
        if (dir == 0)
            return Vector2.zero;

        raycastController.UpdateRaycastOrigins();
        Vector2 raycastOrigin = dir < 0 ? raycastController.raycastOrigins.bottomLeft : raycastController.raycastOrigins.bottomRight;

        Debug.DrawRay(raycastOrigin, Vector2.right * dir * maxForwardSlopeCastDistance, Color.yellow);
        Debug.DrawRay(raycastOrigin, Vector2.down, Color.red);

        RaycastHit2D hitForward = Physics2D.Raycast(raycastOrigin, Vector2.right * dir, maxForwardSlopeCastDistance, raycastController.collisionMask);
        if (hitForward && !Mathf.Approximately(hitForward.normal.y, 0))
        {
            //Debug.Log("Ascending slope (normal) : " + hitForward.normal);
            return Vector2.Perpendicular(hitForward.normal) * -dir;
        }

        RaycastHit2D hitDown = Physics2D.Raycast(raycastOrigin, Vector2.down, maxDownSlopeCastDistance, raycastController.collisionMask);
        if (hitDown && !Mathf.Approximately(hitDown.normal.y, 1))
        {
            //Debug.Log("Descending slope (normal) : " + hitDown.normal );
            return Vector2.Perpendicular(hitDown.normal) * -dir;
        }

        //Debug.Log("Not on slope");
        return dir * Vector2.right;
    }

    public bool isGrounded()
    {
        raycastController.UpdateRaycastOrigins();

        bool grounded = false;

        Debug.DrawRay(rigidbody2d.position, Vector2.down * maxDownSlopeCastDistance, Color.magenta);
        Debug.DrawRay(rigidbody2d.position - new Vector2(collider.bounds.extents.x, collider.bounds.extents.y), Vector2.down * maxDownSlopeCastDistance, Color.magenta);
        Debug.DrawRay(rigidbody2d.position + new Vector2(collider.bounds.extents.x, -collider.bounds.extents.y), Vector2.down * maxDownSlopeCastDistance, Color.magenta);

        RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position, Vector2.down, maxDownSlopeCastDistance, raycastController.collisionMask);
        RaycastHit2D hitL = Physics2D.Raycast(rigidbody2d.position - new Vector2(collider.bounds.extents.x, collider.bounds.extents.y), Vector2.down, maxDownSlopeCastDistance, raycastController.collisionMask);
        RaycastHit2D hitR = Physics2D.Raycast(rigidbody2d.position + new Vector2(collider.bounds.extents.x, -collider.bounds.extents.y), Vector2.down, maxDownSlopeCastDistance, raycastController.collisionMask);
        if (hit)
        {
            levelgrid.CompareScenePositions(hit.collider.gameObject.scene.buildIndex);
            grounded = true;
        }
        else if(hitL)
        {
            levelgrid.CompareScenePositions(hitL.collider.gameObject.scene.buildIndex);
            grounded = true;
        }
        else if(hitR)
        {
            levelgrid.CompareScenePositions(hitR.collider.gameObject.scene.buildIndex);
            grounded = true;
        }

        return grounded;
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
