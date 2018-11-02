using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player : MonoBehaviour {

    public Rigidbody2D rigidbody2d;
    public APlayerState currentState;

    public LayerMask layerMask;

    public float castDist = 0.05f;
    public bool grounded = false;
    public bool left = false;
    public bool right = false;
    public bool top = false;

    public int numCastsVertical = 8;
    public int numCastsHorizontal = 4;

    private BoxCollider2D boxCollider;

	// Use this for initialization
	void Start () {
        rigidbody2d = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void Update() {
        currentState.InputCheck(this);
    }

    RaycastHit2D[] hits = new RaycastHit2D[1];

    Vector2 start = new Vector2();
    Vector2 end = new Vector2();

    // Update is called once per frame
    void FixedUpdate () {

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
        grounded = ParallelCast(hits, start, end, 1, numCastsHorizontal);

        
        
        currentState.Execute(this);
	}


    bool ParallelCast(RaycastHit2D[] hits, Vector2 startPoint, Vector2 endPoint, int directionMultipler, int numCasts) {

        Vector2 direction = Vector2.Perpendicular((startPoint - endPoint).normalized) * directionMultipler;
        Debug.DrawLine(start, end, Color.blue);

        for (int i = 0; i <= numCasts; i++) {
            Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (float)numCasts);

            Debug.DrawRay(origin, direction);
            if (Physics2D.RaycastNonAlloc(origin, direction, hits, castDist, layerMask) > 0)
                return true;
        }
        return false;
    }

}
