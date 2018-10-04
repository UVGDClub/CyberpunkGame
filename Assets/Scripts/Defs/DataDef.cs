using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Defs
{
	public abstract class DataDef : ScriptableObject
	{
		public abstract string DefName { get; }
	}
}
