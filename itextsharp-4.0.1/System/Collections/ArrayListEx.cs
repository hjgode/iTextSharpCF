using System;
using System.Collections;

namespace System.Collections
{
	/// <summary>
	/// .NET CF compatibility class
	/// </summary>
	public class ArrayListEx
	{
		public static ArrayList Repeat( object obj, int iCount )
		{
			object[] arr = new object[iCount];
			for ( int i = 0; i < iCount; i++ )
			{
				arr[i] = obj;
			}
			return new ArrayList( arr );
		}

		public static ArrayList GetRange( ArrayList list, int index, int count )
		{
			object[] arr = new object[ count ];
			list.CopyTo( index, arr, 0, count );
			return new ArrayList( arr );
		}
	}
}