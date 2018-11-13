using Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Defs
{
	[CreateAssetMenu(fileName = "NewEnemyDef", menuName = "DataDef/EnemyDef")]
	public class EnemyDef : DataDef
	{
		[HelpBox("[Enemy Def]\n\n" +
			"Relevant information about this def. I'll fill it in later.", HelpBoxMessageType.Info)]
		[Space()]

		[Header("General Info")]
		public string Name;
		public int Health = 100;
		public int Damage = 10;
		public float SpeedMultiplier = 1f;

		[Header("Sprite Info")]
		public int PixelsPerUnit = 100;
		public Vector2 CellSize;
		public Vector2 VisibleSize;
		public Direction DefaultFacingDirection = Direction.Left;
		public SpriteAnchorPoint DefaultAnchorPoint = SpriteAnchorPoint.Center;
		public float Scale = 1f;
        
        [Tooltip("Warning!! Don't use this unless you know what you're doing.")]
        public bool ForceIgnore = false;

		[Header("References")]
		public AnimatorOverrideController Animations;
		public GameObject Prefab;

		public override string DefName
		{
			get { return Name; }
		}

		


		/// <summary>
		/// Called when values are updated. Ensures valid ranges for parameters.
		/// </summary>
		public void OnValidate()
		{
			if (string.IsNullOrEmpty(Name))
				Name = name;

			Health = Mathf.Clamp(Health, 1, int.MaxValue);
			Damage = Mathf.Clamp(Damage, 0, int.MaxValue);
			SpeedMultiplier = Mathf.Clamp(SpeedMultiplier, 0, float.MaxValue);

			PixelsPerUnit = Mathf.Clamp(PixelsPerUnit, 1, int.MaxValue);

			if (CellSize.x < 0) CellSize = new Vector2(0f, CellSize.y);
			if (CellSize.y < 0) CellSize = new Vector2(CellSize.x, 0f);

			if (VisibleSize.x < 0) VisibleSize = new Vector2(0f, VisibleSize.y);
			if (VisibleSize.y < 0) VisibleSize = new Vector2(VisibleSize.x, 0f);

			Scale = Mathf.Clamp(Scale, 0.01f, float.MaxValue);
		}

		/// <summary>
		/// Called when we create a new instance of this def. Use it to initialize default values.
		/// </summary>
		public void Awake()
		{
			OnValidate();
		}
	}
}