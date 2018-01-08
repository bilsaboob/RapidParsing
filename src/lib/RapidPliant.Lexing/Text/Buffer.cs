using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Lexing.Text
{
    /// <summary>Buffer that allows retrieval of contents by position</summary>
    public interface IBuffer
    {
        int Length { get; }

        string GetText();

        string GetText(TextRange range);

        void AppendTextTo(StringBuilder builder, TextRange range);

        char this[int index] { get; }

        void CopyTo(int sourceIndex, char[] destinationArray, int destinationIndex, int length);
    }

    public class StringBuffer : IBuffer
    {
        public static readonly StringBuffer Empty = new StringBuffer(string.Empty);
        private readonly string myString;

        public StringBuffer(string @string)
        {
            this.myString = @string;
        }

        public int Length
        {
            get
            {
                return this.myString.Length;
            }
        }

        public string GetText()
        {
            return this.myString;
        }

        public string GetText(TextRange range)
        {
            return this.myString.Substring(range.StartOffset, range.Length);
        }

        public void AppendTextTo(StringBuilder builder, TextRange range)
        {
            builder.Append(this.myString, range.StartOffset, range.Length);
        }

        public char this[int index]
        {
            get
            {
                return this.myString[index];
            }
        }

        public void CopyTo(int sourceIndex, char[] destinationArray, int destinationIndex, int length)
        {
            this.myString.CopyTo(sourceIndex, destinationArray, destinationIndex, length);
        }
    }
}
