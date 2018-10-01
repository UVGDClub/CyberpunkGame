using Defs;
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
	[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
	public abstract class EnemyBase : MonoBehaviour
	{
		public Rigidbody2D RigidBody { get; protected set; }
		public Animator Animator { get; protected set; }

		public EnemyDef Def;

		public virtual void Start()
		{
			RigidBody = GetComponent<Rigidbody2D>();
			Animator = GetComponent<Animator>();
		}


		#region Animator Stuff
		protected static class AnimParams
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

		protected void SetAnimParameter(string param, object value)
		{
			AnimUtils.SetParameter(Animator, param, value, AnimParams.IsTrigger(param));
		}

		protected T GetAnimParameter<T>(string param)
		{
			return (T)AnimUtils.GetParameter<T>(Animator, param);
		}

		#endregion
	}
}