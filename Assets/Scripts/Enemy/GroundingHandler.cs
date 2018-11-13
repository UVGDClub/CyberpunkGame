#define DEBUG_MODE

using System;
using UnityEngine;
using Utils;

/// <summary>
/// Tracks the grounded state of the object.
/// </summary>
public class GroundingHandler
{
	private const float MAX_SLOPE_RADIANS = 30f * Mathf.Deg2Rad;

	public bool IsGrounded { get; private set; }
	public bool WasGroundedLastFrame { get; private set; }
	public Vector2 GroundSlope { get; private set; }
    public Vector2 LeftSlope { get; private set; }
    public Vector2 RightSlope { get; private set; }


    private readonly Action onBecomeGrounded;
	private readonly Action onBecomeAirborne;

	private readonly Transform transform;

	private readonly bool followMovingObjects;

	public GroundingHandler(Transform transform, Action onBecomeGrounded = null, Action onBecomeAirborne = null, bool followMovingObjects = true)
	{
		this.transform = transform;

		this.onBecomeGrounded = onBecomeGrounded;
		this.onBecomeAirborne = onBecomeAirborne;

		this.followMovingObjects = followMovingObjects;
	}

	public void Reset()
	{
		IsGrounded = false;
		WasGroundedLastFrame = false;
		GroundSlope = Vector2.zero;
	}

	public void Update(Vector2 dimensions, Direction facingDir)
	{
		const float EPSILON = 0.1f; //the extra amount to cast -> higher values make snapping more noticeable but provide more reliability and better adhesion to moving platforms.

		var position = transform.position.ToV2();

		float heightOffset = dimensions.y / 3; //gives our cast more room to detect the ground (guards cases where we overshoot cuz we are falling quickly)
		float castLength = dimensions.x * Mathf.Tan(MAX_SLOPE_RADIANS) + heightOffset + EPSILON; //detect slopes of up to the maxDetectionAngle
		var xOffset = dimensions.x / 2f;

		var centerPoint = position;
		var leftPoint = new Vector2(position.x - xOffset, position.y);
		var rightPoint = new Vector2(position.x + xOffset, position.y);

		var centerRay = new Ray2D(position + Vector2.up * heightOffset, Vector2.down);
		var leftRay = new Ray2D(leftPoint + Vector2.up * heightOffset, Vector2.down);
		var rightRay = new Ray2D(rightPoint + Vector2.up * heightOffset, Vector2.down);

		CastInfo centerCast = RaycastUtils.Raycast(centerRay, castLength, GameUtils.Layers.GroundMask);
		CastInfo leftCast = RaycastUtils.Raycast(leftRay, castLength, GameUtils.Layers.GroundMask);
		CastInfo rightCast = RaycastUtils.Raycast(rightRay, castLength, GameUtils.Layers.GroundMask);

		CastInfo castInfo = centerCast.didHit ? centerCast : (leftCast.didHit ? leftCast : rightCast);
		SetGroundedInfo(castInfo, heightOffset + EPSILON, facingDir);

		CheckForStateChange();

        //adding forward slope handling
        Vector2 forward = facingDir == Direction.Left ? Vector2.left : Vector2.right;
        var forwardRay = new Ray2D(rightPoint + Vector2.up * heightOffset, forward);
        CastInfo forwardCast = RaycastUtils.Raycast(forwardRay, castLength, GameUtils.Layers.GroundMask);
        SetForwardSlope(forwardCast, facingDir);

        if (followMovingObjects)
		{
			///Cover the case of moving platforms!
			if (IsGrounded && castInfo.didHit)
			{
				position.y = castInfo.hitInfo.point.y;
				transform.position = position.ToV3();
			}
			///TODO -> still need handling for horizontal movement of platforms; that is a bit more 
			///		involved though so ill leave it for now until some level design decisions are sorted
		}

#if DEBUG_MODE
		///bottom positions on collider
		DebugUtils.DrawSquare(centerPoint, 0.035f, Color.yellow);
		DebugUtils.DrawSquare(leftPoint, 0.035f, Color.yellow);
		DebugUtils.DrawSquare(rightPoint, 0.035f, Color.yellow);

		if (centerCast.didHit) DebugUtils.DrawSquare(centerCast.hitInfo.point, 0.05f, Color.blue);
		if (leftCast.didHit && rightCast.didHit) DebugUtils.DrawConnection(leftCast.hitInfo.point, rightCast.hitInfo.point, Color.blue, Color.blue, 0.05f);
		else if (leftCast.didHit) DebugUtils.DrawSquare(leftCast.hitInfo.point, 0.05f, Color.blue);
		else if (rightCast.didHit) DebugUtils.DrawSquare(rightCast.hitInfo.point, 0.05f, Color.blue);

		DebugUtils.DrawArrow(centerRay.origin, centerRay.GetPoint(castLength), Color.red, 0.1f);
		DebugUtils.DrawArrow(leftRay.origin, leftRay.GetPoint(castLength), Color.red, 0.1f);
		DebugUtils.DrawArrow(rightRay.origin, rightRay.GetPoint(castLength), Color.red, 0.1f);

		DebugUtils.DrawArrow(position, position + GroundSlope * xOffset * 1.5f, Color.green, 0.1f);

		if (castInfo.didHit) DebugUtils.DrawSquare(castInfo.hitInfo.point, 0.1f, Color.magenta);
#endif
	}
	
	private void SetGroundedInfo(CastInfo castInfo, float heightOffset, Direction facingDir)
	{
		WasGroundedLastFrame = IsGrounded;

		if (!castInfo.didHit)
		{
			IsGrounded = false;
			GroundSlope = Vector2.zero;
		}
		else
		{
			IsGrounded = (castInfo.hitInfo.distance - heightOffset) <= 0f;
			GroundSlope = castInfo.hitInfo.normal.GetNormal();
			var facing = facingDir == Direction.Left ? Vector2.left : Vector2.right;
			if (Vector2.Dot(GroundSlope, facing) < 0f) GroundSlope = -GroundSlope;
		}
	}

    private void SetForwardSlope(CastInfo castInfo, Direction facingDir)
    {
        if(!castInfo.didHit)
        {
            LeftSlope = Vector2.zero;
            RightSlope = Vector2.zero;
        }
        else
        {
            if (facingDir == Direction.Left)
            {
                RightSlope = Vector2.zero;
                LeftSlope = castInfo.hitInfo.normal.GetNormal();
                if (Vector2.Dot(LeftSlope, Vector2.left) < 0f) LeftSlope = -LeftSlope;
            }
            else if (facingDir == Direction.Right)
            {
                LeftSlope = Vector2.zero;
                RightSlope = castInfo.hitInfo.normal.GetNormal();
                if (Vector2.Dot(RightSlope, Vector2.right) < 0f) RightSlope = -RightSlope;
            }
        }

        //Debug.Log("Left slope: " + LeftSlope);
        //Debug.Log("Right slope: " + RightSlope);
    }

	private void CheckForStateChange()
	{
		///Just became grounded
		if (IsGrounded && !WasGroundedLastFrame)
		{
			if (onBecomeGrounded != null)
				onBecomeGrounded();
		}
		///Just became airborne
		else if (!IsGrounded && WasGroundedLastFrame)
		{
			if (onBecomeAirborne != null)
				onBecomeAirborne();
		}
	}
	
}
