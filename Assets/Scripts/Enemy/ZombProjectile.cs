using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombProjectile : MonoBehaviour {

    public GameObject fizzle;//remove this later, I just like it for now

    private float speed;
    private int damage;
    private bool goRight;
    private bool useBlood;
    private GameObject blood;

    private Vector2 rightVelo;
    private Vector2 leftVelo;


    private void SetVelocity () {
        /*
         * sets the initial velocity
         */
        if (goRight)
        {
            this.GetComponent<Rigidbody2D>().AddForce(rightVelo);
        }
        else
        {
            this.GetComponent<Rigidbody2D>().AddForce(leftVelo);
        }
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    /*
     * on a collision with the ground or the player this enemy dies
     */
    {
        string collisionTag = collision.gameObject.tag;

        if (collisionTag == "Enemy")
        {
            return;
        }
        if (collisionTag == "Player")
        {
            DamagePlayer(collision);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            DestroyProjectile();
            Instantiate(fizzle, transform.position, transform.rotation);//remove later
        }
    }

    private void DamagePlayer(Collision2D theHit)
    /*
     * damages player and destroys bullet, doing damage will have to be
     * completed once player script is done
     */
    {
        if (useBlood)
        {
            Instantiate(blood, theHit.otherCollider.transform.position, transform.rotation);
        }
        //theHit.otherCollider.gameObject.GetComponent<Player>().addDamage(damage);
        DestroyProjectile();
    }

    public void SetVars(float speed, int damage, bool goRight,
        float horizBase, float vertBase, bool useBlood, GameObject blood = null)
    /*
     * sets variables passed from whoever created the object with this script attached
     * This should be done once at the moment it is instantiated so this will also
     * cause the object to have an appropriote initial velocity and starts death coundown
     */
    {
        this.speed = speed;
        this.damage = damage;
        this.goRight = goRight;
        this.useBlood = useBlood;
        this.blood = blood;
        rightVelo = new Vector2(horizBase*speed, vertBase*speed);
        leftVelo = new Vector2(-horizBase*speed, vertBase*speed);


        SetVelocity();
    }

    public void DestroyProjectile()
    {
        Destroy(this.gameObject);
    }
}
