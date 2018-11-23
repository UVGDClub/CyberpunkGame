using Audio;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player : MonoBehaviour {

    //for quick and dirty AudioManager testing
    public GameObject audioPanel;
    public LevelGrid levelgrid;
    public FileDetails fileDetails = null;
    public bool initialized = false;

    public Direction facing = Direction.Right;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Attack")]
    public Vector2 attackBoxSize = new Vector2(0.16f, 0.32f);
    public ContactFilter2D attackFilter = new ContactFilter2D();
    public float maxAttackDistance = 1f;
    public LayerMask attackLayerMask;

    [Header("Collision")]
    public float maxForwardSlopeCastDistance = 0.3f;
    public float maxDownSlopeCastDistance = 0.1f;
    public LayerMask collisionMask;

    public Rigidbody2D rigidbody2d;
    public APlayerState currentState;

    public float castDist = 0.05f;
    public RaycastHit2D bottomHit;
    public RaycastHit2D left;
    public RaycastHit2D right;
    public RaycastHit2D top;

    public int numCastsVertical = 8;
    public int numCastsHorizontal = 4;

    [Header("States")]
    public bool debug = false;
    public bool canDash = true;

    private BoxCollider2D boxCollider;


    private void Awake() {
        fileDetails = null;
        rigidbody2d.gravityScale = 0;

        FindObjectOfType<TileInfo>().levelGrid = levelgrid;

        currentState.OnEnter(this);
    }

    // Use this for initialization
    void Start () {
        rigidbody2d = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }


    public void Update() {
        currentState.InputCheck(this);

        if (Input.GetKeyDown(KeyCode.Escape))
            UI_Manager.instance.MenuTransition();
    }

    RaycastHit2D[] hits = new RaycastHit2D[1];

    Vector2 start = new Vector2();
    Vector2 end = new Vector2();

    // Update is called once per frame
    void FixedUpdate () {
        if (facing == Direction.Left && spriteRenderer.flipX == false)
            spriteRenderer.flipX = true;
        else if (facing == Direction.Right && spriteRenderer.flipX == true)
            spriteRenderer.flipX = false;

        boxCollider.bounds.GetTopLeft(ref start);
        boxCollider.bounds.GetBottomLeft(ref end);
        left = ParallelCast(hits, start, end, 1, numCastsVertical);

        boxCollider.bounds.GetTopRight(ref start);
        boxCollider.bounds.GetBottomRight(ref end);
        right = ParallelCast(hits, start, end, -1, numCastsVertical);
        
        boxCollider.bounds.GetTopLeft(ref start);
        boxCollider.bounds.GetTopRight(ref end);
        top = ParallelCast(hits, start, end, -1, numCastsHorizontal);

        boxCollider.bounds.GetBottomLeft(ref start);
        boxCollider.bounds.GetBottomRight(ref end);
        bottomHit = ParallelCast(hits, start, end, 1, numCastsHorizontal);
        
        if(bottomHit) {
            levelgrid.CompareScenePositions(bottomHit.collider.gameObject.scene.buildIndex);
            canDash = true;
        }

        currentState.Execute(this);
        if (debug)
            GetComponent<MeshRenderer>().material.color = currentState.DebugColor;

        if(rigidbody2d.velocity.x != 0) {
            facing = rigidbody2d.velocity.x > 0 ? Direction.Right : Direction.Left;
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            animator.SetBool("Attack", true);
            Vector2 direction = facing == Direction.Left ? Vector2.left : Vector2.right;

            Debug.DrawRay(rigidbody2d.position, direction * maxAttackDistance, Color.red);

            RaycastHit2D[] hits = Physics2D.BoxCastAll(rigidbody2d.position, attackBoxSize, 0, direction, maxAttackDistance, attackLayerMask);

            foreach (RaycastHit2D hit in hits)
            {
                //maintain a list of objects that can be attacked in a scriptable object?
                //using:
                //hit.collider.GetInstanceID()
                //and a scriptable object?
                BreakableTileInstance bti = hit.collider.GetComponent<BreakableTileInstance>();
                if (bti == null || bti.spriteRenderer.sprite != bti.tileRef.sprites[0])
                    continue;

                bti.StartCoroutine(bti.BreakTile());
            }
        }
	}


    RaycastHit2D ParallelCast( RaycastHit2D[] hits, Vector2 startPoint, Vector2 endPoint, int directionMultipler, int numCasts ) {

        Vector2 direction = Vector2.Perpendicular((startPoint - endPoint).normalized) * directionMultipler;
        Debug.DrawLine(start, end, Color.blue);

        RaycastHit2D lastRay = default(RaycastHit2D);

        for (int i = 0; i <= numCasts; i++) {
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (float)numCasts);

            Debug.DrawRay(origin, direction);
            if (Physics2D.RaycastNonAlloc(origin, direction, hits, castDist, collisionMask) > 0)
                lastRay = hits[0];

        }
        return lastRay;
    }

    public void TransferToState( APlayerState state ) {
       // Debug.Log("Exiting: " + currentState);
       // Debug.Log("Entering: " + state);
        currentState.OnExit(this);
        currentState = state;
        currentState.OnEnter(this);
    }

    public void Spawn(PlayerSpawnInfo spawn)
    {
        levelgrid.InitializeActiveGrid(spawn);
        rigidbody2d.position = spawn.position;
        //set facing
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
