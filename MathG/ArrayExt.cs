using System;
using System.Collections.Generic;

namespace Mathlib.Arrays
{
	public static class ArrayExt
	{
		public static string ToString<T>(this T[] array)
		{
			if (array == null)
				throw new NullReferenceException();

			string output = "{ ";
			for (int i = 0; i < array.Length; i++)
			{
				output += array[i].ToString();
				if (i < array.Length - 1)
					output += ", ";
			}
			output += " }";

			return output;
		}

		// returns true if the given array contains the given element
		// linear search
		public static bool Contains<T>(this T[] array, T e)
		{
			if (array == null)
				throw new NullReferenceException();

			foreach (T t in array)
			{
				if (t.Equals(e))
					return true;
			}
			return false;
		}

		public static int Sum(this int[] array)
		{
			int sum = 0;
			for (int i = 0; i < array.Length; i++)
				sum += array[i];
			return sum;
		}

		/// <summary>
		/// Combines the elements of two arrays.
		/// </summary>
		/// <returns></returns>
		public static T[] Union<T>(this T[] A, T[] B)
		{
			if (A == null)
				return B;
			if (B == null)
				return A;

			List<T> c = new List<T>();
			for (int x = 0; x < A.Length; x++)
				c.Add(A[x]);
			for (int y = 0; y < B.Length; y++)
			{
				if (!c.Contains(B[y]))
					c.Add(B[y]);
			}

			return c.ToArray();
		}
		/// <summary>
		/// Returns every element in A that is also in B.
		/// </summary>
		/// <returns></returns>
		// returns elements shared between two arrays
		public static T[] Intersect<T>(this T[] A, T[] B)
		{
			if (A == null || B == null)
				return new T[0];

			List<T> c = new List<T>();
			T[] smaller = A.Length <= B.Length ? A : B;

			for (int i = 0; i < smaller.Length; i++)
				if (A.Contains(smaller[i]) && B.Contains(smaller[i]))
					c.Add(smaller[i]);

			return c.ToArray();
		}
		/// <summary>
		/// Returns every element of A that is not in B.
		/// </summary>
		/// <returns></returns>
		public static T[] Difference<T>(this T[] A, T[] B)
		{
			List<T> diff = new List<T>();
			foreach (T e in A)
			{
				if (!B.Contains(e))
				{
					diff.Add(e);
				}
			}
			return diff.ToArray();
		}

		public static int IndexOf<T>(this T[] array, T e)
		{
			for (int i = 0; i < array.Length; i++)
				if (array[i].Equals(e))
					return i;

			return -1;
		}
	}
}