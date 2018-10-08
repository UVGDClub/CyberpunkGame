using CustomTypes;
using Defs;
using Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyManager
{
	private static readonly DefHandler<EnemyDef> defHandler;
	private static readonly Dictionary<string, ObjectPool<GameObject>> enemyPoolDict;

	private static Transform poolContainer;
	private static Transform PoolContainer
	{
		get
		{
			if (poolContainer == null)
				poolContainer = new GameObject("enemy_pool_container").transform;
			return poolContainer;
		}
	}


	static EnemyManager()
	{
		defHandler = new DefHandler<EnemyDef>("Defs/Enemy");
		defHandler.LoadDefs();

		enemyPoolDict = new Dictionary<string, ObjectPool<GameObject>>();
	}


	public static EnemyBehaviour SpawnEnemy(string enemyName, Vector2 atPosition, Direction facingDir = Direction.Unspecified)
	{
		var def = defHandler.GetDef(enemyName);
		var enemyObj = GetPool(def).GetNextObject();

		DebugPool(def);

		var behaviour = enemyObj.GetComponent<EnemyBehaviour>();
		behaviour.AssignDef(def, atPosition, facingDir);

		//TEMP
		behaviour.SetAnimParameter(EnemyBehaviour.AnimParams.isGrounded, true);

		return behaviour;
	}

	public static void DestroyEnemy(GameObject enemyObj)
	{
		if (enemyObj != null)
			DestroyEnemy(enemyObj.GetComponent<EnemyBehaviour>());
	}

	public static void DestroyEnemy(EnemyBehaviour enemy)
	{
		if (enemy != null)
		{
			GetPool(enemy.Def).ReturnObject(enemy.gameObject);
			DebugPool(enemy.Def);
		}
	}
	
	static void DebugPool(EnemyDef def)
	{
		var pool = GetPool(def);
		Debug.Log(string.Format("Name: {0} | Active: {1} | Inactive: {2} | Total: {3} | Max: {4}", def.Name, pool.ActiveItems, pool.AvailableItems, pool.TotalItems, pool.MaxPooledItems));
	}



	#region	Object Pool Functions
	private static ObjectPool<GameObject> GetPool(EnemyDef def)
	{
		if (!enemyPoolDict.ContainsKey(def.Name))
		{
			enemyPoolDict.Add(def.Name, new ObjectPool<GameObject>(() => OP_OnCreate(def), OP_OnGet, OP_OnReturn, 10));
		}
		return enemyPoolDict[def.Name];
	}

	private static GameObject OP_OnCreate(EnemyDef def)
	{
		var obj = Object.Instantiate(def.Prefab);
		obj.name = def.Name;
		return obj;
	}
	
	private static void OP_OnGet(GameObject enemyObj)
	{
		enemyObj.transform.SetParent(null);
		enemyObj.SetActive(true);
	}

	private static void OP_OnReturn(GameObject enemyObj)
	{
		enemyObj.transform.SetParent(PoolContainer);
		enemyObj.SetActive(false);
	}
	#endregion
	
}

[System.Serializable]
public enum EnemySpawnMode
{
    Once,
    OnLoad,
    Timer
}

[System.Serializable]
public struct EnemySpawnInfo
{
    public string name;
    public Vector2 position;
    public Direction facingDir;
    public EnemySpawnMode spawnMode;
    public float respawnTime;
}
