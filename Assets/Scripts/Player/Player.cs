///<summary>
/// Created by Glen McManus September 27, 2018
/// </summary>

using Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //for quick and dirty AudioManager testing
    public GameObject audioPanel;
    public LevelGrid levelgrid;
    public FileDetails fileDetails = null;
    public bool initialized = false;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float jumpForce = 5f;
    public int maxJumpIterations = 5;
    public Direction facing = Direction.Right;

    [Header("Attack")]
    public Vector2 attackBoxSize = Vector2.one;
    public ContactFilter2D attackFilter = new ContactFilter2D();
    public float maxAttackDistance = 1f;
    public LayerMask attackLayerMask;

    [Header("Collision")]
    public BoxCollider2D collider;
    public Rigidbody2D rigidbody2d;
    public float groundDistance = 0.01f;
    public float maxForwardSlopeCastDistance = 0.3f;
    public float maxDownSlopeCastDistance = 0.1f;
    public LayerMask collisionMask;

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
        fileDetails = null;
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

        FindObjectOfType<TileInfo>().levelGrid = levelgrid;

        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        while (fileDetails == null)
            yield return null;


        PlayerSpawnInfo spawn = new PlayerSpawnInfo(Vector2Int.zero, new Vector2(fileDetails.position_x, fileDetails.position_y), Direction.Right);
        Spawn(spawn);

        StartCoroutine(RunStateMachine());
        rigidbody2d.gravityScale = 1;
        initialized = true;
    }

    public void Spawn(PlayerSpawnInfo spawn)
    {
        levelgrid.InitializeActiveGrid(spawn);
        rigidbody2d.position = spawn.position;
        //set facing
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

        facing = dir < 0 ? Direction.Left : Direction.Right;

        Vector2 raycastOrigin = dir < 0 ? new Vector2(-collider.bounds.extents.x, -collider.bounds.extents.y) : new Vector2(collider.bounds.extents.x, -collider.bounds.extents.y);

        Debug.DrawRay(raycastOrigin, Vector2.right * dir * maxForwardSlopeCastDistance, Color.yellow);
        Debug.DrawRay(raycastOrigin, Vector2.down, Color.red);

        RaycastHit2D hitForward = Physics2D.Raycast(raycastOrigin, Vector2.right * dir, maxForwardSlopeCastDistance, collisionMask);
        if (hitForward && !Mathf.Approximately(hitForward.normal.y, 0))
        {
            //Debug.Log("Ascending slope (normal) : " + hitForward.normal);
            return Vector2.Perpendicular(hitForward.normal) * -dir;
        }

        RaycastHit2D hitDown = Physics2D.Raycast(raycastOrigin, Vector2.down, maxDownSlopeCastDistance, collisionMask);
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
        bool grounded = false;

        Debug.DrawRay(rigidbody2d.position + Vector2.down * collider.bounds.extents.y, Vector2.down, Color.magenta);
        Debug.DrawRay(rigidbody2d.position - new Vector2(collider.bounds.extents.x, collider.bounds.extents.y), Vector2.down, Color.magenta);
        Debug.DrawRay(rigidbody2d.position + new Vector2(collider.bounds.extents.x, -collider.bounds.extents.y), Vector2.down, Color.magenta);

        RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.down * collider.bounds.extents.y, Vector2.down, 1, collisionMask);
        RaycastHit2D hitL = Physics2D.Raycast(rigidbody2d.position - new Vector2(collider.bounds.extents.x, collider.bounds.extents.y), Vector2.down, 1, collisionMask);
        RaycastHit2D hitR = Physics2D.Raycast(rigidbody2d.position + new Vector2(collider.bounds.extents.x, -collider.bounds.extents.y), Vector2.down, 1, collisionMask);

        if (hit)
        {
            levelgrid.CompareScenePositions(hit.collider.gameObject.scene.buildIndex);

            if(hit.distance <= groundDistance)
                grounded = true;
        }

        if (hitL)
        {
            if(!hit)
                levelgrid.CompareScenePositions(hitL.collider.gameObject.scene.buildIndex);

            if (hitL.distance <= groundDistance)
                grounded = true;
        }

        if (hitR)
        {
            if(!hit && !hitL)
                levelgrid.CompareScenePositions(hitR.collider.gameObject.scene.buildIndex);

            if (hitR.distance <= groundDistance)
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
                //For quick and dirty testing of audio manager
                if (Input.GetKeyDown(KeyCode.Escape))
                    UI_Manager.instance.MenuTransition();

                yield return null;
            }
        }
    }

    //for testing
    public void RESET_GAME()
    {
        levelgrid.CrossfadeBGM -= FindObjectOfType<AudioManager>().CrossFade;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Init");
    }

}

public struct PlayerSpawnInfo
{
    public Vector2Int gridPosition;
    public Vector2 position;
    public Direction facingDir;

    public PlayerSpawnInfo(Vector2Int gridPosition, Vector2 position, Direction facingDir)
    {
        this.gridPosition = gridPosition;
        this.position = position;
        this.facingDir = facingDir;
    }
}
