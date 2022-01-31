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

		public static TItem ItemWithMinProp<TValue, TItem>(TItem[] items, string property) where TValue : IComparable where TItem : PropertyHolder
		{
			try
			{
				TItem itemWithMinVal = items[0];
				for (int i = 1; i < items.Length; i++)
				{
					if (items[i].GetProp<TValue>(property).CompareTo(itemWithMinVal.GetProp<TValue>(property)) < 0)
						itemWithMinVal = items[i];
				}
				return itemWithMinVal;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
		}
		public static TItem ItemWithMaxProp<TValue, TItem>(TItem[] items, string property) where TValue : IComparable where TItem : PropertyHolder
		{
			try
			{
				TItem itemWithMaxVal = items[0];
				for (int i = 1; i < items.Length; i++)
				{
					if (items[i].GetProp<TValue>(property).CompareTo(itemWithMaxVal.GetProp<TValue>(property)) > 0)
						itemWithMaxVal = items[i];
				}
				return itemWithMaxVal;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
		}
	}
}