// Copyright (c) 2010-2013 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using dnSpy.Decompiler.Shared;

namespace ICSharpCode.NRefactory.CSharp
{
	/// <summary>
	/// Writes C# code into a TextWriter.
	/// </summary>
	public class TextWriterTokenWriter : TokenWriter, ILocatable
	{
		readonly TextWriter textWriter;
		int indentation;
		bool needsIndent = true;
		bool isAtStartOfLine = true;
		int line, column;

		public int Indentation {
			get { return this.indentation; }
			set { this.indentation = value; }
		}
		
		public TextPosition Location {
			get { return new TextPosition(line, column + (needsIndent ? indentation * IndentationString.Length : 0)); }
		}
		
		public string IndentationString { get; set; }
		
		public TextWriterTokenWriter(TextWriter textWriter)
		{
			if (textWriter == null)
				throw new ArgumentNullException("textWriter");
			this.textWriter = textWriter;
			this.IndentationString = "\t";
			this.line = 1;
			this.column = 1;
		}
		
		public override void WriteIdentifier(Identifier identifier, TextTokenKind tokenKind)
		{
			WriteIndentation();
			if (tokenKind != TextTokenKind.Keyword && (identifier.IsVerbatim || CSharpOutputVisitor.IsKeyword(identifier.Name, identifier))) {
				textWriter.Write('@');
				column++;
			}
			textWriter.Write(identifier.Name);
			column += identifier.Name.Length;
			isAtStartOfLine = false;
		}
		
		public override void WriteKeyword(Role role, string keyword)
		{
			WriteIndentation();
			column += keyword.Length;
			textWriter.Write(keyword);
			isAtStartOfLine = false;
		}
		
		public override void WriteToken(Role role, string token, TextTokenKind tokenKind)
		{
			WriteIndentation();
			column += token.Length;
			textWriter.Write(token);
			isAtStartOfLine = false;
		}
		
		public override void Space()
		{
			WriteIndentation();
			column++;
			textWriter.Write(' ');
		}
		
		protected void WriteIndentation()
		{
			if (needsIndent) {
				needsIndent = false;
				for (int i = 0; i < indentation; i++) {
					textWriter.Write(this.IndentationString);
				}
				column += indentation * IndentationString.Length;
			}
		}
		
		public override void NewLine()
		{
			textWriter.WriteLine();
			column = 1;
			line++;
			needsIndent = true;
			isAtStartOfLine = true;
		}
		
		public override void Indent()
		{
			indentation++;
		}
		
		public override void Unindent()
		{
			indentation--;
		}
		
		public override void WriteComment(CommentType commentType, string content, CommentReference[] refs)
		{
			WriteIndentation();
			switch (commentType) {
				case CommentType.SingleLine:
					textWriter.Write("//");
					textWriter.WriteLine(content);
					column += 2 + content.Length;
					needsIndent = true;
					isAtStartOfLine = true;
					break;
				case CommentType.MultiLine:
					textWriter.Write("/*");
					textWriter.Write(content);
					textWriter.Write("*/");
					column += 2;
					UpdateEndLocation(content, ref line, ref column);
					column += 2;
					isAtStartOfLine = false;
					break;
				case CommentType.Documentation:
					textWriter.Write("///");
					textWriter.WriteLine(content);
					column += 3 + content.Length;
					needsIndent = true;
					isAtStartOfLine = true;
					break;
				case CommentType.MultiLineDocumentation:
					textWriter.Write("/**");
					textWriter.Write(content);
					textWriter.Write("*/");
					column += 3;
					UpdateEndLocation(content, ref line, ref column);
					column += 2;
					isAtStartOfLine = false;
					break;
				default:
					textWriter.Write(content);
					column += content.Length;
					break;
			}
		}
		
		static void UpdateEndLocation(string content, ref int line, ref int column)
		{
			if (string.IsNullOrEmpty(content))
				return;
			for (int i = 0; i < content.Length; i++) {
				char ch = content[i];
				switch (ch) {
					case '\r':
						if (i + 1 < content.Length && content[i + 1] == '\n')
							i++;
						goto case '\n';
					case '\n':
						line++;
						column = 0;
						break;
				}
				column++;
			}
		}
		
		public override void WritePreProcessorDirective(PreProcessorDirectiveType type, string argument)
		{
			// pre-processor directive must start on its own line
			if (!isAtStartOfLine)
				NewLine();
			WriteIndentation();
			textWriter.Write('#');
			string directive = type.ToString().ToLowerInvariant();
			textWriter.Write(directive);
			column += 1 + directive.Length;
			if (!string.IsNullOrEmpty(argument)) {
				textWriter.Write(' ');
				textWriter.Write(argument);
				column += 1 + argument.Length;
			}
			NewLine();
		}
		
		public static string PrintPrimitiveValue(object value)
		{
			TextWriter writer = new StringWriter();
			TextWriterTokenWriter tokenWriter = new TextWriterTokenWriter(writer);
			tokenWriter.WritePrimitiveValue(value, TextTokenKindUtils.GetTextTokenType(value));
			return writer.ToString();
		}
		
		public override void WritePrimitiveValue(object value, TextTokenKind? tokenKind = null, string literalValue = null)
		{
			WritePrimitiveValue(value, tokenKind, literalValue, ref column, (a, b) => textWriter.Write(a), (a, b, c) => WriteToken(a, b, c));
		}
		
		public static void WritePrimitiveValue(object value, TextTokenKind? tokenKind, string literalValue, ref int column, Action<string, TextTokenKind> writer, Action<Role, string, TextTokenKind> writeToken)
		{
			if (literalValue != null) {
				Debug.Assert(tokenKind != null);
				writer(literalValue, tokenKind ?? TextTokenKind.Text);
				column += literalValue.Length;
				return;
			}
			
			if (value == null) {
				// usually NullReferenceExpression should be used for this, but we'll handle it anyways
				writer("null", TextTokenKind.Keyword);
				column += 4;
				return;
			}
			
			if (value is bool) {
				if ((bool)value) {
					writer("true", TextTokenKind.Keyword);
					column += 4;
				} else {
					writer("false", TextTokenKind.Keyword);
					column += 5;
				}
				return;
			}
			
			if (value is string) {
				string tmp = "\"" + ConvertString(value.ToString()) + "\"";
				column += tmp.Length;
				writer(tmp, TextTokenKind.String);
			} else if (value is char) {
				string tmp = "'" + ConvertCharLiteral((char)value) + "'";
				column += tmp.Length;
				writer(tmp, TextTokenKind.Char);
			} else if (value is decimal) {
				string str = ((decimal)value).ToString(NumberFormatInfo.InvariantInfo) + "m";
				column += str.Length;
				writer(str, TextTokenKind.Number);
			} else if (value is float) {
				float f = (float)value;
				if (float.IsInfinity(f) || float.IsNaN(f)) {
					// Strictly speaking, these aren't PrimitiveExpressions;
					// but we still support writing these to make life easier for code generators.
					writer("float", TextTokenKind.Keyword);
					column += 5;
					writeToken(Roles.Dot, ".", TextTokenKind.Operator);
					if (float.IsPositiveInfinity(f)) {
						writer("PositiveInfinity", TextTokenKind.LiteralField);
						column += "PositiveInfinity".Length;
					} else if (float.IsNegativeInfinity(f)) {
						writer("NegativeInfinity", TextTokenKind.LiteralField);
						column += "NegativeInfinity".Length;
					} else {
						writer("NaN", TextTokenKind.LiteralField);
						column += 3;
					}
					return;
				}
				var number = f.ToString("R", NumberFormatInfo.InvariantInfo) + "f";
				if (f == 0 && 1 / f == float.NegativeInfinity) {
					// negative zero is a special case
					// (again, not a primitive expression, but it's better to handle
					// the special case here than to do it in all code generators)
					number = "-" + number;
				}
				column += number.Length;
				writer(number, TextTokenKind.Number);
			} else if (value is double) {
				double f = (double)value;
				if (double.IsInfinity(f) || double.IsNaN(f)) {
					// Strictly speaking, these aren't PrimitiveExpressions;
					// but we still support writing these to make life easier for code generators.
					writer("double", TextTokenKind.Keyword);
					column += 6;
					writeToken(Roles.Dot, ".", TextTokenKind.Operator);
					if (double.IsPositiveInfinity(f)) {
						writer("PositiveInfinity", TextTokenKind.LiteralField);
						column += "PositiveInfinity".Length;
					} else if (double.IsNegativeInfinity(f)) {
						writer("NegativeInfinity", TextTokenKind.LiteralField);
						column += "NegativeInfinity".Length;
					} else {
						writer("NaN", TextTokenKind.LiteralField);
						column += 3;
					}
					return;
				}
				string number = f.ToString("R", NumberFormatInfo.InvariantInfo);
				if (f == 0 && 1 / f == double.NegativeInfinity) {
					// negative zero is a special case
					// (again, not a primitive expression, but it's better to handle
					// the special case here than to do it in all code generators)
					number = "-" + number;
				}
				if (number.IndexOf('.') < 0 && number.IndexOf('E') < 0) {
					number += ".0";
				}
				column += number.Length;
				writer(number, TextTokenKind.Number);
			} else if (value is IFormattable) {
				StringBuilder b = new StringBuilder ();
//				if (primitiveExpression.LiteralFormat == LiteralFormat.HexadecimalNumber) {
//					b.Append("0x");
//					b.Append(((IFormattable)val).ToString("x", NumberFormatInfo.InvariantInfo));
//				} else {
					b.Append(((IFormattable)value).ToString(null, NumberFormatInfo.InvariantInfo));
//				}
				if (value is uint || value is ulong) {
					b.Append("u");
				}
				if (value is long || value is ulong) {
					b.Append("L");
				}
				writer(b.ToString(), TextTokenKind.Number);
				column += b.Length;
			} else {
				var s = value.ToString();
				writer(s, TextTokenKindUtils.GetTextTokenType(value));
				column += s.Length;
			}
		}
		
		/// <summary>
		/// Gets the escape sequence for the specified character within a char literal.
		/// Does not include the single quotes surrounding the char literal.
		/// </summary>
		public static string ConvertCharLiteral(char ch)
		{
			if (ch == '\'') {
				return "\\'";
			}
			return ConvertChar(ch);
		}
		
		/// <summary>
		/// Gets the escape sequence for the specified character.
		/// </summary>
		/// <remarks>This method does not convert ' or ".</remarks>
		static string ConvertChar(char ch)
		{
			switch (ch) {
				case '\\':
					return "\\\\";
				case '\0':
					return "\\0";
				case '\a':
					return "\\a";
				case '\b':
					return "\\b";
				case '\f':
					return "\\f";
				case '\n':
					return "\\n";
				case '\r':
					return "\\r";
				case '\t':
					return "\\t";
				case '\v':
					return "\\v";
				default:
					if (char.IsControl(ch) || char.IsSurrogate(ch) ||
					    // print all uncommon white spaces as numbers
					    (char.IsWhiteSpace(ch) && ch != ' ')) {
						return "\\u" + ((int)ch).ToString("x4");
					} else {
						return ch.ToString();
					}
			}
		}
		
		/// <summary>
		/// Converts special characters to escape sequences within the given string.
		/// </summary>
		public static string ConvertString(string str)
		{
			StringBuilder sb = new StringBuilder ();
			foreach (char ch in str) {
				if (ch == '"') {
					sb.Append("\\\"");
				} else {
					sb.Append(ConvertChar(ch));
				}
			}
			return sb.ToString();
		}
		
		public override void WritePrimitiveType(string type)
		{
			textWriter.Write(type);
			column += type.Length;
			if (type == "new") {
				textWriter.Write("()");
				column += 2;
			}
		}
		
		public override void StartNode(AstNode node)
		{
			// Write out the indentation, so that overrides of this method
			// can rely use the current output length to identify the position of the node
			// in the output.
			WriteIndentation();
		}
		
		public override void EndNode(AstNode node)
		{
		}
	}
}
