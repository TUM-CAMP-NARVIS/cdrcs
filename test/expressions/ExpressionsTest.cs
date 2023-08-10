using System.Collections.Generic;
using System;
using Cdrcs.Tag;
using Cdrcs;
using System.Runtime.CompilerServices;

namespace ExpressionsTest
{
	using DateTime = Int64;
    using reference = Cdrcs.Tag.blob;

	[Schema]
	public class Base
	{
		[Id(0)]
		List<List<UInt64>> vvb { get; set; }
	}

	[Schema]
	public interface Nested
	{
		[Id(0)]
		double _double { get; set; }
	}

/*	[Schema]
	public interface Generic<T>
{
		[Id(0)]
		T value { get; set; }
	}

	[Schema]
	public interface GenericNothing<T>
{
		[Id(0)]
		T value { get; set; }
	}
*/


[Schema]
	public class Example : Base
	{
		[Id(0)]
		bool _bool { get; set; }

		[Id(2)]
		string _str { get; set; }

		[Id(14)]
		sbyte _int8 { get; set; }

		[Id(17), @RequiredAttribute]
		Int64 _int64 { get; set; }

		[Id(12)]
		UInt32 _uint32 { get; set; }

		[Id(18)]
		double _double { get; set; }

		[Id(20)]
		Cdrcs.GUID guid { get; set; }

		[Id(30)]
		List<Int32> _int32Vector { get; set; }

		[Id(40)]
		List<Nested> _nestedVector { get; set; }

		[Id(50)]
		blob b { get; set; }

		[Id(51)]
		List<blob> _blobList { get; set; }

		[Id(52)]
		List<blob> _blobVector { get; set; }

		[Id(53)]
		Dictionary<Int32, blob> _blobMap { get; set; }

		[Id(54)]
		nullable<blob> _blobNullable { get; set; }

		[Id(60)]
		Dictionary<Int32, double> _map { get; set; }

		[Id(70)]
		decimal _decimal { get; set; }

		[Id(71)]
		List<decimal> _decList { get; set; }

		[Id(72)]
		List<decimal> _decVector { get; set; }

		[Id(73)]
		Dictionary<Int32, decimal> _decMap { get; set; }

		[Id(74)]
		nullable<decimal> _decNullable { get; set; }

		[Id(75), @RequiredAttribute]
		decimal _decimal_req { get; set; }

		[Id(80)]
		reference _reference { get; set; }

		[Id(81)]
		List<reference> _refList { get; set; }

		[Id(82)]
		List<reference> _refVector { get; set; }

		[Id(83)]
		Dictionary<Int32, reference> _refMap { get; set; }

		[Id(84)]
		nullable<reference> _refNullable { get; set; }

/*		[Id(85)]
		Generic<DateTime> _dt { get; set; }

		[Id(86)]
		GenericNothing<DateTime> _dt2 { get; set; }
*/
	};

}