using System.Collections.Generic;
using UnityEngine;

namespace Defs
{
	public class DefHandler<Tdef> where Tdef : DataDef
	{
		private readonly Dictionary<string, Tdef> library;
		private readonly string path;

		public bool IsLoaded { get; private set; }

		public DefHandler(string path)
		{
			this.path = path;
			library = new Dictionary<string, Tdef>();
		}

		public Tdef GetDef(string defName)
		{
			if (!library.ContainsKey(defName))
			{
				if (!IsLoaded) Debug.LogError(string.Format("Defs not loaded! Call LoadDefs before trying to access them!"));
				else Debug.LogError(string.Format("Def '{0}' not found. Check the spelling or make sure that such a def exists in the Resources/Defs/Enemy directory.", defName));
				return null;
			}

			return library[defName];
		}

		public void LoadDefs()
		{
			var defs = Resources.LoadAll<Tdef>(path);
			for (int i = 0; i < defs.Length; i++)
			{
				var def = defs[i];
				if (library.ContainsKey(def.DefName))
				{
					Debug.LogError(string.Format("Error: Def with name '{0}' already exists. Duplicate names are not allowed.\nThis Object: {1}\nOther Object: {2}", def.DefName, def.name, library[def.DefName].name));
					continue;
				}

				library.Add(def.DefName, def);
			}

			IsLoaded = true;
		}
	}
}