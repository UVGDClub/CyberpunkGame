using UnityEngine;

namespace Utils
{
	public static class AnimUtils
	{
		public static object GetParameter<T>(Animator anim, string param)
		{
			if (anim == null)
			{
				Debug.LogError("Animator null! Could not get parameter '" + param + "'.");
			}
			else if (typeof(T) == typeof(float))
			{
				return anim.GetFloat(param);
			}
			else if (typeof(T) == typeof(int))
			{
				return anim.GetInteger(param);
			}
			else if (typeof(T) == typeof(bool))
			{
				return anim.GetBool(param);
			}
			else
			{
				Debug.LogError("Invalid type requested!");
			}

			return default(T);
		}

		public static void SetParameter(Animator anim, string param, object value, bool isTrigger = false)
		{
			if (anim == null)
			{
				Debug.LogError("Animator null! Could not set parameter '" + param + "'.");
				return;
			}

			if (value is float)
			{
				anim.SetFloat(param, (float)value);
			}
			else if (value is int)
			{
				anim.SetInteger(param, (int)value);
			}
			else if (value is bool)
			{
				bool val = (bool)value;
				if (isTrigger)
				{
					if (val) anim.SetTrigger(param);
					else anim.ResetTrigger(param);
				}
				else
				{
					anim.SetBool(param, val);
				}
			}
			else
			{
				Debug.LogError("Invalid type provided!");
			}
		}
	}
}