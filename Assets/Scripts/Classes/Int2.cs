using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using UnityEngine;

namespace Sperlich.Vectors {
	[System.Serializable]
	[TypeConverter(typeof(Int2Converter))]
	[DataContract]
	public struct Int2 : System.IEquatable<Int2> {
		[DataMember] public int x;
		[DataMember] public int y;

		public Int2(int _x, int _y) {
			x = _x;
			y = _y;
		}

		public Int2(Vector2 index) {
			x = Mathf.RoundToInt(index.x);
			y = Mathf.RoundToInt(index.y);
		}

		public Vector2 Vector2 => new Vector2(x, y);

		public static int Distance(Int2 a, Int2 b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

		public static Int2 operator +(Int2 item1, Int2 item2) => new Int2(item1.x + item2.x, item1.y + item2.y);
		public static Int2 operator -(Int2 item1, Int2 item2) => new Int2(item1.x - item2.x, item1.y - item2.y);
		public static Int2 operator *(Int2 item1, Int2 item2) => new Int2(item1.x * item2.x, item1.y * item2.y);
		public static Int2 operator *(Int2 item1, int multiplier) => new Int2(item1.x * multiplier, item1.y + multiplier);
		public static Int2 operator *(int multiplier, Int2 item1) => new Int2(item1.x * multiplier, item1.y + multiplier);
		public static Int2 operator /(Int2 item1, Int2 item2) => new Int2(item1.x / item2.x, item1.y / item2.y);
		public static Int2 operator /(Int2 item1, int multiplier) => new Int2(item1.x / multiplier, item1.y / multiplier);
		public static Int2 operator /(int multiplier, Int2 item1) => new Int2(item1.x / multiplier, item1.y / multiplier);
		public static bool operator ==(Int2 item1, Int2 item2) => item1.x == item2.x && item1.y == item2.y;
		public static bool operator !=(Int2 item1, Int2 item2) => !(item1 == item2);



		override
		public string ToString() => "[" + x + "," + y + "]";

		public static string Int2ToString(int x, int y) => "[" + x + "," + y + "]";

		public override bool Equals(object obj) => obj is Int2 @int && Equals(@int);

		public bool Equals(Int2 other) => x == other.x && y == other.y;

		public override int GetHashCode() {
			int hashCode = 1502939027;
			hashCode = hashCode * -1521134295 + x.GetHashCode();
			hashCode = hashCode * -1521134295 + y.GetHashCode();
			return hashCode;
		}
	}

	class Int2Converter : TypeConverter {
		public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType) {
			if(sourceType == typeof(string))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType) {
			if(destinationType == typeof(string))
				return true;
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			if(value is string strValue) {
				if(strValue.Length >= 5) {
					string[] elements = strValue.Substring(1, strValue.Length - 2).Split(',');
					if(elements.Length == 2) {
						if(int.TryParse(elements[0], NumberStyles.Integer, culture.NumberFormat, out int x)) {
							if(int.TryParse(elements[1], NumberStyles.Integer, culture.NumberFormat, out int y)) {
								return new Int2() { x = x, y = y };
							}
						}
					}
				}
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType) {
			Int2 int2 = (Int2)value;
			if(destinationType == typeof(string)) {
				return $"[{int2.x.ToString(culture.NumberFormat)},{int2.y.ToString(culture.NumberFormat)}]";
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}