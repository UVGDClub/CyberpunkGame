using Defs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Enemy
{
	/// <summary>
	/// The base class of all enemies. 
	/// Holds references to components and data.
	/// Deals with managing the AnimatorController.
	/// TODO -> more stuff like basic logic modules (virtual functions most likely)
	/// </summary>
	public abstract class EnemyBehaviour : MonoBehaviour
	{
		public Rigidbody2D RigidBody { get; private set; }
		public BoxCollider2D Collider { get; private set; }
		public SpriteRenderer Sprite { get; private set; }
		public Animator Animator { get; private set; }

		public EnemyDef Def { get; private set; }

		public bool IsInitialized { get; private set; }

		private Direction facingDir;
		public Direction FacingDirection
		{
			get
			{
				return facingDir;
			}
			set
			{
				//flip the sprite if the direction is not our def's default
				if (value == Direction.Left || value == Direction.Right)
				{
					var isDefaultDir = Def.DefaultFacingDirection == value;
					Sprite.flipX = !isDefaultDir;

				}
				else
				{
					Sprite.flipX = false; //direction unspecified - just use default
				}

				facingDir = value;
			}
		}

		protected GroundingHandler groundHandler;

		public virtual void AssignDef(EnemyDef newDef, Vector2 position = default(Vector2), Direction facingDirection = Direction.Unspecified)
		{
			if (!IsInitialized)
			{
				Initialize(); //Gets executed the first time this function runs (when we are created rather than drawn from the pool)
			}

			groundHandler.Reset();

			Def = newDef;

			transform.position = position.ToV3();
			transform.rotation = Quaternion.identity;

            //(ADVANCED) Force override
            if(!newDef.ForceIgnore)
            {
                Sprite.transform.localScale = new Vector3(Def.Scale, Def.Scale, 1f);

                switch (Def.DefaultAnchorPoint)
                {
                    case SpriteAnchorPoint.BottomCenter: Sprite.transform.localPosition = Vector3.zero; break;
                    case SpriteAnchorPoint.Center: Sprite.transform.localPosition = new Vector3(0f, (Def.Scale * Def.CellSize.y) / (2f * Def.PixelsPerUnit), 0f); break;
                }

                //Set facing direction - use default if not specified
                FacingDirection = (facingDirection == Direction.Unspecified) ? Def.DefaultFacingDirection : facingDirection;

                //Set collider - At this point, sprite is guaranteed to be centered on the x and at the bottom on the y.
                Collider.size = Def.VisibleSize * Def.Scale;
                Collider.offset = new Vector2(0f, Collider.size.y / 2f);
            }			

			//Set animator override
			if (Def.Animations == null)
			{
				Debug.LogError(string.Format("Animator Override Controller not set on '{0}' enemy def!", Def.Name));
			}
			Animator.runtimeAnimatorController = Def.Animations;
		}

		private void Initialize()
		{
			RigidBody = GetComponentInChildren<Rigidbody2D>();
			Collider = GetComponentInChildren<BoxCollider2D>();
			Sprite = GetComponentInChildren<SpriteRenderer>();
			Animator = GetComponentInChildren<Animator>();

			groundHandler = new GroundingHandler(transform, OnBecomeGrounded, OnBecomeAirborne);

			IsInitialized = true;
		}




		public virtual void Update()
		{
			if (!IsInitialized) return;

			///TODO this needs to be skipped if we are jumping, cuz we know we arent grounded in that case.
			///Skipping for one or two frames while jumping will let us get off the ground (if enemies can even jump that is)
			groundHandler.Update(Collider.size, FacingDirection);

			ApplyGravity();
		}

		#region Base Behaviours

		private void Move(Direction direction = Direction.Unspecified)
		{

		}

		private void OnBecomeGrounded()
		{
			RigidBody.velocity = new Vector2(RigidBody.velocity.x, 0f);
		}

		private void OnBecomeAirborne()
		{ 

		}

		protected virtual void ApplyGravity()
		{
			if (!groundHandler.IsGrounded)
			{
				RigidBody.velocity += Physics2D.gravity * Time.deltaTime;
			}
		}

		#endregion

		/// <summary>
		/// Return true if facing left, false otherwise (presumably right facing).
		/// </summary>
		/// <returns></returns>
		public bool IsFacingLeft()
		{
			return FacingDirection == Direction.Left;
		}


		#region Animator Stuff
		public static class AnimParams
		{
			public const string isGrounded = "isGrounded";
			public const string isDead = "isDead";
			public const string jump = "jump";
			public const string takeDamage = "takeDamage";
			public const string moveState = "moveState";
			public const string attackState = "attackState";

			public static bool IsTrigger(string param)
			{
				switch(param)
				{
					case jump:
					case takeDamage:
						return true;
					default:
						return false;
				}
			}
		}

		public void SetAnimParameter(string param, object value)
		{
			AnimUtils.SetParameter(Animator, param, value, AnimParams.IsTrigger(param));
		}

		public T GetAnimParameter<T>(string param)
		{
			return (T)AnimUtils.GetParameter<T>(Animator, param);
		}

		#endregion
	}
}