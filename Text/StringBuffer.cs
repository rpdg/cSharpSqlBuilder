
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Lyu.Text
{
	/// <summary>
	/// StringBuffer : 类似于StringBuilder的字符串处理类
	/// 提供了更多的功能
	/// </summary>
	/// 
	public class StringBuffer
	{
		
		protected StringBuilder Builder;

        
		public StringBuffer()
		{
			Builder = new StringBuilder(); 
		}
		
		
		public StringBuffer(string value)
		{
			Builder = new StringBuilder(value); 
		}
		
		/// <summary>
		/// 隐式从 string 转换
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static implicit operator StringBuffer(string value)
		{ 
			var text = new StringBuffer(); 
			text.Builder.Append(value); 
			return text; 
		}
		
		/// <summary>
		/// 能够隐式转换成 string 类型
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static implicit operator string(StringBuffer value)
		{ 
			return value.ToString(); 
		}
		
		/// <summary>
		/// s += "Bruce";
		/// s += 123;
		/// </summary>
		/// <param name="self"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static StringBuffer operator +(StringBuffer self, object value)
		{ 
			self.Builder.Append(value);
			return self; 
		}


		public override string ToString()
		{ 
			return this.Builder.ToString(); 
		}

		public string ToString(int startIndex , int length)
		{ 
			return this.Builder.ToString(startIndex ,length); 
		}
		
		
		// Adds IndexOf, Substring, Equals to the class.
		public int IndexOf(string value, bool ignoreCase = false)
		{
			return this.IndexOf(value, 0, ignoreCase);
		}
        
		public int IndexOf(string value, int startIndex, bool ignoreCase = false)
		{
			int index;
			int length = value.Length;
			int maxSearchLength = (Builder.Length - length) + 1;

			if (ignoreCase) {
				for (int i = startIndex; i < maxSearchLength; ++i) {
					if (Char.ToLower(Builder[i]) == Char.ToLower(value[0])) {
						index = 1;
						while ((index < length) && (Char.ToLower(Builder[i + index]) == Char.ToLower(value[index])))
							++index;

						if (index == length)
							return i;
					}
				}
				return -1;
			}

			for (int i = startIndex; i < maxSearchLength; ++i) {
				if (Builder[i] == value[0]) {
					index = 1;
					while ((index < length) && (Builder[i + index] == value[index]))
						++index;

					if (index == length)
						return i;
				}
			}
			return -1;
		}
        
		public string Substring(int startIndex, int length)
		{
			return this.Builder == null ? null : this.Builder.ToString(startIndex, length);
		}
		
		
		public StringBuffer Remove(int startIndex, int length)
		{
			Builder.Remove(startIndex, length);
			return this;
		}
		
		public StringBuffer RemoveFromEnd(int num)
		{
			Builder.Remove(Builder.Length - num, num);
			return this;
		}
		
		
        
		public bool Equals(string compareString)
		{
			if (this.Builder == null) {
				return compareString == null;
			}
			if (compareString == null) {
				return false;
			}
			int len = this.Builder.Length;
			if (len != compareString.Length) {
				return false;
			}
			for (int loop = 0; loop < len; loop++) {
				if (this.Builder[loop] != compareString[loop]) {
					return false;
				}
			}
			return true;            
		}
		/// <summary>
		/// Compares one string to part of another string.
		/// </summary>
		/// <param name="needle">Needle to look for</param>
		/// <param name="position">Looks to see if the needle is at position in haystack</param>
		/// <returns></returns>
		public bool Contains(string needle, int position = 0)
		{
			if (this.Builder == null) {
				return needle == null;
			}
			if (needle == null) {
				return false;
			}
			int len = this.Builder.Length;
			int compareLen = needle.Length;
			if (len < compareLen + position) {
				return false;
			}
			for (int loop = 0; loop < compareLen; loop++) {
				if (this.Builder[loop + position] != needle[loop]) {
					return false;
				}
			}
			return true;
		}
        
		public void WriteToFile(string path)
		{
			File.WriteAllText(path, this.Builder.ToString());
		}
		
		/// <summary>
		/// 从文本文件载入
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="charset"></param>
		/// <returns></returns>
		public static StringBuffer FromFile(string fileName, System.Text.Encoding charset)
		{
			StringBuffer text = null;
			StreamReader stream = null;
			try {            
				stream = new StreamReader(fileName, charset);
				text = new StringBuffer(stream.ReadToEnd());
			} catch {
				text = null;
			} finally {
				if (stream != null)
					stream.Close();
			}
			return text;
		}
		public static StringBuffer FromFile(string fileName)
		{
			return StringBuffer.FromFile(fileName, System.Text.Encoding.Default);
		}
		
		public void Clear()
		{
			this.Builder.Length = 0;
		}
		
		/// <summary>
		/// Appends a formatted string and the default line terminator to to this StringBuilder instance.
		/// usage:
		/// sb.AppendLineFormat("File name: {0} (line: {1}, column: {2})", fileName, lineNumber, column);
		/// sb.AppendLineFormat("Message: {0}", exception.Message);
		/// </summary>
		/// <param name="format"></param>
		/// <param name="arguments"></param>
		public StringBuffer AppendLineFormat(string format, params object[] arguments)
		{
			string value = String.Format(format, arguments);
			Builder.AppendLine(value);
			return this;
		}
		
		public StringBuffer AppendFormat(string format, params object[] args)
		{
			Builder.AppendFormat(format, args);
			return this;
		}
		
		
		/// <summary>
		/// var sb1 = new StringBuffer();
		/// var people = new List<Person>() { ...init people here... };
		/// sb1.AppendCollection(people, p => p.ToString());
		/// string stringPeople = sb1.ToString();
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="method"></param>
		public StringBuffer AppendCollection<T>(IEnumerable<T> collection, Func<T, string> method)
		{
			foreach (T x in collection)
				this.Builder.AppendLine(method(x));
			
			return this;
		}
		
		
		
		#region 属性
		
		public bool IsEmpty {
			get {
				return (this.Builder.Length == 0);
			}
			
		}
		
		public char this[int index] {
			get {
				if ((index < 0) || (index >= this.Builder.Length)) {
					throw new ArgumentOutOfRangeException();
				}
				return this.Builder[index];
			}
			set {
				if ((index < 0) || (index >= Builder.Length)) {
					throw new ArgumentOutOfRangeException();
				}
				this.Builder[index] = value;
			}
		}
		/// <summary>
		/// 设置或返回初始容量
		/// </summary>
		public int Capacity {
			get {
				return this.Builder.Capacity;
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException();
				
				this.Builder.Capacity = value;
			}
		}
		/// <summary>
		/// 设置或返回当前存储字符的长度
		/// </summary>
		public int Length {
			get {
				return this.Builder.Length;
			}
			set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException();
				}
				if (this.Builder.Length > value) {
					this.Builder.Length = value;
				}
			}
		}
		#endregion
	}
}
