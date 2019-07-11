using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DesktopFolders.StronglyTypedMathConverter
{
	internal class MathConverter : IMultiValueConverter, IValueConverter
	{
		public static List<Tuple<DateTime, string, Exception>> ErrorLog = new List<Tuple<DateTime, string, Exception>>();

		public object Default { get; set; } = null;

		public MathConverter() { }

		// -------- IValueConverter --------

		private object TryReturnDefault(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//	if (Default == null) {
			//		ErrorLog.Add(new Tuple<DateTime, string>(
			//			DateTime.Now,
			//			  "Tried to return the Default value, but it was null, so returned null. "
			//			+ "(value = \"" + value
			//			+ "\", targetType = \"" + targetType.FullName
			//			+ "\", parameter = \"" + parameter
			//			+ "\", culture = \"" + culture
			//			+ "\")"
			//		));
			//		return null;
			//	} else {
			//		return Default;
			//	}
			return TryReturnDefault(new object[] { value }, targetType, parameter, culture);
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//	try {
			//		if (parameter == null) {
			//			return TryReturnDefault(value, targetType, parameter, culture);
			//		} else {
			//			MathNodeBase Expression = (MathNodeBase)parameter;
			//			if (Expression == null) {
			//				return TryReturnDefault(value, targetType, parameter, culture);
			//			} else {
			//				Number evaled = Expression.Evaluate(new MathBindingValueCollection(new object[] { value }));
			//				if (evaled == null) {
			//					return TryReturnDefault(value, targetType, parameter, culture);
			//				} else {
			//					return evaled.GetValue();
			//				}
			//			}
			//		}
			//	} catch (Exception e) {
			//		ErrorLog.Add(new Tuple<DateTime, string>(
			//			DateTime.Now,
			//			  "Encountered an exception while evaluating the MathNode, so (tried to) return the Default value. "
			//			+ "(value = \"" + value
			//			+ "\", targetType = \"" + targetType.FullName
			//			+ "\", parameter = \"" + parameter
			//			+ "\", culture = \"" + culture
			//			+ "\"). "
			//			+ "Exception: " + e.ToString()
			//		));
			//		return TryReturnDefault(value, targetType, parameter, culture);
			//	}
			return Convert(new object[] { value }, targetType, parameter, culture);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NonDefaultableException(new InvalidOperationException("Math bindings cannot be converted backwards."));
		}

		// -------- IMultiValueConverter --------

		private object TryReturnDefault(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (Default == null) {
				ErrorLog.Add(new Tuple<DateTime, string, Exception>(
					DateTime.Now,
					  "Tried to return the Default value, but it was null, so returned null. "
					+ "(values = \"" + values
					+ "\", targetType = \"" + targetType.FullName
					+ "\", parameter = \"" + parameter
					+ "\", culture = \"" + culture
					+ "\")",
					null
				));
				return null;
			} else {
				return Default;
			}
		}

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				if (parameter == null) {
					return TryReturnDefault(values, targetType, parameter, culture);
				} else {
					MathNodeBase Expression = (MathNodeBase)parameter;
					if (Expression == null) {
						return TryReturnDefault(values, targetType, parameter, culture);
					} else {
						Number evaled = Expression.Evaluate(new MathBindingValueCollection(values));
						if (evaled == null) {
							return TryReturnDefault(values, targetType, parameter, culture);
						} else {
							return evaled.GetValue();
						}
					}
				}
			}
			catch (NonDefaultableException e)
			{
				ErrorLog.Add(new Tuple<DateTime, string, Exception>(
					DateTime.Now,
					  "Encountered a non-defaultable exception while evaluating the MathNode, so re-threw the exception. "
					+ "(values = \"" + values
					+ "\", targetType = \"" + targetType?.FullName
					+ "\", parameter = \"" + parameter
					+ "\", culture = \"" + culture
					+ "\"). "
					+ "Exception: " + e.ToString(),
					e
				));
				throw;
			}
			catch (Exception e)
			{
				ErrorLog.Add(new Tuple<DateTime, string, Exception>(
					DateTime.Now,
					  "Encountered an exception while evaluating the MathNode, so (tried to) return the Default value. "
					+ "(values = \"" + values
					+ "\", targetType = \"" + targetType.FullName
					+ "\", parameter = \"" + parameter
					+ "\", culture = \"" + culture
					+ "\"). "
					+ "Exception: " + e.ToString(),
					e
				));
				return TryReturnDefault(values, targetType, parameter, culture);
			}
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NonDefaultableException(new InvalidOperationException("Math bindings cannot be converted backwards."));
		}

		[Serializable]
		public class NonDefaultableException : Exception
		{
			public Exception WrappedException;
			public NonDefaultableException(Exception wrappedException) : base("MathConverter.NonDefaultableException(\r\n" + wrappedException.ToString() + "\r\n).", wrappedException) {
				this.WrappedException = wrappedException;
			}
			public override string ToString()
			{
				return "MathConverter.NonDefaultableException(\r\n" + WrappedException.ToString() + "\r\n).";
			}
		}
	}

	internal class MathBindingValueCollection
	{
		private List<Number> _values = new List<Number>();
		public ReadOnlyCollection<Number> Values { get { return new ReadOnlyCollection<Number>(_values); } }
		public int Count { get { return _values.Count; } }

		public MathBindingValueCollection(object[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				try
				{
					if (values[i] == null) {
						MathConverter.ErrorLog.Add(new Tuple<DateTime, string, Exception>(
							DateTime.Now,
							"Info: Binding value at index " + i + " was null. " + "(values = \"" + values + "\").",
							null
						));
						_values.Add(null);
					} else {
						Type valType = values[i].GetType();
						if (Number.IsNumberType(valType)) {
							switch (Number.TypeToNumberType(valType))
							{
								case NumberType.SByte:   _values.Add(new Number<sbyte  >((sbyte  )values[i])); break;
								case NumberType.Short:   _values.Add(new Number<short  >((short  )values[i])); break;
								case NumberType.Int:     _values.Add(new Number<int    >((int    )values[i])); break;
								case NumberType.Long:    _values.Add(new Number<long   >((long   )values[i])); break;
								case NumberType.Byte:    _values.Add(new Number<byte   >((byte   )values[i])); break;
								case NumberType.UInt:    _values.Add(new Number<uint   >((uint   )values[i])); break;
								case NumberType.UShort:  _values.Add(new Number<ushort >((ushort )values[i])); break;
								case NumberType.ULong:   _values.Add(new Number<ulong  >((ulong  )values[i])); break;
								case NumberType.Decimal: _values.Add(new Number<decimal>((decimal)values[i])); break;
								case NumberType.Float:   _values.Add(new Number<float  >((float  )values[i])); break;
								case NumberType.Double:  _values.Add(new Number<double >((double )values[i])); break;
								default: {
									MathConverter.ErrorLog.Add(new Tuple<DateTime, string, Exception>(
										DateTime.Now,
										  "Info: Binding value at index " + i + " was a valid number type, but the type was somehow not detected. Null was used in place of it's value. "
										+ "(values = \"" + values + "\").",
										null
									));
									_values.Add(null); break;
								}
							}
						} else {
							MathConverter.ErrorLog.Add(new Tuple<DateTime, string, Exception>(
								DateTime.Now,
								  "Warning: Binding value at index " + i + " was not a valid number type, so null was used in place of it's value. "
								+ "(values = \"" + values + "\").",
								null
							));
							_values.Add(null);
						}
					}
				} catch (Exception e) {
					MathConverter.ErrorLog.Add(new Tuple<DateTime, string, Exception>(
						DateTime.Now,
						  "Encountered an exception while parsing binding value at index " + i + ". "
						+ "(values = \"" + values + "\"). "
						+ "Exception: " + e.ToString(),
						e
					));
				}
				//Make sure indexes are correct
				while (_values.Count < i + 1) {
					_values.Add(null);
				}
			}
		}
	}

	internal abstract class MathNodeBase
	{
		public abstract Number Evaluate(MathBindingValueCollection bindings);
	}

	internal abstract class ExpressionNode : MathNodeBase { }

	internal enum UnaryMathOperator
	{
		Negate,
		Abs,
		Sin,
		Cos,
		Tan,
		Sinh,
		Cosh,
		Tanh,
		Asin,
		Acos,
		Atan,
		Sqrt,
		EToThe,
		Ln,
		Log10,
		Sign,
		Round,
		Ceiling,
		Floor
	}

	[ContentProperty("Operand")]
	internal class NUnary : ExpressionNode
	{
		public MathNodeBase Operand { get; set; }
		public UnaryMathOperator Op { get; set; }

		public override Number Evaluate(MathBindingValueCollection bindings)
		{
			Number evaled = Operand.Evaluate(bindings);
			switch (Op)
			{
				case UnaryMathOperator.Negate : return Number.OPNumNegate(evaled);
				case UnaryMathOperator.Abs    : return Number.Abs        (evaled);
				case UnaryMathOperator.Sin    : return Number.Sin        (evaled);
				case UnaryMathOperator.Cos    : return Number.Cos        (evaled);
				case UnaryMathOperator.Tan    : return Number.Tan        (evaled);
				case UnaryMathOperator.Sinh   : return Number.Sinh       (evaled);
				case UnaryMathOperator.Cosh   : return Number.Cosh       (evaled);
				case UnaryMathOperator.Tanh   : return Number.Tanh       (evaled);
				case UnaryMathOperator.Asin   : return Number.Asin       (evaled);
				case UnaryMathOperator.Acos   : return Number.Acos       (evaled);
				case UnaryMathOperator.Atan   : return Number.Atan       (evaled);
				case UnaryMathOperator.Sqrt   : return Number.Sqrt       (evaled);
				case UnaryMathOperator.EToThe : return Number.EToThe     (evaled);
				case UnaryMathOperator.Ln     : return Number.Ln         (evaled);
				case UnaryMathOperator.Log10  : return Number.Log10      (evaled);
				case UnaryMathOperator.Sign   : return Number.Sign       (evaled);
				case UnaryMathOperator.Round  : return Number.Round      (evaled);
				case UnaryMathOperator.Ceiling: return Number.Ceiling    (evaled);
				case UnaryMathOperator.Floor  : return Number.Floor      (evaled);
			}
			throw new MathConverter.NonDefaultableException(new InvalidOperationException("The current Operation value ('" + Op.ToString() + "') is not valid."));
		}
	}

	internal enum BinaryMathOperator
	{
		Add,
		Subtract,
		Multiply,
		Divide,
		Modulus,
		Atan2,
		WholeDiv,
		Log,
		Max,
		Min,
		Pow,
		Round,
	}

	internal class NBinary : ExpressionNode
	{
		public MathNodeBase Left { get; set; }
		public MathNodeBase Right { get; set; }
		public BinaryMathOperator Op { get; set; }

		public override Number Evaluate(MathBindingValueCollection bindings)
		{
			Number evaledLeft = Left.Evaluate(bindings);
			Number evaledRight = Right.Evaluate(bindings);
			switch (Op)
			{
				case BinaryMathOperator.Add     : return Number.OPAdd     (evaledLeft, evaledRight);
				case BinaryMathOperator.Subtract: return Number.OPSubtract(evaledLeft, evaledRight);
				case BinaryMathOperator.Multiply: return Number.OPMultiply(evaledLeft, evaledRight);
				case BinaryMathOperator.Divide  : return Number.OPDivide  (evaledLeft, evaledRight);
				case BinaryMathOperator.Modulus : return Number.OPModulus (evaledLeft, evaledRight);
				case BinaryMathOperator.Atan2   : return Number.Atan2     (evaledLeft, evaledRight);
				case BinaryMathOperator.WholeDiv: return Number.WholeDiv  (evaledLeft, evaledRight);
				case BinaryMathOperator.Log     : return Number.Log       (evaledLeft, evaledRight);
				case BinaryMathOperator.Max     : return Number.Max       (evaledLeft, evaledRight);
				case BinaryMathOperator.Min     : return Number.Min       (evaledLeft, evaledRight);
				case BinaryMathOperator.Pow     : return Number.Pow       (evaledLeft, evaledRight);
				case BinaryMathOperator.Round   : return Number.Round     (evaledLeft, evaledRight);
			}
			throw new MathConverter.NonDefaultableException(new InvalidOperationException("The current Operation value is not valid."));
		}
	}

	[ContentProperty("Source")]
	internal class NCast : ExpressionNode
	{
		public MathNodeBase Source { get; set; }
		public NumberType DestType { get; set; }

		public override Number Evaluate(MathBindingValueCollection bindings)
		{
			Number evaled = Source.Evaluate(bindings);
			
			switch (DestType)
			{
				case NumberType.SByte:   return Number.Cast<sbyte  >(evaled);
				case NumberType.Short:   return Number.Cast<short  >(evaled);
				case NumberType.Int:     return Number.Cast<int    >(evaled);
				case NumberType.Long:    return Number.Cast<long   >(evaled);
				case NumberType.Byte:    return Number.Cast<byte   >(evaled);
				case NumberType.UInt:    return Number.Cast<uint   >(evaled);
				case NumberType.UShort:  return Number.Cast<ushort >(evaled);
				case NumberType.ULong:   return Number.Cast<ulong  >(evaled);
				case NumberType.Decimal: return Number.Cast<decimal>(evaled);
				case NumberType.Float:   return Number.Cast<float  >(evaled);
				case NumberType.Double:  return Number.Cast<double >(evaled);
			}
			throw new MathConverter.NonDefaultableException(new InvalidOperationException("The current Operation value ('" + DestType.ToString() + "') is not valid."));
		}
	}

	//	[ContentProperty("BindingIndex")]
	[TypeConverter(typeof(NBoundConverter))]
	internal class NBound : MathNodeBase
	{
		private int _bindingIndex = 0;
		public int BindingIndex {
			get { return _bindingIndex; }
			set {
				if (value < 0) {
					throw new MathConverter.NonDefaultableException(new ArgumentOutOfRangeException("The BindingIndex (" + value + ") may not be negative."));
				} else {
					_bindingIndex = value;
				}
			}
		}
		//	public string BindingIndexStr
		//	{
		//		get { return _bindingIndex.ToString(); }
		//		set {
		//	
		//		}
		//	}

		public override Number Evaluate(MathBindingValueCollection bindings)
		{
			Console.WriteLine("24: " + _bindingIndex + ", " + bindings.Count);
			if (BindingIndex >= bindings.Count) {
				throw new MathConverter.NonDefaultableException(new ArgumentOutOfRangeException("The specified BindingIndex (" + BindingIndex + ") is out of range for the provided list of bindings (Count = " + bindings.Count + "')."));
			} else {
				return bindings.Values[BindingIndex];
			}
		}

		public class NBoundConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				if (sourceType == typeof(string)) return true;
				if (sourceType == typeof(int)) return true;
				return base.CanConvertFrom(context, sourceType);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				if (value == null) return new NBound() { BindingIndex = 0 };
				if (value.GetType() == typeof(string))
				{
					int res;
					if (int.TryParse(Regex.Replace((string)value, @"[^0-9]", ""), out res)) {
						Console.WriteLine("#20: " + res + ", " + (string)value);
						return new NBound() { BindingIndex = res };
					} else {
						Console.WriteLine("#21");
						throw new MathConverter.NonDefaultableException(new FormatException("The specified string '" + (string)value + "' could not be parsed."));
					}
				}
				if (value.GetType() == typeof(int)) {
					Console.WriteLine("#22");
					return new NBound() { BindingIndex = (int)value };
				}
				Console.WriteLine("#23");
				throw new MathConverter.NonDefaultableException(new ArgumentException("The specified value is not of a valid type (string or int)."));
			}
		}
	}

	//	public class MathNumber : MathNode
	//	{
	//		private object _value;
	//	
	//		public sbyte  ? SByte   { get { return _value as sbyte  ?; } set { _value = value; } }
	//		public short  ? Short   { get { return _value as short  ?; } set { _value = value; } }
	//		public int    ? Int     { get { return _value as int    ?; } set { _value = value; } }
	//		public long   ? Long    { get { return _value as long   ?; } set { _value = value; } }
	//		public uint   ? UInt    { get { return _value as uint   ?; } set { _value = value; } }
	//		public ushort ? UShort  { get { return _value as ushort ?; } set { _value = value; } }
	//		public ulong  ? ULong   { get { return _value as ulong  ?; } set { _value = value; } }
	//		public byte   ? Byte    { get { return _value as byte   ?; } set { _value = value; } }
	//		public decimal? Decimal { get { return _value as decimal?; } set { _value = value; } }
	//		public float  ? Float   { get { return _value as float  ?; } set { _value = value; } }
	//		public double ? Double  { get { return _value as double ?; } set { _value = value; } }
	//	
	//		public Type Type { get { return _value == null ? null : _value.GetType(); } }
	//	
	//		public MathNumber() { }
	//	
	//		public override T EvaluateAs<T>()
	//		{
	//			Type type = typeof(T);
	//			if (
	//				   type == typeof(sbyte  )
	//				|| type == typeof(short  )
	//				|| type == typeof(int    )
	//				|| type == typeof(long   )
	//				|| type == typeof(uint   )
	//				|| type == typeof(ushort )
	//				|| type == typeof(ulong  )
	//				|| type == typeof(byte   )
	//				|| type == typeof(decimal)
	//				|| type == typeof(float  )
	//				|| type == typeof(double )
	//			) {
	//				return _value as T;
	//			} else {
	//				throw new TypeMismatchException("The  type \"" + type.FullName + "\" is not a valid MathNumber type");
	//			}
	//		}
	//	
	//		
	//		public static bool IsNumber(object obj)
	//		{
	//			if (obj == null) return false;
	//			Type type = obj.GetType();
	//			return (
	//				//Signed
	//				   type == typeof(sbyte  )
	//				|| type == typeof(short  )
	//				|| type == typeof(int    )
	//				|| type == typeof(long   )
	//				//Unsigned
	//				|| type == typeof(byte   )
	//				|| type == typeof(uint   )
	//				|| type == typeof(ushort )
	//				|| type == typeof(ulong  )
	//				//Other
	//				|| type == typeof(decimal)
	//				|| type == typeof(float  )
	//				|| type == typeof(double )
	//			);
	//		}
	//	}

	/// <summary>
	/// Note: Only one property (SByte, Short, Int, etc) may be set. If another is set, it overwrites the first
	/// </summary>
	internal class NLiteral : MathNodeBase
	{
		private Number _value = null;
		
		public sbyte  ? SByte   { get { return (_value as Number<sbyte  >)?.Value; } set { _value = new Number<sbyte  >(value); } }
		public short  ? Short   { get { return (_value as Number<short  >)?.Value; } set { _value = new Number<short  >(value); } }
		public int    ? Int     { get { return (_value as Number<int    >)?.Value; } set { _value = new Number<int    >(value); } }
		public long   ? Long    { get { return (_value as Number<long   >)?.Value; } set { _value = new Number<long   >(value); } }
		public uint   ? UInt    { get { return (_value as Number<uint   >)?.Value; } set { _value = new Number<uint   >(value); } }
		public ushort ? UShort  { get { return (_value as Number<ushort >)?.Value; } set { _value = new Number<ushort >(value); } }
		public ulong  ? ULong   { get { return (_value as Number<ulong  >)?.Value; } set { _value = new Number<ulong  >(value); } }
		public byte   ? Byte    { get { return (_value as Number<byte   >)?.Value; } set { _value = new Number<byte   >(value); } }
		public decimal? Decimal { get { return (_value as Number<decimal>)?.Value; } set { _value = new Number<decimal>(value); } }
		public float  ? Float   { get { return (_value as Number<float  >)?.Value; } set { _value = new Number<float  >(value); } }
		public double ? Double  { get { return (_value as Number<double >)?.Value; } set { _value = new Number<double >(value); } }
		
		public Type Type { get { return _value == null ? null : _value.Type; } }
	
		//	public LiteralNode() { }
	
		public override Number Evaluate(MathBindingValueCollection bindings)
		{
			//	if (Number.IsNumber(typeof(T)))
			//	{
			//		//return Number.Cast(_value, typeof(T));
			//		return Number.Cast<T>(_value);
			//		//	switch (Number.TypeToNumberType(typeof(T)))
			//		//	{
			//		//		case Number.NumberTypes.SByte:   return (sbyte  )((Type-property)_value);
			//		//		case Number.NumberTypes.Short:   return (short  )((Type-property)_value);
			//		//		case Number.NumberTypes.Int:     return (int    )((Type-property)_value);
			//		//		case Number.NumberTypes.Long:    return (long   )((Type-property)_value);
			//		//		case Number.NumberTypes.Byte:    return (byte   )((Type-property)_value);
			//		//		case Number.NumberTypes.UInt:    return (uint   )((Type-property)_value);
			//		//		case Number.NumberTypes.UShort:  return (ushort )((Type-property)_value);
			//		//		case Number.NumberTypes.ULong:   return (ulong  )((Type-property)_value);
			//		//		case Number.NumberTypes.Decimal: return (decimal)((Type-property)_value);
			//		//		case Number.NumberTypes.Float:   return (float  )((Type-property)_value);
			//		//		case Number.NumberTypes.Double:  return (double )((Type-property)_value);
			//		//	}
			//	} else
			//	{
			//		throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
			//	}
			return _value;
		}
	}

	internal enum NumberType
	{
		SByte,
		Short,
		Int,
		Long,
		Byte,
		UInt,
		UShort,
		ULong,
		Decimal,
		Float,
		Double,
	}

	internal enum NumberTypeCategories
	{
		Double,
		Decimal,
	}

	internal abstract class Number
	{
		public abstract Type Type { get; }

		public abstract object GetValue();

		public abstract Number Clone();

		//	private static Dictionary<Type, Func<object, sbyte>[]> CastsToSByte = new Dictionary<Type, Func<object, sbyte>[]>();
		//	
		//	static Number()
		//	{
		//		CastsToSByte.Add(typeof(int) (x));
		//	}

		protected Number() { }

		public static bool IsNumberType(Type type)
		{
			if (type == null) return false;
			return (
				//Signed
				   type == typeof(sbyte  )
				|| type == typeof(short  )
				|| type == typeof(int    )
				|| type == typeof(long   )
				//Unsigned
				|| type == typeof(byte   )
				|| type == typeof(uint   )
				|| type == typeof(ushort )
				|| type == typeof(ulong  )
				//Other
				|| type == typeof(decimal)
				|| type == typeof(float  )
				|| type == typeof(double )
			);
		}

		public static NumberType TypeToNumberType(Type type)
		{
			//Null
			if (type == null) throw new ArgumentNullException("type", "The 'type' argument cannot be null.");
			//Signed
			if (type == typeof(sbyte  )) return NumberType.SByte;
			if (type == typeof(short  )) return NumberType.Short;
			if (type == typeof(int    )) return NumberType.Int;
			if (type == typeof(long   )) return NumberType.Long;
			//Unsigned							
			if (type == typeof(byte   )) return NumberType.Byte;
			if (type == typeof(uint   )) return NumberType.UInt;
			if (type == typeof(ushort )) return NumberType.UShort;
			if (type == typeof(ulong  )) return NumberType.ULong;
			//Other								
			if (type == typeof(decimal)) return NumberType.Decimal;
			if (type == typeof(float  )) return NumberType.Float;
			if (type == typeof(double )) return NumberType.Double;
			//Throw
			throw new InvalidOperationException("The type '" + type.FullName + "' is not a valid number type.");
		}

		public static NumberTypeCategories TypeToNumberTypeCategory(Type type)
		{
			//Null
			if (type == null) throw new ArgumentNullException("type", "The 'type' argument cannot be null.");
			//Signed
			if (type == typeof(sbyte  )) return NumberTypeCategories.Double;
			if (type == typeof(short  )) return NumberTypeCategories.Double;
			if (type == typeof(int    )) return NumberTypeCategories.Double;
			if (type == typeof(long   )) return NumberTypeCategories.Decimal;
			//Unsigned							
			if (type == typeof(byte   )) return NumberTypeCategories.Double;
			if (type == typeof(uint   )) return NumberTypeCategories.Double;
			if (type == typeof(ushort )) return NumberTypeCategories.Double;
			if (type == typeof(ulong  )) return NumberTypeCategories.Decimal;
			//Other								
			if (type == typeof(decimal)) return NumberTypeCategories.Decimal;
			if (type == typeof(float  )) return NumberTypeCategories.Double;
			if (type == typeof(double )) return NumberTypeCategories.Double;
			//Throw
			throw new InvalidOperationException("The type '" + type.FullName + "' is not a valid number type.");
		}

		public static Number<TRes> Cast<TSource, TRes>(Number<TSource> n) where TSource : struct where TRes : struct
		{
			return (Number<TRes>)Cast(n, typeof(TRes));
		}
		public static Number<TRes> Cast<TRes>(Number n) where TRes : struct
		{
			return (Number<TRes>)Cast(n, typeof(TRes));
		}

		public static Number Cast(Number n, Type type)
		{
			//if (n == null) throw new ArgumentNullException("The argument 'n' is null");
			if (n == null) return null;
			if (type == null) throw new ArgumentNullException("The argument 'type' is null");
			if (!IsNumberType(type)) throw new ArgumentException("The provided type is not a number type (see IsNumber())");
			if (n.Type == type) return n.Clone();
			try {
				switch (TypeToNumberType(n.Type)) {
					case NumberType.SByte:   switch (TypeToNumberType(type)) {
						case NumberType.SByte:   return n.Clone();
						case NumberType.Short:   return new Number<short  >((short  )(sbyte  )n.GetValue());
						case NumberType.Int:     return new Number<int    >((int    )(sbyte  )n.GetValue());
						case NumberType.Long:    return new Number<long   >((long   )(sbyte  )n.GetValue());
						case NumberType.Byte:    return new Number<byte   >((byte   )(sbyte  )n.GetValue());
						case NumberType.UInt:    return new Number<uint   >((uint   )(sbyte  )n.GetValue());
						case NumberType.UShort:  return new Number<ushort >((ushort )(sbyte  )n.GetValue());
						case NumberType.ULong:   return new Number<ulong  >((ulong  )(sbyte  )n.GetValue());
						case NumberType.Decimal: return new Number<decimal>((decimal)(sbyte  )n.GetValue());
						case NumberType.Float:   return new Number<float  >((float  )(sbyte  )n.GetValue());
						case NumberType.Double:  return new Number<double >((double )(sbyte  )n.GetValue());
						default: throw new InvalidOperationException("Invalid destination type.");
					}
					case NumberType.Short:   switch (TypeToNumberType(type)) {
						case NumberType.SByte:   return new Number<sbyte  >((sbyte  )(short  )n.GetValue());
						case NumberType.Short:   return n.Clone();
						case NumberType.Int:     return new Number<int    >((int    )(short  )n.GetValue());
						case NumberType.Long:    return new Number<long   >((long   )(short  )n.GetValue());
						case NumberType.Byte:    return new Number<byte   >((byte   )(short  )n.GetValue());
						case NumberType.UInt:    return new Number<uint   >((uint   )(short  )n.GetValue());
						case NumberType.UShort:  return new Number<ushort >((ushort )(short  )n.GetValue());
						case NumberType.ULong:   return new Number<ulong  >((ulong  )(short  )n.GetValue());
						case NumberType.Decimal: return new Number<decimal>((decimal)(short  )n.GetValue());
						case NumberType.Float:   return new Number<float  >((float  )(short  )n.GetValue());
						case NumberType.Double:  return new Number<double >((double )(short  )n.GetValue());
						default: throw new InvalidOperationException("Invalid destination type.");
					}
					case NumberType.Int:     switch (TypeToNumberType(type)) {
						case NumberType.SByte:   return new Number<sbyte  >((sbyte  )(int    )n.GetValue());
						case NumberType.Short:   return new Number<short  >((short  )(int    )n.GetValue());
						case NumberType.Int:     return n.Clone();
						case NumberType.Long:    return new Number<long   >((long   )(int    )n.GetValue());
						case NumberType.Byte:    return new Number<byte   >((byte   )(int    )n.GetValue());
						case NumberType.UInt:    return new Number<uint   >((uint   )(int    )n.GetValue());
						case NumberType.UShort:  return new Number<ushort >((ushort )(int    )n.GetValue());
						case NumberType.ULong:   return new Number<ulong  >((ulong  )(int    )n.GetValue());
						case NumberType.Decimal: return new Number<decimal>((decimal)(int    )n.GetValue());
						case NumberType.Float:   return new Number<float  >((float  )(int    )n.GetValue());
						case NumberType.Double:  return new Number<double >((double )(int    )n.GetValue());
						default: throw new InvalidOperationException("Invalid destination type.");
					}
					case NumberType.Long:    switch (TypeToNumberType(type)) {
						case NumberType.SByte:   return new Number<sbyte  >((sbyte  )(long   )n.GetValue());
						case NumberType.Short:   return new Number<short  >((short  )(long   )n.GetValue());
						case NumberType.Int:     return new Number<int    >((int    )(long   )n.GetValue());
						case NumberType.Long:    return n.Clone();
						case NumberType.Byte:    return new Number<byte   >((byte   )(long   )n.GetValue());
						case NumberType.UInt:    return new Number<uint   >((uint   )(long   )n.GetValue());
						case NumberType.UShort:  return new Number<ushort >((ushort )(long   )n.GetValue());
						case NumberType.ULong:   return new Number<ulong  >((ulong  )(long   )n.GetValue());
						case NumberType.Decimal: return new Number<decimal>((decimal)(long   )n.GetValue());
						case NumberType.Float:   return new Number<float  >((float  )(long   )n.GetValue());
						case NumberType.Double:  return new Number<double >((double )(long   )n.GetValue());
						default: throw new InvalidOperationException("Invalid destination type.");
					}
					case NumberType.Byte:    switch (TypeToNumberType(type)) {
						case NumberType.SByte:   return new Number<sbyte  >((sbyte  )(byte   )n.GetValue());
						case NumberType.Short:   return new Number<short  >((short  )(byte   )n.GetValue());
						case NumberType.Int:     return new Number<int    >((int    )(byte   )n.GetValue());
						case NumberType.Long:    return new Number<long   >((long   )(byte   )n.GetValue());
						case NumberType.Byte:    return n.Clone();
						case NumberType.UInt:    return new Number<uint   >((uint   )(byte   )n.GetValue());
						case NumberType.UShort:  return new Number<ushort >((ushort )(byte   )n.GetValue());
						case NumberType.ULong:   return new Number<ulong  >((ulong  )(byte   )n.GetValue());
						case NumberType.Decimal: return new Number<decimal>((decimal)(byte   )n.GetValue());
						case NumberType.Float:   return new Number<float  >((float  )(byte   )n.GetValue());
						case NumberType.Double:  return new Number<double >((double )(byte   )n.GetValue());
						default: throw new InvalidOperationException("Invalid destination type.");
					}
					case NumberType.UInt:    switch (TypeToNumberType(type)) {
						case NumberType.SByte:   return new Number<sbyte  >((sbyte  )(uint   )n.GetValue());
						case NumberType.Short:   return new Number<short  >((short  )(uint   )n.GetValue());
						case NumberType.Int:     return new Number<int    >((int    )(uint   )n.GetValue());
						case NumberType.Long:    return new Number<long   >((long   )(uint   )n.GetValue());
						case NumberType.Byte:    return new Number<byte   >((byte   )(uint   )n.GetValue());
						case NumberType.UInt:    return n.Clone();
						case NumberType.UShort:  return new Number<ushort >((ushort )(uint   )n.GetValue());
						case NumberType.ULong:   return new Number<ulong  >((ulong  )(uint   )n.GetValue());
						case NumberType.Decimal: return new Number<decimal>((decimal)(uint   )n.GetValue());
						case NumberType.Float:   return new Number<float  >((float  )(uint   )n.GetValue());
						case NumberType.Double:  return new Number<double >((double )(uint   )n.GetValue());
						default: throw new InvalidOperationException("Invalid destination type.");
					}
					case NumberType.UShort:  switch (TypeToNumberType(type)) {
						case NumberType.SByte:   return new Number<sbyte  >((sbyte  )(ushort )n.GetValue());
						case NumberType.Short:   return new Number<short  >((short  )(ushort )n.GetValue());
						case NumberType.Int:     return new Number<int    >((int    )(ushort )n.GetValue());
						case NumberType.Long:    return new Number<long   >((long   )(ushort )n.GetValue());
						case NumberType.Byte:    return new Number<byte   >((byte   )(ushort )n.GetValue());
						case NumberType.UInt:    return new Number<uint   >((uint   )(ushort )n.GetValue());
						case NumberType.UShort:  return n.Clone();
						case NumberType.ULong:   return new Number<ulong  >((ulong  )(ushort )n.GetValue());
						case NumberType.Decimal: return new Number<decimal>((decimal)(ushort )n.GetValue());
						case NumberType.Float:   return new Number<float  >((float  )(ushort )n.GetValue());
						case NumberType.Double:  return new Number<double >((double )(ushort )n.GetValue());
						default: throw new InvalidOperationException("Invalid destination type.");
					}
					case NumberType.ULong:   switch (TypeToNumberType(type)) {
						case NumberType.SByte:   return new Number<sbyte  >((sbyte  )(ulong  )n.GetValue());
						case NumberType.Short:   return new Number<short  >((short  )(ulong  )n.GetValue());
						case NumberType.Int:     return new Number<int    >((int    )(ulong  )n.GetValue());
						case NumberType.Long:    return new Number<long   >((long   )(ulong  )n.GetValue());
						case NumberType.Byte:    return new Number<byte   >((byte   )(ulong  )n.GetValue());
						case NumberType.UInt:    return new Number<uint   >((uint   )(ulong  )n.GetValue());
						case NumberType.UShort:  return new Number<ushort >((ushort )(ulong  )n.GetValue());
						case NumberType.ULong:   return n.Clone();
						case NumberType.Decimal: return new Number<decimal>((decimal)(ulong  )n.GetValue());
						case NumberType.Float:   return new Number<float  >((float  )(ulong  )n.GetValue());
						case NumberType.Double:  return new Number<double >((double )(ulong  )n.GetValue());
						default: throw new InvalidOperationException("Invalid destination type.");
					}
					case NumberType.Decimal: switch (TypeToNumberType(type)) {
						case NumberType.SByte:   return new Number<sbyte  >((sbyte  )(decimal)n.GetValue());
						case NumberType.Short:   return new Number<short  >((short  )(decimal)n.GetValue());
						case NumberType.Int:     return new Number<int    >((int    )(decimal)n.GetValue());
						case NumberType.Long:    return new Number<long   >((long   )(decimal)n.GetValue());
						case NumberType.Byte:    return new Number<byte   >((byte   )(decimal)n.GetValue());
						case NumberType.UInt:    return new Number<uint   >((uint   )(decimal)n.GetValue());
						case NumberType.UShort:  return new Number<ushort >((ushort )(decimal)n.GetValue());
						case NumberType.ULong:   return new Number<ulong  >((ulong  )(decimal)n.GetValue());
						case NumberType.Decimal: return n.Clone();
						case NumberType.Float:   return new Number<float  >((float  )(decimal)n.GetValue());
						case NumberType.Double:  return new Number<double >((double )(decimal)n.GetValue());
						default: throw new InvalidOperationException("Invalid destination type.");
					}
					case NumberType.Float:   switch (TypeToNumberType(type)) {
						case NumberType.SByte:   return new Number<sbyte  >((sbyte  )(float  )n.GetValue());
						case NumberType.Short:   return new Number<short  >((short  )(float  )n.GetValue());
						case NumberType.Int:     return new Number<int    >((int    )(float  )n.GetValue());
						case NumberType.Long:    return new Number<long   >((long   )(float  )n.GetValue());
						case NumberType.Byte:    return new Number<byte   >((byte   )(float  )n.GetValue());
						case NumberType.UInt:    return new Number<uint   >((uint   )(float  )n.GetValue());
						case NumberType.UShort:  return new Number<ushort >((ushort )(float  )n.GetValue());
						case NumberType.ULong:   return new Number<ulong  >((ulong  )(float  )n.GetValue());
						case NumberType.Decimal: return new Number<decimal>((decimal)(float  )n.GetValue());
						case NumberType.Float:   return n.Clone();
						case NumberType.Double:  return new Number<double >((double )(float  )n.GetValue());
						default: throw new InvalidOperationException("Invalid destination type.");
					}
					case NumberType.Double:  switch (TypeToNumberType(type)) {
						case NumberType.SByte:   return new Number<sbyte  >((sbyte  )(double )n.GetValue());
						case NumberType.Short:   return new Number<short  >((short  )(double )n.GetValue());
						case NumberType.Int:     return new Number<int    >((int    )(double )n.GetValue());
						case NumberType.Long:    return new Number<long   >((long   )(double )n.GetValue());
						case NumberType.Byte:    return new Number<byte   >((byte   )(double )n.GetValue());
						case NumberType.UInt:    return new Number<uint   >((uint   )(double )n.GetValue());
						case NumberType.UShort:  return new Number<ushort >((ushort )(double )n.GetValue());
						case NumberType.ULong:   return new Number<ulong  >((ulong  )(double )n.GetValue());
						case NumberType.Decimal: return new Number<decimal>((decimal)(double )n.GetValue());
						case NumberType.Float:   return new Number<float  >((float  )(double )n.GetValue());
						case NumberType.Double:  return n.Clone();
						default: throw new InvalidOperationException("Invalid destination type.");
					}
					default: throw new InvalidOperationException("Invalid source type.");
				}
			} catch (Exception e) {
				throw new ArgumentException("The source type '" + n.Type.FullName + "' could not be cast to destination type '" + type.FullName + "'", e);
			}
		}


		/// <summary>
		/// Just adds some null checking stuff (a Number with Value = null == a null Number). Does NOT add the functionality from OPEq();
		/// </summary>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, null) && ReferenceEquals(this.GetValue(), null)) return true;
			return Equals(this, obj);
		}
		/// <summary>
		/// Just adds some null checking stuff (a Number with Value = null == a null Number). Does NOT add the functionality from OPEq();
		/// </summary>
		public static bool operator ==(Number n1, Number n2)
		{
			bool n1Null = ReferenceEquals(n1, null) || ReferenceEquals(n1.GetValue(), null);
			bool n2Null = ReferenceEquals(n2, null) || ReferenceEquals(n2.GetValue(), null);
			if (n1Null && n2Null) return true;
			return Equals(n1, n2);
		}
		/// <summary>
		/// Just adds some null checking stuff (a Number with Value = null == a null Number). Does NOT add the functionality from OPEq();
		/// </summary>
		public static bool operator !=(Number n1, Number n2)
		{
			bool n1Null = ReferenceEquals(n1, null) || ReferenceEquals(n1.GetValue(), null);
			bool n2Null = ReferenceEquals(n2, null) || ReferenceEquals(n2.GetValue(), null);
			if (n1Null && n2Null) return false;
			return !Equals(n1, n2);
		}
		public override int GetHashCode()
		{
			object val = this.GetValue();
			return val == null ? 0 : val.GetHashCode();
		}

		/// <summary>
		/// Calls "new Number&lt;type&gt;((type) +(type)n.GetValue() )"
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static Number OPValueOf(Number n) {
			//	if (n == null) return new Number<T>();
			//	switch (TypeToNumberType(typeof(T))) {
			//		case NumberTypes.SByte:   return new Number<T>((+(n.Value as sbyte  ?)) as T?);
			//		case NumberTypes.Short:   return new Number<T>((+(n.Value as short  ?)) as T?);
			//		case NumberTypes.Int:     return new Number<T>((+(n.Value as int    ?)) as T?);
			//		case NumberTypes.Long:    return new Number<T>((+(n.Value as long   ?)) as T?);
			//		case NumberTypes.Byte:    return new Number<T>((+(n.Value as byte   ?)) as T?);
			//		case NumberTypes.UInt:    return new Number<T>((+(n.Value as uint   ?)) as T?);
			//		case NumberTypes.UShort:  return new Number<T>((+(n.Value as ushort ?)) as T?);
			//		case NumberTypes.ULong:   return new Number<T>((+(n.Value as ulong  ?)) as T?);
			//		case NumberTypes.Decimal: return new Number<T>((+(n.Value as decimal?)) as T?);
			//		case NumberTypes.Float:   return new Number<T>((+(n.Value as float  ?)) as T?);
			//		case NumberTypes.Double:  return new Number<T>((+(n.Value as double ?)) as T?);
			//	}
			//	throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
			return ApplyFunc(
				n,
				ifSByte  : (x) => (sbyte  )+x,
				ifShort  : (x) => (short  )+x,
				ifInt    : (x) => (int    )+x,
				ifLong   : (x) => (long   )+x,
				ifByte   : (x) => (byte   )+x,
				ifUInt   : (x) => (uint   )+x,
				ifUShort : (x) => (ushort )+x,
				ifULong  : (x) => (ulong  )+x,
				ifDecimal: (x) => (decimal)+x,
				ifFloat  : (x) => (float  )+x,
				ifDouble : (x) => (double )+x
			);
		}
		/// <summary>
		/// Calls "new Number&lt;type&gt;((type) -(type)n.GetValue() )" for all types except ulong, where it calls "new Number&lt;ulong&gt;((ulong) (0-(ulong)n.GetValue()) )"
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static Number OPNumNegate(Number n)
		{
			return ApplyFunc(
				n,
				ifSByte  : (x) => (sbyte  )-x,
				ifShort  : (x) => (short  )-x,
				ifInt    : (x) => (int    )-x,
				ifLong   : (x) => (long   )-x,
				ifByte   : (x) => (byte   )-x,
				ifUInt   : (x) => (uint   )-x,
				ifUShort : (x) => (ushort )-x,
				ifULong  : (x) => (ulong  )(0-x),
				ifDecimal: (x) => (decimal)-x,
				ifFloat  : (x) => (float  )-x,
				ifDouble : (x) => (double )-x
			);
		}
		/// <summary>
		/// Calls "new Number&lt;decimal&gt;(ToDecimal(n1) + ToDecimal(n2))" or "new Number&lt;double&gt;(ToDouble(n1) + ToDouble(n2))" depending on the result of TypeToNumberCategoryType()
		/// <para/>
		/// Only uses decimal if both TypeToNumberCategoryType(n1) and TypeToNumberCategoryType(n2) return NumberTypeCategories.Decimal
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static Number OPAdd(Number n1, Number n2)
		{
			//	if (n1 == null || n2 == null) return new Number<T>();
			//	switch (TypeToNumberType(typeof(T))) {
			//		case NumberTypes.SByte:   return new Number<T>(((n1.Value as sbyte  ?) + (n2.Value as sbyte  ?)) as T?);
			//		case NumberTypes.Short:   return new Number<T>(((n1.Value as short  ?) + (n2.Value as short  ?)) as T?);
			//		case NumberTypes.Int:     return new Number<T>(((n1.Value as int    ?) + (n2.Value as int    ?)) as T?);
			//		case NumberTypes.Long:    return new Number<T>(((n1.Value as long   ?) + (n2.Value as long   ?)) as T?);
			//		case NumberTypes.Byte:    return new Number<T>(((n1.Value as byte   ?) + (n2.Value as byte   ?)) as T?);
			//		case NumberTypes.UInt:    return new Number<T>(((n1.Value as uint   ?) + (n2.Value as uint   ?)) as T?);
			//		case NumberTypes.UShort:  return new Number<T>(((n1.Value as ushort ?) + (n2.Value as ushort ?)) as T?);
			//		case NumberTypes.ULong:   return new Number<T>(((n1.Value as ulong  ?) + (n2.Value as ulong  ?)) as T?);
			//		case NumberTypes.Decimal: return new Number<T>(((n1.Value as decimal?) + (n2.Value as decimal?)) as T?);
			//		case NumberTypes.Float:   return new Number<T>(((n1.Value as float  ?) + (n2.Value as float  ?)) as T?);
			//		case NumberTypes.Double:  return new Number<T>(((n1.Value as double ?) + (n2.Value as double ?)) as T?);
			//	}
			//	throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
			return ApplyFuncTwo(
				n1,
				n2,
				ifDecimal: (x1, x2) => x1 + x2,
				ifDouble : (x1, x2) => x1 + x2
			);
		}
		/// <summary>
		/// Calls "new Number&lt;decimal&gt;(ToDecimal(n1) - ToDecimal(n2))" or "new Number&lt;double&gt;(ToDouble(n1) - ToDouble(n2))" depending on the result of TypeToNumberCategoryType()
		/// <para/>
		/// Only uses decimal if both TypeToNumberCategoryType(n1) and TypeToNumberCategoryType(n2) return NumberTypeCategories.Decimal
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static Number OPSubtract(Number n1, Number n2)
		{
			return ApplyFuncTwo(
				n1,
				n2,
				ifDecimal: (x1, x2) => x1 - x2,
				ifDouble : (x1, x2) => x1 - x2
			);
		}
		/// <summary>
		/// Calls "new Number&lt;decimal&gt;(ToDecimal(n1) * ToDecimal(n2))" or "new Number&lt;double&gt;(ToDouble(n1) * ToDouble(n2))" depending on the result of TypeToNumberCategoryType()
		/// <para/>
		/// Only uses decimal if both TypeToNumberCategoryType(n1) and TypeToNumberCategoryType(n2) return NumberTypeCategories.Decimal
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static Number OPMultiply(Number n1, Number n2)
		{
			return ApplyFuncTwo(
				n1,
				n2,
				ifDecimal: (x1, x2) => x1 * x2,
				ifDouble : (x1, x2) => x1 * x2
			);
		}
		/// <summary>
		/// Calls "new Number&lt;decimal&gt;(ToDecimal(n1) / ToDecimal(n2))" or "new Number&lt;double&gt;(ToDouble(n1) / ToDouble(n2))" depending on the result of TypeToNumberCategoryType()
		/// <para/>
		/// Only uses decimal if both TypeToNumberCategoryType(n1) and TypeToNumberCategoryType(n2) return NumberTypeCategories.Decimal
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static Number OPDivide(Number n1, Number n2)
		{
			return ApplyFuncTwo(
				n1,
				n2,
				ifDecimal: (x1, x2) => x1 / x2,
				ifDouble : (x1, x2) => x1 / x2
			);
		}
		/// <summary>
		/// Calls "new Number&lt;decimal&gt;(ToDecimal(n1) % ToDecimal(n2))" or "new Number&lt;double&gt;(ToDouble(n1) % ToDouble(n2))" depending on the result of TypeToNumberCategoryType()
		/// <para/>
		/// Only uses decimal if both TypeToNumberCategoryType(n1) and TypeToNumberCategoryType(n2) return NumberTypeCategories.Decimal
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static Number OPModulus(Number n1, Number n2)
		{
			return ApplyFuncTwo(
				n1,
				n2,
				ifDecimal: (x1, x2) => x1 % x2,
				ifDouble : (x1, x2) => x1 % x2
			);
		}
		/// <summary>
		/// Calls "ToDecimal(n1) == ToDecimal(n2)" or "ToDouble(n1) == ToDouble(n2)", with null == new Number&lt;T&gt;(null) != new Number&lt;T&gt;(non-null-value)
		/// <para/>
		/// Only uses decimal if both TypeToNumberCategoryType(n1) and TypeToNumberCategoryType(n2) return NumberTypeCategories.Decimal
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPEq(Number n1, Number n2)
		{
			//	if (n1 == null && n2 == null) return true;
			//	else if (n1 == null || n2 == null) return false;
			//	switch (TypeToNumberType(typeof(T))) {
			//		case NumberTypes.SByte:   return (n1.Value as sbyte  ?) == (n2.Value as sbyte  ?);
			//		case NumberTypes.Short:   return (n1.Value as short  ?) == (n2.Value as short  ?);
			//		case NumberTypes.Int:     return (n1.Value as int    ?) == (n2.Value as int    ?);
			//		case NumberTypes.Long:    return (n1.Value as long   ?) == (n2.Value as long   ?);
			//		case NumberTypes.Byte:    return (n1.Value as byte   ?) == (n2.Value as byte   ?);
			//		case NumberTypes.UInt:    return (n1.Value as uint   ?) == (n2.Value as uint   ?);
			//		case NumberTypes.UShort:  return (n1.Value as ushort ?) == (n2.Value as ushort ?);
			//		case NumberTypes.ULong:   return (n1.Value as ulong  ?) == (n2.Value as ulong  ?);
			//		case NumberTypes.Decimal: return (n1.Value as decimal?) == (n2.Value as decimal?);
			//		case NumberTypes.Float:   return (n1.Value as float  ?) == (n2.Value as float  ?);
			//		case NumberTypes.Double:  return (n1.Value as double ?) == (n2.Value as double ?);
			//	}
			//	throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
			return ApplyComparison(
				n1,
				n2,
				(x1, x2) => x1 == x2,
				(x1, x2) => x1 == x2,
				true,
				false
			);
		}
		/// <summary>
		/// Calls "ToDecimal(n1) != ToDecimal(n2)" or "ToDouble(n1) != ToDouble(n2)", with null == new Number&lt;T&gt;(null) != new Number&lt;T&gt;(non-null-value)
		/// <para/>
		/// Only uses decimal if both TypeToNumberCategoryType(n1) and TypeToNumberCategoryType(n2) return NumberTypeCategories.Decimal
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPNeq(Number n1, Number n2)
		{
			return ApplyComparison(
				n1,
				n2,
				(x1, x2) => x1 != x2,
				(x1, x2) => x1 != x2,
				false,
				true
			);
		}
		/// <summary>
		/// Calls "ToDecimal(n1) &lt; ToDecimal(n2)" or "ToDouble(n1) &lt; ToDouble(n2)", with anything involving null or new Number&lt;T&gt;(null) returning false
		/// <para/>
		/// Only uses decimal if both TypeToNumberCategoryType(n1) and TypeToNumberCategoryType(n2) return NumberTypeCategories.Decimal
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPLss(Number n1, Number n2)
		{
			return ApplyComparison(
				n1,
				n2,
				(x1, x2) => x1 < x2,
				(x1, x2) => x1 < x2,
				false,
				false
			);
		}
		/// <summary>
		/// Calls "ToDecimal(n1) &gt; ToDecimal(n2)" or "ToDouble(n1) &gt; ToDouble(n2)", with anything involving null or new Number&lt;T&gt;(null) returning false
		/// <para/>
		/// Only uses decimal if both TypeToNumberCategoryType(n1) and TypeToNumberCategoryType(n2) return NumberTypeCategories.Decimal
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPGtr(Number n1, Number n2)
		{
			return ApplyComparison(
				n1,
				n2,
				(x1, x2) => x1 > x2,
				(x1, x2) => x1 > x2,
				false,
				false
			);
		}
		/// <summary>
		/// Calls "ToDecimal(n1) &lt;= ToDecimal(n2)" or "ToDouble(n1) &lt;= ToDouble(n2)", with null == new Number&lt;T&gt;(null) != new Number&lt;T&gt;(non-null-value)
		/// <para/>
		/// Only uses decimal if both TypeToNumberCategoryType(n1) and TypeToNumberCategoryType(n2) return NumberTypeCategories.Decimal
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPLeq(Number n1, Number n2)
		{
			return ApplyComparison(
				n1,
				n2,
				(x1, x2) => x1 <= x2,
				(x1, x2) => x1 <= x2,
				true,
				false
			);
		}
		/// <summary>
		/// Calls "ToDecimal(n1) &gt;= ToDecimal(n2)" or "ToDouble(n1) &gt;= ToDouble(n2)", with null == new Number&lt;T&gt;(null) != new Number&lt;T&gt;(non-null-value)
		/// <para/>
		/// Only uses decimal if both TypeToNumberCategoryType(n1) and TypeToNumberCategoryType(n2) return NumberTypeCategories.Decimal
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPGeq(Number n1, Number n2)
		{
			return ApplyComparison(
				n1,
				n2,
				(x1, x2) => x1 >= x2,
				(x1, x2) => x1 >= x2,
				true,
				false
			);
		}


		public static double ToDouble(Number n)
		{
			if (n == null) return double.NaN;
			try
			{
				switch (TypeToNumberType(n.Type))
				{
					case NumberType.SByte:   return (double)((sbyte  )n.GetValue());
					case NumberType.Short:   return (double)((short  )n.GetValue());
					case NumberType.Int:     return (double)((int    )n.GetValue());
					case NumberType.Long:    return (double)((long   )n.GetValue());
					case NumberType.Byte:    return (double)((byte   )n.GetValue());
					case NumberType.UInt:    return (double)((uint   )n.GetValue());
					case NumberType.UShort:  return (double)((ushort )n.GetValue());
					case NumberType.ULong:   return (double)((ulong  )n.GetValue());
					case NumberType.Decimal: return (double)((decimal)n.GetValue());
					case NumberType.Float:   return (double)((float  )n.GetValue());
					case NumberType.Double:  return (double)((double )n.GetValue());
				}
				throw new InvalidOperationException("The type '" + n.Type.FullName + "' is not a valid number type.");
			} catch (InvalidCastException) {
				return double.NaN;
			}
		}
		public static int ToInt(Number n)
		{
			if (n == null) return default(int);
			switch (TypeToNumberType(n.Type))
			{
				case NumberType.SByte:   return (int)((sbyte  )n.GetValue());
				case NumberType.Short:   return (int)((short  )n.GetValue());
				case NumberType.Int:     return (int)((int    )n.GetValue());
				case NumberType.Long:    return (int)((long   )n.GetValue());
				case NumberType.Byte:    return (int)((byte   )n.GetValue());
				case NumberType.UInt:    return (int)((uint   )n.GetValue());
				case NumberType.UShort:  return (int)((ushort )n.GetValue());
				case NumberType.ULong:   return (int)((ulong  )n.GetValue());
				case NumberType.Decimal: return (int)((decimal)n.GetValue());
				case NumberType.Float:   return (int)((float  )n.GetValue());
				case NumberType.Double:  return (int)((double )n.GetValue());
			}
			throw new InvalidOperationException("The type '" + n.Type.FullName + "' is not a valid number type.");
		}
		public static long ToLong(Number n)
		{
			if (n == null) return default(long);
			switch (TypeToNumberType(n.Type))
			{
				case NumberType.SByte:   return (long)((sbyte  )n.GetValue());
				case NumberType.Short:   return (long)((short  )n.GetValue());
				case NumberType.Int:     return (long)((int    )n.GetValue());
				case NumberType.Long:    return (long)((long   )n.GetValue());
				case NumberType.Byte:    return (long)((byte   )n.GetValue());
				case NumberType.UInt:    return (long)((uint   )n.GetValue());
				case NumberType.UShort:  return (long)((ushort )n.GetValue());
				case NumberType.ULong:   return (long)((ulong  )n.GetValue());
				case NumberType.Decimal: return (long)((decimal)n.GetValue());
				case NumberType.Float:   return (long)((float  )n.GetValue());
				case NumberType.Double:  return (long)((double )n.GetValue());
			}
			throw new InvalidOperationException("The type '" + n.Type.FullName + "' is not a valid number type.");
		}
		public static decimal ToDecimal(Number n)
		{
			if (n == null) return default(decimal);
			switch (TypeToNumberType(n.Type))
			{
				case NumberType.SByte:   return (decimal)((sbyte  )n.GetValue());
				case NumberType.Short:   return (decimal)((short  )n.GetValue());
				case NumberType.Int:     return (decimal)((int    )n.GetValue());
				case NumberType.Long:    return (decimal)((long   )n.GetValue());
				case NumberType.Byte:    return (decimal)((byte   )n.GetValue());
				case NumberType.UInt:    return (decimal)((uint   )n.GetValue());
				case NumberType.UShort:  return (decimal)((ushort )n.GetValue());
				case NumberType.ULong:   return (decimal)((ulong  )n.GetValue());
				case NumberType.Decimal: return (decimal)((decimal)n.GetValue());
				case NumberType.Float:   return (decimal)((float  )n.GetValue());
				case NumberType.Double:  return (decimal)((double )n.GetValue());
			}
			throw new InvalidOperationException("The type '" + n.Type.FullName + "' is not a valid number type.");
		}

		private static Number ApplyFunc(
			Number n,
			Func<sbyte  , sbyte  > ifSByte  ,
			Func<short  , short  > ifShort  ,
			Func<int    , int    > ifInt    ,
			Func<long   , long   > ifLong   ,
			Func<byte   , byte   > ifByte   ,
			Func<uint   , uint   > ifUInt   ,
			Func<ushort , ushort > ifUShort ,
			Func<ulong  , ulong  > ifULong  ,
			Func<decimal, decimal> ifDecimal,
			Func<float  , float  > ifFloat  ,
			Func<double , double > ifDouble 
		) {
			if (n == null) return new Number<double>(null);
			try {
				switch (TypeToNumberType(n.Type))
				{
					case NumberType.SByte:   return ifSByte   == null ? n : new Number<sbyte  >(ifSByte  .Invoke(((sbyte  )n.GetValue())));
					case NumberType.Short:   return ifShort   == null ? n : new Number<short  >(ifShort  .Invoke(((short  )n.GetValue())));
					case NumberType.Int:     return ifInt     == null ? n : new Number<int    >(ifInt    .Invoke(((int    )n.GetValue())));
					case NumberType.Long:    return ifLong    == null ? n : new Number<long   >(ifLong   .Invoke(((long   )n.GetValue())));
					case NumberType.Byte:    return ifByte    == null ? n : new Number<byte   >(ifByte   .Invoke(((byte   )n.GetValue())));
					case NumberType.UInt:    return ifUInt    == null ? n : new Number<uint   >(ifUInt   .Invoke(((uint   )n.GetValue())));
					case NumberType.UShort:  return ifUShort  == null ? n : new Number<ushort >(ifUShort .Invoke(((ushort )n.GetValue())));
					case NumberType.ULong:   return ifULong   == null ? n : new Number<ulong  >(ifULong  .Invoke(((ulong  )n.GetValue())));
					case NumberType.Decimal: return ifDecimal == null ? n : new Number<decimal>(ifDecimal.Invoke(((decimal)n.GetValue())));
					case NumberType.Float:   return ifFloat   == null ? n : new Number<float  >(ifFloat  .Invoke(((float  )n.GetValue())));
					case NumberType.Double:  return ifDouble  == null ? n : new Number<double >(ifDouble .Invoke(((double )n.GetValue())));
				}
				throw new InvalidOperationException("The type '" + n.Type.FullName + "' is not a valid number type.");
			} catch (Exception e) {
				throw new InvalidOperationException("The function could not be applied to type '" + n.Type.FullName + "' with value '" + (n.GetValue() ?? "null") + "'.", e);
			}
		}

		private static Number<TResult> ApplyFunc<TResult>(
			Number n,
			Func<sbyte  , TResult> ifSByte  ,
			Func<short  , TResult> ifShort  ,
			Func<int    , TResult> ifInt    ,
			Func<long   , TResult> ifLong   ,
			Func<byte   , TResult> ifByte   ,
			Func<uint   , TResult> ifUInt   ,
			Func<ushort , TResult> ifUShort ,
			Func<ulong  , TResult> ifULong  ,
			Func<decimal, TResult> ifDecimal,
			Func<float  , TResult> ifFloat  ,
			Func<double , TResult> ifDouble 
		) where TResult : struct
		{
			if (n == null) return new Number<TResult>(null);
			try {
				switch (TypeToNumberType(n.Type))
				{
					case NumberType.SByte:   return new Number<TResult>(ifSByte  ?.Invoke(((sbyte  )n.GetValue())));
					case NumberType.Short:   return new Number<TResult>(ifShort  ?.Invoke(((short  )n.GetValue())));
					case NumberType.Int:     return new Number<TResult>(ifInt    ?.Invoke(((int    )n.GetValue())));
					case NumberType.Long:    return new Number<TResult>(ifLong   ?.Invoke(((long   )n.GetValue())));
					case NumberType.Byte:    return new Number<TResult>(ifByte   ?.Invoke(((byte   )n.GetValue())));
					case NumberType.UInt:    return new Number<TResult>(ifUInt   ?.Invoke(((uint   )n.GetValue())));
					case NumberType.UShort:  return new Number<TResult>(ifUShort ?.Invoke(((ushort )n.GetValue())));
					case NumberType.ULong:   return new Number<TResult>(ifULong  ?.Invoke(((ulong  )n.GetValue())));
					case NumberType.Decimal: return new Number<TResult>(ifDecimal?.Invoke(((decimal)n.GetValue())));
					case NumberType.Float:   return new Number<TResult>(ifFloat  ?.Invoke(((float  )n.GetValue())));
					case NumberType.Double:  return new Number<TResult>(ifDouble ?.Invoke(((double )n.GetValue())));
				}
				throw new InvalidOperationException("The type '" + n.Type.FullName + "' is not a valid number type.");
			} catch (Exception e) {
				throw new InvalidOperationException("The function could not be applied to type '" + n.Type.FullName + "' with value '" + (n.GetValue() ?? "null") + "'.", e);
			}
		}

		private static Number ApplyFuncTwo (
			Number n1,
			Number n2,
			Func<decimal, decimal, decimal> ifDecimal,
			Func<double , double , double > ifDouble
		) {
			if (n1 == null || n2 == null) return new Number<double>(null);
			try {
				if (TypeToNumberTypeCategory(n1.Type) == NumberTypeCategories.Decimal && TypeToNumberTypeCategory(n2.Type) == NumberTypeCategories.Decimal) {
					return new Number<decimal>(ifDecimal?.Invoke(ToDecimal(n1), ToDecimal(n2)));
				} else {
					return new Number<double >(ifDouble ?.Invoke(ToDouble (n1), ToDouble (n2)));
				}
			} catch (Exception e) {
				throw new InvalidOperationException("The function could not be applied to types '" + n1.Type.FullName + "' and '" + n2.Type.FullName + "' with values '" + (n1.GetValue() ?? "null") + "' and '" + (n2.GetValue() ?? "null") + "'.", e);
			}
		}

		private static bool ApplyComparison (
			Number n1,
			Number n2,
			Func<decimal, decimal, bool> ifDecimal,
			Func<double , double , bool> ifDouble,
			bool ifBothNull,
			bool ifOnlyOneNull
		) {
			if (n1 == null && n2 == null) return ifBothNull;
			else if (n1 == null || n2 == null) return ifOnlyOneNull;
			if (ifDouble  == null) throw new ArgumentNullException("ifDouble" , "The argument 'ifDouble' is null.");
			if (ifDecimal == null) throw new ArgumentNullException("ifDecimal", "The argument 'ifDecimal' is null.");
			try {
				if (TypeToNumberTypeCategory(n1.Type) == NumberTypeCategories.Decimal && TypeToNumberTypeCategory(n2.Type) == NumberTypeCategories.Decimal) {
					return ifDecimal.Invoke(ToDecimal(n1), ToDecimal(n2));
				} else {
					return ifDouble .Invoke(ToDouble (n1), ToDouble (n2));
				}
			} catch (Exception e) {
				throw new InvalidOperationException("The function could not be applied to types '" + n1.Type.FullName + "' and '" + n2.Type.FullName + "' with values '" + (n1.GetValue() ?? "null") + "' and '" + (n2.GetValue() ?? "null") + "'", e);
			}
		}
		
		private static T Ret<T>(T val) { return val; }

		public static Number Abs(Number n) {
			//	if (n == null) return new Number<int>(null);
			//	switch (TypeToNumberType(n.Type))
			//	{
			//		case NumberTypes.SByte:   return new Number<T>(((sbyte  )Math.Abs((n.Value as sbyte  ?).GetValueOrDefault())) as T?);
			//		case NumberTypes.Short:   return new Number<T>(((short  )Math.Abs((n.Value as short  ?).GetValueOrDefault())) as T?);
			//		case NumberTypes.Int:     return new Number<T>(((int    )Math.Abs((n.Value as int    ?).GetValueOrDefault())) as T?);
			//		case NumberTypes.Long:    return new Number<T>(((long   )Math.Abs((n.Value as long   ?).GetValueOrDefault())) as T?);
			//		case NumberTypes.Byte:    return n                                                                                  ;
			//		case NumberTypes.UInt:    return n                                                                                  ;
			//		case NumberTypes.UShort:  return n                                                                                  ;
			//		case NumberTypes.ULong:   return n                                                                                  ;
			//		case NumberTypes.Decimal: return new Number<T>(((decimal)Math.Abs((n.Value as decimal?).GetValueOrDefault())) as T?);
			//		case NumberTypes.Float:   return new Number<T>(((float  )Math.Abs((n.Value as float  ?).GetValueOrDefault())) as T?);
			//		case NumberTypes.Double:  return new Number<T>(((double )Math.Abs((n.Value as double ?).GetValueOrDefault())) as T?);
			//	}
			//	throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
			return ApplyFunc(
				n,
				ifSByte  : Math.Abs,
				ifShort  : Math.Abs,
				ifInt    : Math.Abs,
				ifLong   : Math.Abs,
				ifByte   : null,
				ifUInt   : null,
				ifUShort : null,
				ifULong  : null,
				ifDecimal: Math.Abs,
				ifFloat  : Math.Abs,
				ifDouble : Math.Abs
			);
		}
		public static Number<double> Acos(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Acos(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Asin(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Asin(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Atan(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Atan(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Ceiling(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Ceiling(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Cos(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Cos(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Cosh(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Cosh(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> EToThe(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Exp(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Floor(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Floor(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Ln(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Log(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Log10(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Log10(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number Round(Number n) {
			if (n == null) return new Number<double>(null);
			if (TypeToNumberTypeCategory(n.Type) == NumberTypeCategories.Decimal) {
				return new Number<decimal>(Math.Round(ToDecimal(n)));
			} else {
				return new Number<double >(Math.Round(ToDouble (n)));
			}
		}
		public static Number<int> Sign(Number n) {
			return ApplyFunc<int>(
				n,
				ifSByte  : Math.Sign,
				ifShort  : Math.Sign,
				ifInt    : Math.Sign,
				ifLong   : Math.Sign,
				ifByte   : (x) => Math.Sign(x),
				ifUInt   : (x) => Math.Sign(x),
				ifUShort : (x) => Math.Sign(x),
				ifULong  : (x) => Math.Sign((decimal)x),
				ifDecimal: Math.Sign,
				ifFloat  : Math.Sign,
				ifDouble : Math.Sign
			);
		}
		public static Number<double> Sin(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Sin(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Sinh(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Sinh(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Sqrt(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Sqrt(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Tan(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Tan(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Tanh(Number n) {
			if (n == null) return new Number<double>();
			double res = Math.Tanh(ToDouble(n));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<double> Atan2(Number n1, Number n2) {
			if (n1 == null || n2 == null) return new Number<double>();
			//	double n1Double = 0;
			//	double n2Double = 0;
			//	switch (TypeToNumberType(n1.Type))
			//	{
			//		case NumberTypes.SByte:   n1Double =         (n1.Value as sbyte  ?).GetValueOrDefault(); break;
			//		case NumberTypes.Short:   n1Double =         (n1.Value as short  ?).GetValueOrDefault(); break;
			//		case NumberTypes.Int:     n1Double =         (n1.Value as int    ?).GetValueOrDefault(); break;
			//		case NumberTypes.Long:    n1Double =         (n1.Value as long   ?).GetValueOrDefault(); break;
			//		case NumberTypes.Byte:    n1Double =         (n1.Value as byte   ?).GetValueOrDefault(); break;
			//		case NumberTypes.UInt:    n1Double =         (n1.Value as uint   ?).GetValueOrDefault(); break;
			//		case NumberTypes.UShort:  n1Double =         (n1.Value as ushort ?).GetValueOrDefault(); break;
			//		case NumberTypes.ULong:   n1Double =         (n1.Value as ulong  ?).GetValueOrDefault(); break;
			//		case NumberTypes.Decimal: n1Double = (double)(n1.Value as decimal?).GetValueOrDefault(); break;
			//		case NumberTypes.Float:   n1Double =         (n1.Value as float  ?).GetValueOrDefault(); break;
			//		case NumberTypes.Double:  n1Double =         (n1.Value as double ?).GetValueOrDefault(); break;
			//	}
			//	switch (TypeToNumberType(n2.Type))
			//	{
			//		case NumberTypes.SByte:   n1Double =         (n2.Value as sbyte  ?).GetValueOrDefault(); break;
			//		case NumberTypes.Short:   n1Double =         (n2.Value as short  ?).GetValueOrDefault(); break;
			//		case NumberTypes.Int:     n1Double =         (n2.Value as int    ?).GetValueOrDefault(); break;
			//		case NumberTypes.Long:    n1Double =         (n2.Value as long   ?).GetValueOrDefault(); break;
			//		case NumberTypes.Byte:    n1Double =         (n2.Value as byte   ?).GetValueOrDefault(); break;
			//		case NumberTypes.UInt:    n1Double =         (n2.Value as uint   ?).GetValueOrDefault(); break;
			//		case NumberTypes.UShort:  n1Double =         (n2.Value as ushort ?).GetValueOrDefault(); break;
			//		case NumberTypes.ULong:   n1Double =         (n2.Value as ulong  ?).GetValueOrDefault(); break;
			//		case NumberTypes.Decimal: n1Double = (double)(n2.Value as decimal?).GetValueOrDefault(); break;
			//		case NumberTypes.Float:   n1Double =         (n2.Value as float  ?).GetValueOrDefault(); break;
			//		case NumberTypes.Double:  n1Double =         (n2.Value as double ?).GetValueOrDefault(); break;
			//	}
			double res = Math.Atan2(ToDouble(n1), ToDouble(n2));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number<long> WholeDiv(Number n1, Number n2) {
			if (n1 == null || n2 == null) return new Number<long>(null);
			if (!IsNumberType(n1.Type) || IsNumberType(n2.Type)) return new Number<long>(null);
			return new Number<long>((long?)(OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).GetValue()));
		}
		public static Number<double> Log(Number n1, Number n2) {
			if (n1 == null || n2 == null) return new Number<double>();
			double res = Math.Log(ToDouble(n1), ToDouble(n2));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number Max(Number n1, Number n2) {
			//	if (n1 == null || n2 == null) return new Number<int>(null);
			//	switch (TypeToNumberType(typeof(T)))
			//	{
			//		case NumberTypes.SByte:   return new Number<sbyte  >(Math.Max((n1.GetValue() as sbyte  ?).GetValueOrDefault(), (n2.GetValue() as sbyte  ?).GetValueOrDefault()));
			//		case NumberTypes.Short:   return new Number<short  >(Math.Max((n1.GetValue() as short  ?).GetValueOrDefault(), (n2.GetValue() as short  ?).GetValueOrDefault()));
			//		case NumberTypes.Int:     return new Number<int    >(Math.Max((n1.GetValue() as int    ?).GetValueOrDefault(), (n2.GetValue() as int    ?).GetValueOrDefault()));
			//		case NumberTypes.Long:    return new Number<long   >(Math.Max((n1.GetValue() as long   ?).GetValueOrDefault(), (n2.GetValue() as long   ?).GetValueOrDefault()));
			//		case NumberTypes.Byte:    return new Number<byte   >(Math.Max((n1.GetValue() as byte   ?).GetValueOrDefault(), (n2.GetValue() as byte   ?).GetValueOrDefault()));
			//		case NumberTypes.UInt:    return new Number<uint   >(Math.Max((n1.GetValue() as uint   ?).GetValueOrDefault(), (n2.GetValue() as uint   ?).GetValueOrDefault()));
			//		case NumberTypes.UShort:  return new Number<ushort >(Math.Max((n1.GetValue() as ushort ?).GetValueOrDefault(), (n2.GetValue() as ushort ?).GetValueOrDefault()));
			//		case NumberTypes.ULong:   return new Number<ulong  >(Math.Max((n1.GetValue() as ulong  ?).GetValueOrDefault(), (n2.GetValue() as ulong  ?).GetValueOrDefault()));
			//		case NumberTypes.Decimal: return new Number<decimal>(Math.Max((n1.GetValue() as decimal?).GetValueOrDefault(), (n2.GetValue() as decimal?).GetValueOrDefault()));
			//		case NumberTypes.Float:   return new Number<float  >(Math.Max((n1.GetValue() as float  ?).GetValueOrDefault(), (n2.GetValue() as float  ?).GetValueOrDefault()));
			//		case NumberTypes.Double:  return new Number<double >(Math.Max((n1.GetValue() as double ?).GetValueOrDefault(), (n2.GetValue() as double ?).GetValueOrDefault()));
			//	}
			//	throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
			return ApplyFuncTwo(
				n1,
				n2,
				Math.Max,
				Math.Max
			);
		}
		public static Number Min(Number n1, Number n2) {
			//	if (n1 == null || n2 == null) return new Number<double>(null);
			//	NumberTypeCategories ntc1 = TypeToNumberTypeCategory(n1.Type);
			//	NumberTypeCategories ntc2 = TypeToNumberTypeCategory(n2.Type);
			//	if (ntc1 == NumberTypeCategories.Decimal && ntc2 == NumberTypeCategories.Decimal) {
			//		return new Number<decimal>(Math.Min(ToDecimal(n1), ToDecimal(n2)));
			//	} else {
			//		return new Number<double >(Math.Min(ToDouble(n1), ToDouble(n2)));
			//	}
			return ApplyFuncTwo(
				n1,
				n2,
				Math.Min,
				Math.Min
			);
		}
		public static Number<double> Pow(Number n1, Number n2) {
			if (n1 == null || n2 == null) return new Number<double>();
			double res = Math.Pow(ToDouble(n1), ToDouble(n2));
			return new Number<double>(double.IsNaN(res) ? (double?)null : res);
		}
		public static Number Round(Number n1, Number n2) {
			if (n1 == null || n2 == null) return new Number<double>(null);
			if (TypeToNumberTypeCategory(n1.Type) == NumberTypeCategories.Decimal) {
				return new Number<decimal>(Math.Round(ToDecimal(n1), ToInt(n2)));
			} else {
				return new Number<double >(Math.Round(ToDouble (n1), ToInt(n2)));
			}
		}
	}

	internal class Number<T> : Number where T : struct
	{
		public T? Value = null;

		public override Type Type { get { return typeof(T); } }

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if T is not a valid number type</exception>
		public Number()
		{
			if (!IsNumberType(typeof(T))) throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if T is not a valid number type</exception>
		public Number(T? value)
		{
			if (!IsNumberType(typeof(T))) throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
			Value = value;
		}


		public override object GetValue() {
			return Value;
		}

		public override Number Clone()
		{
			return new Number<T>(Value);
		}
		public Number<T> CloneGeneric()
		{
			return new Number<T>(Value);
		}


		/* Now implemented in Number
		/// <summary>
		/// Just adds some null checking stuff (a Number with Value = null == a null Number). Does NOT add the functionality from OPEq();
		/// </summary>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, null) && ReferenceEquals(this.Value, null)) return true;
			return Equals(this, obj);
		}
		/// <summary>
		/// Just adds some null checking stuff (a Number with Value = null == a null Number). Does NOT add the functionality from OPEq();
		/// </summary>
		public static bool operator ==(Number<T> n1, Number<T> n2)
		{
			bool n1Null = ReferenceEquals(n1, null) || ReferenceEquals(n1.Value, null);
			bool n2Null = ReferenceEquals(n2, null) || ReferenceEquals(n2.Value, null);
			if (n1Null && n2Null) return true;
			return Equals(n1, n2);
		}
		/// <summary>
		/// Just adds some null checking stuff (a Number with Value = null == a null Number). Does NOT add the functionality from OPEq();
		/// </summary>
		public static bool operator !=(Number<T> n1, Number<T> n2)
		{
			bool n1Null = ReferenceEquals(n1, null) || ReferenceEquals(n1.Value, null);
			bool n2Null = ReferenceEquals(n2, null) || ReferenceEquals(n2.Value, null);
			if (n1Null && n2Null) return false;
			return !Equals(n1, n2);
		}

		/// <summary>
		/// Calls "(+(n.Value as [type]?)) as T?"
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static Number<T> OPValueOf(Number<T> n) {
			if (n == null) return new Number<T>();
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return new Number<T>((+(n.Value as sbyte  ?)) as T?);
				case NumberTypes.Short:   return new Number<T>((+(n.Value as short  ?)) as T?);
				case NumberTypes.Int:     return new Number<T>((+(n.Value as int    ?)) as T?);
				case NumberTypes.Long:    return new Number<T>((+(n.Value as long   ?)) as T?);
				case NumberTypes.Byte:    return new Number<T>((+(n.Value as byte   ?)) as T?);
				case NumberTypes.UInt:    return new Number<T>((+(n.Value as uint   ?)) as T?);
				case NumberTypes.UShort:  return new Number<T>((+(n.Value as ushort ?)) as T?);
				case NumberTypes.ULong:   return new Number<T>((+(n.Value as ulong  ?)) as T?);
				case NumberTypes.Decimal: return new Number<T>((+(n.Value as decimal?)) as T?);
				case NumberTypes.Float:   return new Number<T>((+(n.Value as float  ?)) as T?);
				case NumberTypes.Double:  return new Number<T>((+(n.Value as double ?)) as T?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "(-(n.Value as [type]?)) as T" for all types except ulong, where it calls "(0 - (n.Value as ulong?)) as T?"
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static Number<T> OPNumNegate(Number<T> n)
		{
			if (n == null) return new Number<T>();
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return new Number<T>(( -(n.Value as sbyte  ?)) as T?);
				case NumberTypes.Short:   return new Number<T>(( -(n.Value as short  ?)) as T?);
				case NumberTypes.Int:     return new Number<T>(( -(n.Value as int    ?)) as T?);
				case NumberTypes.Long:    return new Number<T>(( -(n.Value as long   ?)) as T?);
				case NumberTypes.Byte:    return new Number<T>(( -(n.Value as byte   ?)) as T?);
				case NumberTypes.UInt:    return new Number<T>(( -(n.Value as uint   ?)) as T?);
				case NumberTypes.UShort:  return new Number<T>(( -(n.Value as ushort ?)) as T?);
				case NumberTypes.ULong:   return new Number<T>((0-(n.Value as ulong  ?)) as T?);
				case NumberTypes.Decimal: return new Number<T>(( -(n.Value as decimal?)) as T?);
				case NumberTypes.Float:   return new Number<T>(( -(n.Value as float  ?)) as T?);
				case NumberTypes.Double:  return new Number<T>(( -(n.Value as double ?)) as T?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "((n1.Value as [type]?) + (n2.Value as [type]?)) as T?"
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static Number<T> OPAdd(Number<T> n1, Number<T> n2)
		{
			if (n1 == null || n2 == null) return new Number<T>();
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return new Number<T>(((n1.Value as sbyte  ?) + (n2.Value as sbyte  ?)) as T?);
				case NumberTypes.Short:   return new Number<T>(((n1.Value as short  ?) + (n2.Value as short  ?)) as T?);
				case NumberTypes.Int:     return new Number<T>(((n1.Value as int    ?) + (n2.Value as int    ?)) as T?);
				case NumberTypes.Long:    return new Number<T>(((n1.Value as long   ?) + (n2.Value as long   ?)) as T?);
				case NumberTypes.Byte:    return new Number<T>(((n1.Value as byte   ?) + (n2.Value as byte   ?)) as T?);
				case NumberTypes.UInt:    return new Number<T>(((n1.Value as uint   ?) + (n2.Value as uint   ?)) as T?);
				case NumberTypes.UShort:  return new Number<T>(((n1.Value as ushort ?) + (n2.Value as ushort ?)) as T?);
				case NumberTypes.ULong:   return new Number<T>(((n1.Value as ulong  ?) + (n2.Value as ulong  ?)) as T?);
				case NumberTypes.Decimal: return new Number<T>(((n1.Value as decimal?) + (n2.Value as decimal?)) as T?);
				case NumberTypes.Float:   return new Number<T>(((n1.Value as float  ?) + (n2.Value as float  ?)) as T?);
				case NumberTypes.Double:  return new Number<T>(((n1.Value as double ?) + (n2.Value as double ?)) as T?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "((n1.Value as [type]?) - (n2.Value as [type]?)) as T?"
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static Number<T> OPSubtract(Number<T> n1, Number<T> n2)
		{
			if (n1 == null || n2 == null) return new Number<T>();
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return new Number<T>(((n1.Value as sbyte  ?) - (n2.Value as sbyte  ?)) as T?);
				case NumberTypes.Short:   return new Number<T>(((n1.Value as short  ?) - (n2.Value as short  ?)) as T?);
				case NumberTypes.Int:     return new Number<T>(((n1.Value as int    ?) - (n2.Value as int    ?)) as T?);
				case NumberTypes.Long:    return new Number<T>(((n1.Value as long   ?) - (n2.Value as long   ?)) as T?);
				case NumberTypes.Byte:    return new Number<T>(((n1.Value as byte   ?) - (n2.Value as byte   ?)) as T?);
				case NumberTypes.UInt:    return new Number<T>(((n1.Value as uint   ?) - (n2.Value as uint   ?)) as T?);
				case NumberTypes.UShort:  return new Number<T>(((n1.Value as ushort ?) - (n2.Value as ushort ?)) as T?);
				case NumberTypes.ULong:   return new Number<T>(((n1.Value as ulong  ?) - (n2.Value as ulong  ?)) as T?);
				case NumberTypes.Decimal: return new Number<T>(((n1.Value as decimal?) - (n2.Value as decimal?)) as T?);
				case NumberTypes.Float:   return new Number<T>(((n1.Value as float  ?) - (n2.Value as float  ?)) as T?);
				case NumberTypes.Double:  return new Number<T>(((n1.Value as double ?) - (n2.Value as double ?)) as T?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "((n1.Value as [type]?) * (n2.Value as [type]?)) as T?"
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static Number<T> OPMultiply(Number<T> n1, Number<T> n2)
		{
			if (n1 == null || n2 == null) return new Number<T>();
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return new Number<T>(((n1.Value as sbyte  ?) * (n2.Value as sbyte  ?)) as T?);
				case NumberTypes.Short:   return new Number<T>(((n1.Value as short  ?) * (n2.Value as short  ?)) as T?);
				case NumberTypes.Int:     return new Number<T>(((n1.Value as int    ?) * (n2.Value as int    ?)) as T?);
				case NumberTypes.Long:    return new Number<T>(((n1.Value as long   ?) * (n2.Value as long   ?)) as T?);
				case NumberTypes.Byte:    return new Number<T>(((n1.Value as byte   ?) * (n2.Value as byte   ?)) as T?);
				case NumberTypes.UInt:    return new Number<T>(((n1.Value as uint   ?) * (n2.Value as uint   ?)) as T?);
				case NumberTypes.UShort:  return new Number<T>(((n1.Value as ushort ?) * (n2.Value as ushort ?)) as T?);
				case NumberTypes.ULong:   return new Number<T>(((n1.Value as ulong  ?) * (n2.Value as ulong  ?)) as T?);
				case NumberTypes.Decimal: return new Number<T>(((n1.Value as decimal?) * (n2.Value as decimal?)) as T?);
				case NumberTypes.Float:   return new Number<T>(((n1.Value as float  ?) * (n2.Value as float  ?)) as T?);
				case NumberTypes.Double:  return new Number<T>(((n1.Value as double ?) * (n2.Value as double ?)) as T?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "((n1.Value as [type]?) / (n2.Value as [type]?)) as T?"
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static Number<T> OPDivide(Number<T> n1, Number<T> n2)
		{
			if (n1 == null || n2 == null) return new Number<T>();
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return new Number<T>(((n1.Value as sbyte  ?) / (n2.Value as sbyte  ?)) as T?);
				case NumberTypes.Short:   return new Number<T>(((n1.Value as short  ?) / (n2.Value as short  ?)) as T?);
				case NumberTypes.Int:     return new Number<T>(((n1.Value as int    ?) / (n2.Value as int    ?)) as T?);
				case NumberTypes.Long:    return new Number<T>(((n1.Value as long   ?) / (n2.Value as long   ?)) as T?);
				case NumberTypes.Byte:    return new Number<T>(((n1.Value as byte   ?) / (n2.Value as byte   ?)) as T?);
				case NumberTypes.UInt:    return new Number<T>(((n1.Value as uint   ?) / (n2.Value as uint   ?)) as T?);
				case NumberTypes.UShort:  return new Number<T>(((n1.Value as ushort ?) / (n2.Value as ushort ?)) as T?);
				case NumberTypes.ULong:   return new Number<T>(((n1.Value as ulong  ?) / (n2.Value as ulong  ?)) as T?);
				case NumberTypes.Decimal: return new Number<T>(((n1.Value as decimal?) / (n2.Value as decimal?)) as T?);
				case NumberTypes.Float:   return new Number<T>(((n1.Value as float  ?) / (n2.Value as float  ?)) as T?);
				case NumberTypes.Double:  return new Number<T>(((n1.Value as double ?) / (n2.Value as double ?)) as T?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "((n1.Value as [type]?) % (n2.Value as [type]?)) as T?"
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static Number<T> OPModulus(Number<T> n1, Number<T> n2)
		{
			if (n1 == null || n2 == null) return new Number<T>();
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return new Number<T>(((n1.Value as sbyte  ?) % (n2.Value as sbyte  ?)) as T?);
				case NumberTypes.Short:   return new Number<T>(((n1.Value as short  ?) % (n2.Value as short  ?)) as T?);
				case NumberTypes.Int:     return new Number<T>(((n1.Value as int    ?) % (n2.Value as int    ?)) as T?);
				case NumberTypes.Long:    return new Number<T>(((n1.Value as long   ?) % (n2.Value as long   ?)) as T?);
				case NumberTypes.Byte:    return new Number<T>(((n1.Value as byte   ?) % (n2.Value as byte   ?)) as T?);
				case NumberTypes.UInt:    return new Number<T>(((n1.Value as uint   ?) % (n2.Value as uint   ?)) as T?);
				case NumberTypes.UShort:  return new Number<T>(((n1.Value as ushort ?) % (n2.Value as ushort ?)) as T?);
				case NumberTypes.ULong:   return new Number<T>(((n1.Value as ulong  ?) % (n2.Value as ulong  ?)) as T?);
				case NumberTypes.Decimal: return new Number<T>(((n1.Value as decimal?) % (n2.Value as decimal?)) as T?);
				case NumberTypes.Float:   return new Number<T>(((n1.Value as float  ?) % (n2.Value as float  ?)) as T?);
				case NumberTypes.Double:  return new Number<T>(((n1.Value as double ?) % (n2.Value as double ?)) as T?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "(n1.Value as [type]?) == (n2.Value as [type]?)"
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPEq(Number<T> n1, Number<T> n2)
		{
			if (n1 == null && n2 == null) return true;
			else if (n1 == null || n2 == null) return false;
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return (n1.Value as sbyte  ?) == (n2.Value as sbyte  ?);
				case NumberTypes.Short:   return (n1.Value as short  ?) == (n2.Value as short  ?);
				case NumberTypes.Int:     return (n1.Value as int    ?) == (n2.Value as int    ?);
				case NumberTypes.Long:    return (n1.Value as long   ?) == (n2.Value as long   ?);
				case NumberTypes.Byte:    return (n1.Value as byte   ?) == (n2.Value as byte   ?);
				case NumberTypes.UInt:    return (n1.Value as uint   ?) == (n2.Value as uint   ?);
				case NumberTypes.UShort:  return (n1.Value as ushort ?) == (n2.Value as ushort ?);
				case NumberTypes.ULong:   return (n1.Value as ulong  ?) == (n2.Value as ulong  ?);
				case NumberTypes.Decimal: return (n1.Value as decimal?) == (n2.Value as decimal?);
				case NumberTypes.Float:   return (n1.Value as float  ?) == (n2.Value as float  ?);
				case NumberTypes.Double:  return (n1.Value as double ?) == (n2.Value as double ?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "(n1.Value as [type]?) != (n2.Value as [type]?)"
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPNeq(Number<T> n1, Number<T> n2)
		{
			if (n1 == null && n2 == null) return false;
			else if (n1 == null || n2 == null) return true;
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return (n1.Value as sbyte  ?) != (n2.Value as sbyte  ?);
				case NumberTypes.Short:   return (n1.Value as short  ?) != (n2.Value as short  ?);
				case NumberTypes.Int:     return (n1.Value as int    ?) != (n2.Value as int    ?);
				case NumberTypes.Long:    return (n1.Value as long   ?) != (n2.Value as long   ?);
				case NumberTypes.Byte:    return (n1.Value as byte   ?) != (n2.Value as byte   ?);
				case NumberTypes.UInt:    return (n1.Value as uint   ?) != (n2.Value as uint   ?);
				case NumberTypes.UShort:  return (n1.Value as ushort ?) != (n2.Value as ushort ?);
				case NumberTypes.ULong:   return (n1.Value as ulong  ?) != (n2.Value as ulong  ?);
				case NumberTypes.Decimal: return (n1.Value as decimal?) != (n2.Value as decimal?);
				case NumberTypes.Float:   return (n1.Value as float  ?) != (n2.Value as float  ?);
				case NumberTypes.Double:  return (n1.Value as double ?) != (n2.Value as double ?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "(n1.Value as [type]?) &lt; (n2.Value as [type]?)"
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPLss(Number<T> n1, Number<T> n2)
		{
			if (n1 == null && n2 == null) return true;
			else if (n1 == null || n2 == null) return false;
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return (n1.Value as sbyte  ?) < (n2.Value as sbyte  ?);
				case NumberTypes.Short:   return (n1.Value as short  ?) < (n2.Value as short  ?);
				case NumberTypes.Int:     return (n1.Value as int    ?) < (n2.Value as int    ?);
				case NumberTypes.Long:    return (n1.Value as long   ?) < (n2.Value as long   ?);
				case NumberTypes.Byte:    return (n1.Value as byte   ?) < (n2.Value as byte   ?);
				case NumberTypes.UInt:    return (n1.Value as uint   ?) < (n2.Value as uint   ?);
				case NumberTypes.UShort:  return (n1.Value as ushort ?) < (n2.Value as ushort ?);
				case NumberTypes.ULong:   return (n1.Value as ulong  ?) < (n2.Value as ulong  ?);
				case NumberTypes.Decimal: return (n1.Value as decimal?) < (n2.Value as decimal?);
				case NumberTypes.Float:   return (n1.Value as float  ?) < (n2.Value as float  ?);
				case NumberTypes.Double:  return (n1.Value as double ?) < (n2.Value as double ?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "(n1.Value as [type]?) &gt; (n2.Value as [type]?)"
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPGtr(Number<T> n1, Number<T> n2)
		{
			if (n1 == null && n2 == null) return true;
			else if (n1 == null || n2 == null) return false;
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return (n1.Value as sbyte  ?) > (n2.Value as sbyte  ?);
				case NumberTypes.Short:   return (n1.Value as short  ?) > (n2.Value as short  ?);
				case NumberTypes.Int:     return (n1.Value as int    ?) > (n2.Value as int    ?);
				case NumberTypes.Long:    return (n1.Value as long   ?) > (n2.Value as long   ?);
				case NumberTypes.Byte:    return (n1.Value as byte   ?) > (n2.Value as byte   ?);
				case NumberTypes.UInt:    return (n1.Value as uint   ?) > (n2.Value as uint   ?);
				case NumberTypes.UShort:  return (n1.Value as ushort ?) > (n2.Value as ushort ?);
				case NumberTypes.ULong:   return (n1.Value as ulong  ?) > (n2.Value as ulong  ?);
				case NumberTypes.Decimal: return (n1.Value as decimal?) > (n2.Value as decimal?);
				case NumberTypes.Float:   return (n1.Value as float  ?) > (n2.Value as float  ?);
				case NumberTypes.Double:  return (n1.Value as double ?) > (n2.Value as double ?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "(n1.Value as [type]?) &lt;= (n2.Value as [type]?)"
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPLeq(Number<T> n1, Number<T> n2)
		{
			if (n1 == null && n2 == null) return true;
			else if (n1 == null || n2 == null) return false;
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return (n1.Value as sbyte  ?) <= (n2.Value as sbyte  ?);
				case NumberTypes.Short:   return (n1.Value as short  ?) <= (n2.Value as short  ?);
				case NumberTypes.Int:     return (n1.Value as int    ?) <= (n2.Value as int    ?);
				case NumberTypes.Long:    return (n1.Value as long   ?) <= (n2.Value as long   ?);
				case NumberTypes.Byte:    return (n1.Value as byte   ?) <= (n2.Value as byte   ?);
				case NumberTypes.UInt:    return (n1.Value as uint   ?) <= (n2.Value as uint   ?);
				case NumberTypes.UShort:  return (n1.Value as ushort ?) <= (n2.Value as ushort ?);
				case NumberTypes.ULong:   return (n1.Value as ulong  ?) <= (n2.Value as ulong  ?);
				case NumberTypes.Decimal: return (n1.Value as decimal?) <= (n2.Value as decimal?);
				case NumberTypes.Float:   return (n1.Value as float  ?) <= (n2.Value as float  ?);
				case NumberTypes.Double:  return (n1.Value as double ?) <= (n2.Value as double ?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		/// <summary>
		/// Calls "(n1.Value as [type]?) &gt;= (n2.Value as [type]?)"
		/// </summary>
		/// <param name="n1"></param>
		/// <param name="n2"></param>
		/// <returns></returns>
		public static bool OPGeq(Number<T> n1, Number<T> n2)
		{
			if (n1 == null && n2 == null) return true;
			else if (n1 == null || n2 == null) return false;
			switch (TypeToNumberType(typeof(T))) {
				case NumberTypes.SByte:   return (n1.Value as sbyte  ?) >= (n2.Value as sbyte  ?);
				case NumberTypes.Short:   return (n1.Value as short  ?) >= (n2.Value as short  ?);
				case NumberTypes.Int:     return (n1.Value as int    ?) >= (n2.Value as int    ?);
				case NumberTypes.Long:    return (n1.Value as long   ?) >= (n2.Value as long   ?);
				case NumberTypes.Byte:    return (n1.Value as byte   ?) >= (n2.Value as byte   ?);
				case NumberTypes.UInt:    return (n1.Value as uint   ?) >= (n2.Value as uint   ?);
				case NumberTypes.UShort:  return (n1.Value as ushort ?) >= (n2.Value as ushort ?);
				case NumberTypes.ULong:   return (n1.Value as ulong  ?) >= (n2.Value as ulong  ?);
				case NumberTypes.Decimal: return (n1.Value as decimal?) >= (n2.Value as decimal?);
				case NumberTypes.Float:   return (n1.Value as float  ?) >= (n2.Value as float  ?);
				case NumberTypes.Double:  return (n1.Value as double ?) >= (n2.Value as double ?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}




		public static Number<T> Abs(Number<T> n) {
			if (n == null) return new Number<T>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<T>(((sbyte  )Math.Abs((n.Value as sbyte  ?).GetValueOrDefault())) as T?);
				case NumberTypes.Short:   return new Number<T>(((short  )Math.Abs((n.Value as short  ?).GetValueOrDefault())) as T?);
				case NumberTypes.Int:     return new Number<T>(((int    )Math.Abs((n.Value as int    ?).GetValueOrDefault())) as T?);
				case NumberTypes.Long:    return new Number<T>(((long   )Math.Abs((n.Value as long   ?).GetValueOrDefault())) as T?);
				case NumberTypes.Byte:    return n                                                                                  ;
				case NumberTypes.UInt:    return n                                                                                  ;
				case NumberTypes.UShort:  return n                                                                                  ;
				case NumberTypes.ULong:   return n                                                                                  ;
				case NumberTypes.Decimal: return new Number<T>(((decimal)Math.Abs((n.Value as decimal?).GetValueOrDefault())) as T?);
				case NumberTypes.Float:   return new Number<T>(((float  )Math.Abs((n.Value as float  ?).GetValueOrDefault())) as T?);
				case NumberTypes.Double:  return new Number<T>(((double )Math.Abs((n.Value as double ?).GetValueOrDefault())) as T?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Acos(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Acos(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Acos(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Acos(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Acos(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Acos(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Acos(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Acos(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Acos(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Acos((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Acos(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Acos(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Asin(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Asin(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Asin(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Asin(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Asin(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Asin(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Asin(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Asin(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Asin(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Asin((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Asin(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Asin(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Atan(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Atan(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Atan(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Atan(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Atan(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Atan(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Atan(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Atan(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Atan(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Atan((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Atan(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Atan(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Ceiling(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Ceiling((double)(n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Ceiling((double)(n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Ceiling((double)(n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Ceiling((double)(n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Ceiling((double)(n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Ceiling((double)(n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Ceiling((double)(n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Ceiling((double)(n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>((double)Math.Ceiling((n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Ceiling(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Ceiling(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Cos(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Cos(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Cos(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Cos(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Cos(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Cos(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Cos(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Cos(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Cos(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Cos((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Cos(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Cos(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Cosh(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Cosh(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Cosh(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Cosh(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Cosh(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Cosh(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Cosh(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Cosh(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Cosh(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Cosh((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Cosh(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Cosh(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Exp(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Exp(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Exp(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Exp(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Exp(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Exp(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Exp(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Exp(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Exp(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Exp((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Exp(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Exp(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Floor(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Floor((double)(n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Floor((double)(n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Floor((double)(n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Floor((double)(n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Floor((double)(n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Floor((double)(n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Floor((double)(n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Floor((double)(n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>((double)Math.Floor((n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Floor(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Floor(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Ln(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Log(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Log(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Log(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Log(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Log(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Log(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Log(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Log(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Log((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Log(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Log(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Log10(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Log10(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Log10(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Log10(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Log10(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Log10(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Log10(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Log10(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Log10(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Log10((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Log10(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Log10(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Round(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Round((double)(n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Round((double)(n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Round((double)(n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Round((double)(n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Round((double)(n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Round((double)(n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Round((double)(n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Round((double)(n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>((double)Math.Round((n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Round(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Round(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<int> Sign(Number<T> n) {
			if (n == null) return new Number<int>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<int>(Math.Sign(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<int>(Math.Sign(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<int>(Math.Sign(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<int>(Math.Sign(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<int>(Math.Sign(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<int>(Math.Sign(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<int>(Math.Sign(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<int>(Math.Sign((float )(n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<int>(Math.Sign(        (n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<int>(Math.Sign(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<int>(Math.Sign(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Sin(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Sin(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Sin(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Sin(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Sin(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Sin(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Sin(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Sin(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Sin(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Sin((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Sin(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Sin(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Sinh(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Sinh(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Sinh(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Sinh(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Sinh(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Sinh(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Sinh(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Sinh(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Sinh(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Sinh((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Sinh(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Sinh(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Sqrt(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Sqrt(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Sqrt(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Sqrt(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Sqrt(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Sqrt(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Sqrt(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Sqrt(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Sqrt(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Sqrt((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Sqrt(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Sqrt(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Tan(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Tan(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Tan(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Tan(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Tan(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Tan(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Tan(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Tan(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Tan(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Tan((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Tan(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Tan(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Tanh(Number<T> n) {
			if (n == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Tanh(        (n.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Tanh(        (n.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Tanh(        (n.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Tanh(        (n.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Tanh(        (n.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Tanh(        (n.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Tanh(        (n.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Tanh(        (n.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Tanh((double)(n.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Tanh(        (n.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Tanh(        (n.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Atan2(Number<T> n1, Number<T> n2) {
			if (n1 == null || n2 == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Atan2(        (n1.Value as sbyte  ?).GetValueOrDefault(),         (n2.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Atan2(        (n1.Value as short  ?).GetValueOrDefault(),         (n2.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Atan2(        (n1.Value as int    ?).GetValueOrDefault(),         (n2.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Atan2(        (n1.Value as long   ?).GetValueOrDefault(),         (n2.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Atan2(        (n1.Value as byte   ?).GetValueOrDefault(),         (n2.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Atan2(        (n1.Value as uint   ?).GetValueOrDefault(),         (n2.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Atan2(        (n1.Value as ushort ?).GetValueOrDefault(),         (n2.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Atan2(        (n1.Value as ulong  ?).GetValueOrDefault(),         (n2.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Atan2((double)(n1.Value as decimal?).GetValueOrDefault(), (double)(n2.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Atan2(        (n1.Value as float  ?).GetValueOrDefault(),         (n2.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Atan2(        (n1.Value as double ?).GetValueOrDefault(),         (n2.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<long> WholeDiv(Number<T> n1, Number<T> n2) {
			if (n1 == null || n2 == null) return new Number<long>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<long>(       (OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).Value as sbyte  ?));
				case NumberTypes.Short:   return new Number<long>(       (OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).Value as short  ?));
				case NumberTypes.Int:     return new Number<long>(       (OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).Value as int    ?));
				case NumberTypes.Long:    return new Number<long>(       (OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).Value as long   ?));
				case NumberTypes.Byte:    return new Number<long>(       (OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).Value as byte   ?));
				case NumberTypes.UInt:    return new Number<long>(       (OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).Value as uint   ?));
				case NumberTypes.UShort:  return new Number<long>(       (OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).Value as ushort ?));
				case NumberTypes.ULong:   return new Number<long>((long?)(OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).Value as ulong  ?));
				case NumberTypes.Decimal: return new Number<long>((long?)(OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).Value as decimal?));
				case NumberTypes.Float:   return new Number<long>((long?)(OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).Value as float  ?));
				case NumberTypes.Double:  return new Number<long>((long?)(OPDivide(OPSubtract(n1, OPModulus(n1, n2)), n2).Value as double ?));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Log(Number<T> n1, Number<T> n2) {
			if (n1 == null || n2 == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Log(        (n1.Value as sbyte  ?).GetValueOrDefault(),         (n2.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Log(        (n1.Value as short  ?).GetValueOrDefault(),         (n2.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Log(        (n1.Value as int    ?).GetValueOrDefault(),         (n2.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Log(        (n1.Value as long   ?).GetValueOrDefault(),         (n2.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Log(        (n1.Value as byte   ?).GetValueOrDefault(),         (n2.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Log(        (n1.Value as uint   ?).GetValueOrDefault(),         (n2.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Log(        (n1.Value as ushort ?).GetValueOrDefault(),         (n2.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Log(        (n1.Value as ulong  ?).GetValueOrDefault(),         (n2.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Log((double)(n1.Value as decimal?).GetValueOrDefault(), (double)(n2.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Log(        (n1.Value as float  ?).GetValueOrDefault(),         (n2.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Log(        (n1.Value as double ?).GetValueOrDefault(),         (n2.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<T> Max(Number<T> n1, Number<T> n2) {
			if (n1 == null || n2 == null) return new Number<T>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<T>(Math.Max(        (n1.Value as sbyte  ?).GetValueOrDefault(),         (n2.Value as sbyte  ?).GetValueOrDefault()) as T?);
				case NumberTypes.Short:   return new Number<T>(Math.Max(        (n1.Value as short  ?).GetValueOrDefault(),         (n2.Value as short  ?).GetValueOrDefault()) as T?);
				case NumberTypes.Int:     return new Number<T>(Math.Max(        (n1.Value as int    ?).GetValueOrDefault(),         (n2.Value as int    ?).GetValueOrDefault()) as T?);
				case NumberTypes.Long:    return new Number<T>(Math.Max(        (n1.Value as long   ?).GetValueOrDefault(),         (n2.Value as long   ?).GetValueOrDefault()) as T?);
				case NumberTypes.Byte:    return new Number<T>(Math.Max(        (n1.Value as byte   ?).GetValueOrDefault(),         (n2.Value as byte   ?).GetValueOrDefault()) as T?);
				case NumberTypes.UInt:    return new Number<T>(Math.Max(        (n1.Value as uint   ?).GetValueOrDefault(),         (n2.Value as uint   ?).GetValueOrDefault()) as T?);
				case NumberTypes.UShort:  return new Number<T>(Math.Max(        (n1.Value as ushort ?).GetValueOrDefault(),         (n2.Value as ushort ?).GetValueOrDefault()) as T?);
				case NumberTypes.ULong:   return new Number<T>(Math.Max(        (n1.Value as ulong  ?).GetValueOrDefault(),         (n2.Value as ulong  ?).GetValueOrDefault()) as T?);
				case NumberTypes.Decimal: return new Number<T>(Math.Max((double)(n1.Value as decimal?).GetValueOrDefault(), (double)(n2.Value as decimal?).GetValueOrDefault()) as T?);
				case NumberTypes.Float:   return new Number<T>(Math.Max(        (n1.Value as float  ?).GetValueOrDefault(),         (n2.Value as float  ?).GetValueOrDefault()) as T?);
				case NumberTypes.Double:  return new Number<T>(Math.Max(        (n1.Value as double ?).GetValueOrDefault(),         (n2.Value as double ?).GetValueOrDefault()) as T?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<T> Min(Number<T> n1, Number<T> n2) {
			if (n1 == null || n2 == null) return new Number<T>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<T>(Math.Min(        (n1.Value as sbyte  ?).GetValueOrDefault(),         (n2.Value as sbyte  ?).GetValueOrDefault()) as T?);
				case NumberTypes.Short:   return new Number<T>(Math.Min(        (n1.Value as short  ?).GetValueOrDefault(),         (n2.Value as short  ?).GetValueOrDefault()) as T?);
				case NumberTypes.Int:     return new Number<T>(Math.Min(        (n1.Value as int    ?).GetValueOrDefault(),         (n2.Value as int    ?).GetValueOrDefault()) as T?);
				case NumberTypes.Long:    return new Number<T>(Math.Min(        (n1.Value as long   ?).GetValueOrDefault(),         (n2.Value as long   ?).GetValueOrDefault()) as T?);
				case NumberTypes.Byte:    return new Number<T>(Math.Min(        (n1.Value as byte   ?).GetValueOrDefault(),         (n2.Value as byte   ?).GetValueOrDefault()) as T?);
				case NumberTypes.UInt:    return new Number<T>(Math.Min(        (n1.Value as uint   ?).GetValueOrDefault(),         (n2.Value as uint   ?).GetValueOrDefault()) as T?);
				case NumberTypes.UShort:  return new Number<T>(Math.Min(        (n1.Value as ushort ?).GetValueOrDefault(),         (n2.Value as ushort ?).GetValueOrDefault()) as T?);
				case NumberTypes.ULong:   return new Number<T>(Math.Min(        (n1.Value as ulong  ?).GetValueOrDefault(),         (n2.Value as ulong  ?).GetValueOrDefault()) as T?);
				case NumberTypes.Decimal: return new Number<T>(Math.Min((double)(n1.Value as decimal?).GetValueOrDefault(), (double)(n2.Value as decimal?).GetValueOrDefault()) as T?);
				case NumberTypes.Float:   return new Number<T>(Math.Min(        (n1.Value as float  ?).GetValueOrDefault(),         (n2.Value as float  ?).GetValueOrDefault()) as T?);
				case NumberTypes.Double:  return new Number<T>(Math.Min(        (n1.Value as double ?).GetValueOrDefault(),         (n2.Value as double ?).GetValueOrDefault()) as T?);
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Pow(Number<T> n1, Number<T> n2) {
			if (n1 == null || n2 == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Pow(        (n1.Value as sbyte  ?).GetValueOrDefault(),         (n2.Value as sbyte  ?).GetValueOrDefault()));
				case NumberTypes.Short:   return new Number<double>(Math.Pow(        (n1.Value as short  ?).GetValueOrDefault(),         (n2.Value as short  ?).GetValueOrDefault()));
				case NumberTypes.Int:     return new Number<double>(Math.Pow(        (n1.Value as int    ?).GetValueOrDefault(),         (n2.Value as int    ?).GetValueOrDefault()));
				case NumberTypes.Long:    return new Number<double>(Math.Pow(        (n1.Value as long   ?).GetValueOrDefault(),         (n2.Value as long   ?).GetValueOrDefault()));
				case NumberTypes.Byte:    return new Number<double>(Math.Pow(        (n1.Value as byte   ?).GetValueOrDefault(),         (n2.Value as byte   ?).GetValueOrDefault()));
				case NumberTypes.UInt:    return new Number<double>(Math.Pow(        (n1.Value as uint   ?).GetValueOrDefault(),         (n2.Value as uint   ?).GetValueOrDefault()));
				case NumberTypes.UShort:  return new Number<double>(Math.Pow(        (n1.Value as ushort ?).GetValueOrDefault(),         (n2.Value as ushort ?).GetValueOrDefault()));
				case NumberTypes.ULong:   return new Number<double>(Math.Pow(        (n1.Value as ulong  ?).GetValueOrDefault(),         (n2.Value as ulong  ?).GetValueOrDefault()));
				case NumberTypes.Decimal: return new Number<double>(Math.Pow((double)(n1.Value as decimal?).GetValueOrDefault(), (double)(n2.Value as decimal?).GetValueOrDefault()));
				case NumberTypes.Float:   return new Number<double>(Math.Pow(        (n1.Value as float  ?).GetValueOrDefault(),         (n2.Value as float  ?).GetValueOrDefault()));
				case NumberTypes.Double:  return new Number<double>(Math.Pow(        (n1.Value as double ?).GetValueOrDefault(),         (n2.Value as double ?).GetValueOrDefault()));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		public static Number<double> Round(Number<T> n1, Number<T> n2) {
			if (n1 == null || n2 == null) return new Number<double>();
			switch (TypeToNumberType(typeof(T)))
			{
				case NumberTypes.SByte:   return new Number<double>(Math.Round((double)(n1.Value as sbyte  ?).GetValueOrDefault(), (int)(double)Floor(n2).Value));
				case NumberTypes.Short:   return new Number<double>(Math.Round((double)(n1.Value as short  ?).GetValueOrDefault(), (int)(double)Floor(n2).Value));
				case NumberTypes.Int:     return new Number<double>(Math.Round((double)(n1.Value as int    ?).GetValueOrDefault(), (int)(double)Floor(n2).Value));
				case NumberTypes.Long:    return new Number<double>(Math.Round((double)(n1.Value as long   ?).GetValueOrDefault(), (int)(double)Floor(n2).Value));
				case NumberTypes.Byte:    return new Number<double>(Math.Round((double)(n1.Value as byte   ?).GetValueOrDefault(), (int)(double)Floor(n2).Value));
				case NumberTypes.UInt:    return new Number<double>(Math.Round((double)(n1.Value as uint   ?).GetValueOrDefault(), (int)(double)Floor(n2).Value));
				case NumberTypes.UShort:  return new Number<double>(Math.Round((double)(n1.Value as ushort ?).GetValueOrDefault(), (int)(double)Floor(n2).Value));
				case NumberTypes.ULong:   return new Number<double>(Math.Round((double)(n1.Value as ulong  ?).GetValueOrDefault(), (int)(double)Floor(n2).Value));
				case NumberTypes.Decimal: return new Number<double>((double)Math.Round((n1.Value as decimal?).GetValueOrDefault(), (int)(double)Floor(n2).Value));
				case NumberTypes.Float:   return new Number<double>(Math.Round(        (n1.Value as float  ?).GetValueOrDefault(), (int)(double)Floor(n2).Value));
				case NumberTypes.Double:  return new Number<double>(Math.Round(        (n1.Value as double ?).GetValueOrDefault(), (int)(double)Floor(n2).Value));
			}
			throw new InvalidOperationException("The type '" + typeof(T).FullName + "' is not a valid number type.");
		}
		*/
	}
}
