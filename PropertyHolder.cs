using System;
using System.Collections.Generic;

namespace Mathlib
{
	public abstract class PropertyHolder
	{
		private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

		public bool HasProp(string key)
		{
			return properties.ContainsKey(key);
		}

		public void SetProp<T>(string key, T value)
		{
			if (!HasProp(key))
				properties.Add(key, value);
			else
				properties[key] = value;
		}
		public T GetProp<T>(string key)
		{
			try
			{
				return (T)properties[key];
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return default;
			}
		}
	}
}