using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy;

public class Boss_BigBlue : EnemyBehaviour
{
    Player player;

    [Header("Movement")]
    public float moveSpeed = 1f;
    public Vector2 offsetToPlayer;

    [Header("Attacks")]
    public bool attacking = false;

    [Header("Slam Attack")]
    public int slamDamage = 4;
    public Vector2 maxSlamDistance;

    [Header("Uppercut")]
    public int uppercutDamage = 3;
    public Vector2 minUppercutDistance;
    public Vector2 maxUppercutDistance;
    public float dashSpeed = 4f;
    public float dashEndThreshold = 0.16f;

    [Header("Jab")]
    public int jabDamage = 1;
    public Vector2 maxJabDistance;

    private void Start()
    {
        //yuck -- super hackish :(
        //probably we need a static global reference object
        player = FindObjectOfType<Player>();
    }

    public override void Update()
    {
        base.Update();

        if (player == null)
            return;

        if (groundHandler.IsGrounded)
            RigidBody.gravityScale = 0;
        else
            RigidBody.gravityScale = 1;

        offsetToPlayer = player.rigidbody2d.position - RigidBody.position;
        if (offsetToPlayer.x > 0)
            FacingDirection = Direction.Right;
        else if (offsetToPlayer.x < 0)
            FacingDirection = Direction.Left;

        if (attacking)
            if (Animator.GetInteger("attackState") == 0)
                attacking = false;
            else
                return;

        //try to attack, move closer if not in range
        //possibly try to make more room to uppercut
        switch(ChooseAttack())
        {           
            case 0: //reposition to attack
                Reposition();
                break;
            case 1: //jab
                attacking = true;
                RigidBody.velocity = Vector2.zero;
                Animator.SetInteger("attackState", 1);
                break;
            case 2: //slam
                attacking = true;
                RigidBody.velocity = Vector2.zero;
                Animator.SetInteger("attackState", 2);
                break;
            case 3: //uppercut
                attacking = true;
                StartCoroutine(Uppercut());
                break;
        }
    }

    #region Movement

    public void Reposition()
    {
        //try to backup to uppercut if we're too close
        if(Mathf.Abs(offsetToPlayer.x) > minUppercutDistance.x)
        {
            if (offsetToPlayer.x > 0)
            {
                if (groundHandler.LeftSlope != Vector2.zero)
                    RigidBody.velocity = groundHandler.LeftSlope.normalized * -moveSpeed;
                else if (groundHandler.RightSlope != Vector2.zero)
                    RigidBody.velocity = groundHandler.RightSlope.normalized * -moveSpeed;
                else
                    RigidBody.velocity = groundHandler.GroundSlope.normalized * -moveSpeed;
            }

            else if (offsetToPlayer.x < 0)
            {
                if (groundHandler.LeftSlope != Vector2.zero)
                    RigidBody.velocity = groundHandler.LeftSlope.normalized * moveSpeed;
                else if (groundHandler.RightSlope != Vector2.zero)
                    RigidBody.velocity = groundHandler.RightSlope.normalized * moveSpeed;
                else
                    RigidBody.velocity = groundHandler.GroundSlope.normalized * moveSpeed;
            }
            return;
        }

        if (offsetToPlayer.x > 0)
        {
            if (groundHandler.LeftSlope != Vector2.zero)
                RigidBody.velocity = groundHandler.LeftSlope.normalized * moveSpeed;
            else if (groundHandler.RightSlope != Vector2.zero)
                RigidBody.velocity = groundHandler.RightSlope.normalized * moveSpeed;
            else
                RigidBody.velocity = groundHandler.GroundSlope.normalized * moveSpeed;
        }
        else if(offsetToPlayer.x < 0)
        {
            if (groundHandler.LeftSlope != Vector2.zero)
                RigidBody.velocity = groundHandler.LeftSlope.normalized * -moveSpeed;
            else if (groundHandler.RightSlope != Vector2.zero)
                RigidBody.velocity = groundHandler.RightSlope.normalized * -moveSpeed;
            else
                RigidBody.velocity = groundHandler.GroundSlope.normalized * -moveSpeed;
        }

    }

    #endregion

    #region Attacking

    int ChooseAttack()
    {
        if (attacking)
            Debug.Log("What the fuck how are we attacking?");

        if (UppercutCheck())
            return 3;

        if (SlamCheck())
            return 2;

        if (JabCheck())
            return 1;

        return 0;
    }

    bool SlamCheck()
    {
        if (Mathf.Abs(offsetToPlayer.x) <= maxSlamDistance.x && Mathf.Abs(offsetToPlayer.y) <= maxSlamDistance.y)
            return true;

        return false;
    }

    bool UppercutCheck()
    {
        if (Mathf.Abs(offsetToPlayer.x) <= maxUppercutDistance.x && Mathf.Abs(offsetToPlayer.y) <= maxUppercutDistance.y
            && Mathf.Abs(offsetToPlayer.x) >= minUppercutDistance.x && Mathf.Abs(offsetToPlayer.y) >= minUppercutDistance.y)
            return true;

        return false;
    }

    IEnumerator Uppercut()
    {
        Animator.SetInteger("attackState", 3);

        while (Animator.GetBool("dashingIn") == false)
            yield return null;


        while (Animator.GetBool("dashingIn") == true && Mathf.Abs(offsetToPlayer.x) > dashEndThreshold)
        {
            if(groundHandler.LeftSlope != Vector2.zero)
                RigidBody.velocity = groundHandler.LeftSlope.normalized * dashSpeed;
            else if(groundHandler.RightSlope != Vector2.zero)
                RigidBody.velocity = groundHandler.RightSlope.normalized * dashSpeed;
            else
                RigidBody.velocity = groundHandler.GroundSlope.normalized * dashSpeed;

            Debug.Log(RigidBody.velocity);
            yield return null;
        }

        RigidBody.velocity = Vector2.zero;
    }

    bool JabCheck()
    {
        if(Mathf.Abs(offsetToPlayer.x) <= maxJabDistance.x && Mathf.Abs(offsetToPlayer.y) <= maxJabDistance.y)
            return true;

        return false;
    }
    #endregion
}
