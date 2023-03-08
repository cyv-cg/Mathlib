using System;
using System.Collections.Generic;

namespace Mathlib
{
	public abstract class PropertyHolder
	{
		private readonly Dictionary<string, object> properties = new Dictionary<string, object>();
		internal readonly Dictionary<string, Type> propTypes = new Dictionary<string, Type>();

		public bool HasProp(string key)
		{
			return properties.ContainsKey(key);
		}

		public void RemoveProp(string key)
		{
			if (!HasProp(key))
				throw new ArgumentNullException($"Key '{key}' not present in dictionary.");

			properties.Remove(key);
			propTypes.Remove(key);
		}

		public virtual void SetProp<T>(string key, T value)
		{
			if (!HasProp(key))
			{
				properties.Add(key, value);
				propTypes.Add(key, typeof(T));
			}
			else
			{
				properties[key] = value;
				propTypes[key] = typeof(T);
			}
		}
		public virtual T GetProp<T>(string key)
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

		public static TItem ItemWithMinProp<TValue, TItem>(List<TItem> items, string property) where TValue : IComparable where TItem : PropertyHolder
		{
			return ItemWithMinProp<TValue, TItem>(items.ToArray(), property);
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

		public static TItem ItemWithMaxProp<TValue, TItem>(List<TItem> items, string property) where TValue : IComparable where TItem : PropertyHolder
		{
			return ItemWithMaxProp<TValue, TItem>(items.ToArray(), property);
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