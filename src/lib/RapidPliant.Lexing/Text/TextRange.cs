using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace RapidPliant.Lexing.Text
{
    /// <summary>Represents a range in a plaintext document.</summary>
    [DebuggerDisplay("Range = ({StartOffset}:{EndOffset}), Length = {Length}")]
    [Serializable]
    public struct TextRange : IEquatable<TextRange>
    {
        public static readonly TextRange Empty = new TextRange(0, 0);

        /// <summary>
        /// Should be replaced with <see cref="T:System.Nullable`1" /> of <see cref="T:JetBrains.Util.TextRange" /> wherever possible.
        /// Avoid using in new code.
        /// </summary>
        public static readonly TextRange InvalidRange = new TextRange(-1, -1);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int myStartOffset;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int myEndOffset;

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange(int startOffset, int endOffset)
        {
            this.myStartOffset = startOffset;
            this.myEndOffset = endOffset;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange(int offset)
        {
            this.myStartOffset = this.myEndOffset = offset;
        }

        /// <summary>
        /// <para>The first offset (character position) of the range, inclusive.</para>
        /// <para>A character at this index is included with the range.</para>
        /// </summary>
        public int StartOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.myStartOffset;
            }
        }

        /// <summary>
        /// <para>The last offset (character position) of the range, non-inclusive.</para>
        /// <para>A character at this index is not included with the range and goes right after the range end.</para>
        /// <para>An end offset could point at the end of the document, in which case there's no character at this position.</para>
        /// </summary>
        public int EndOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.myEndOffset;
            }
        }

        /// <summary>
        /// <para>The number of characters in the text range.</para>
        /// <para>As the end offset is non-inclusive, this is equal to <see cref="P:JetBrains.Util.TextRange.EndOffset" /> <c>–</c> <see cref="P:JetBrains.Util.TextRange.StartOffset" />.</para>
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.myEndOffset - this.myStartOffset;
            }
        }

        /// <summary>
        /// <para>Whether the range is empty.</para>
        /// <para>The <see cref="P:JetBrains.Util.TextRange.Length" /> of an empty range is zero, and its <see cref="P:JetBrains.Util.TextRange.StartOffset" /> is the same as its <see cref="P:JetBrains.Util.TextRange.EndOffset" />.</para>
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.myStartOffset == this.myEndOffset;
            }
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, (object)null) || !(obj is TextRange))
                return false;
            return this.Equals((TextRange)obj);
        }

        public bool Equals(TextRange other)
        {
            if (other.myStartOffset == this.myStartOffset)
                return other.myEndOffset == this.myEndOffset;
            return false;
        }

        public override int GetHashCode()
        {
            return this.myStartOffset * 397 ^ this.myEndOffset;
        }

        public override string ToString()
        {
            return this.ToString((IFormatProvider)CultureInfo.CurrentCulture);
        }
        
        [Pure]
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "({0:N0} - {1:N0})", new object[2]
            {
        (object) this.myStartOffset,
        (object) this.myEndOffset
            });
        }

        [Pure]
        public string ToInvariantString()
        {
            return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "({0},{1})", new object[2]
            {
        (object) this.myStartOffset,
        (object) this.myEndOffset
            });
        }

        public static TextRange Parse(string s)
        {
            s = s.Trim();
            int startIndex = 0;
            int length = s.Length;
            if ((int)s[startIndex] == 40)
            {
                ++startIndex;
                --length;
            }
            if ((int)s[startIndex + length - 1] == 41)
                --length;
            s = s.Substring(startIndex, length);
            string[] strArray1;
            if (!s.Contains(","))
                strArray1 = s.Split('-');
            else
                strArray1 = s.Split(',');
            string[] strArray2 = strArray1;
            int result1;
            int result2;
            if (strArray2.Length != 2 || !int.TryParse(strArray2[0].Trim(), out result1) || !int.TryParse(strArray2[1].Trim(), out result2))
                return TextRange.InvalidRange;
            return new TextRange(result1, result2);
        }

        /// <summary>
        /// Creates a new range from offset and length, rather than from start + end offsets, as the <c>.ctor</c> would do.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextRange FromLength(int offset, int length)
        {
            return new TextRange(offset, offset + length);
        }

        /// <summary>
        /// Creates a new range from zero offset and length, rather than from start + end offsets, as the <c>.ctor</c> would do.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextRange FromLength(int length)
        {
            return new TextRange(0, length);
        }

        /// <summary>
        /// <para>Creates a range from two unordered offsets, i.e. when it's not known which of the offsets is the start and which one is the end.</para>
        /// <para>The resultant range is guaranteed to be normalized.</para>
        /// <para>DO NOT USE unless the source of your offset is unordered by its nature (eg if you select text with shift+left the offsets of
        /// the selection range will be inverted, so they're sometimes out of order by design). Usually you should know which offset is start
        /// and which is end, and failure to do so might indicate an error in preceding code.</para>
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextRange FromUnorderedOffsets(int one, int onemore)
        {
            if (one > onemore)
                return new TextRange(onemore, one);
            return new TextRange(one, onemore);
        }

        /// <summary>
        /// Gets the <see cref="P:JetBrains.Util.TextRange.StartOffset" /> or <see cref="P:JetBrains.Util.TextRange.EndOffset" />, whichever is smaller.
        /// Use for consistent processing of potentially non-normalized ranges.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMinOffset()
        {
            if (this.myStartOffset > this.myEndOffset)
                return this.myEndOffset;
            return this.myStartOffset;
        }

        /// <summary>
        /// Gets the <see cref="P:JetBrains.Util.TextRange.StartOffset" /> or <see cref="P:JetBrains.Util.TextRange.EndOffset" />, whichever is greater.
        /// Use for consistent processing of potentially non-normalized ranges.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMaxOffset()
        {
            if (this.myEndOffset < this.myStartOffset)
                return this.myStartOffset;
            return this.myEndOffset;
        }

        /// <summary>
        /// Gets whether this range is a subset of the <paramref name="textRange" />.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainedIn(TextRange textRange)
        {
            this.AssertNormalized();
            textRange.AssertNormalized();
            if (this.myStartOffset >= textRange.myStartOffset)
                return this.myEndOffset <= textRange.myEndOffset;
            return false;
        }

        /// <summary>
        /// Gets whether this range is a proper subset of the <paramref name="textRange" />.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StrictContainedIn(TextRange textRange)
        {
            this.AssertNormalized();
            textRange.AssertNormalized();
            int startOffset1 = this.myStartOffset;
            int endOffset1 = this.myEndOffset;
            int startOffset2 = textRange.myStartOffset;
            int endOffset2 = textRange.myEndOffset;
            if (startOffset1 < startOffset2 || endOffset1 > endOffset2)
                return false;
            if (startOffset1 == startOffset2)
                return endOffset1 != endOffset2;
            return true;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TextRange textRange)
        {
            return textRange.ContainedIn(this);
        }

        /// <summary>
        /// Determines whether the offset falls within the range, start and end offsets included.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int offset)
        {
            if ((this.myStartOffset <= this.myEndOffset ? this.myStartOffset : this.myEndOffset) <= offset)
                return offset <= (this.myEndOffset >= this.myStartOffset ? this.myEndOffset : this.myStartOffset);
            return false;
        }

        /// <summary>
        /// <para>Checks whether the character at the <paramref name="charindex">given index</paramref> falls within this range.</para>
        /// <para>Unlike <see cref="M:JetBrains.Util.TextRange.Contains(System.Int32)" />, the right offset is not included, because the range ends before the character with such an index.</para>
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsCharIndex(int charindex)
        {
            if ((this.myStartOffset <= this.myEndOffset ? this.myStartOffset : this.myEndOffset) <= charindex)
                return charindex < (this.myEndOffset >= this.myStartOffset ? this.myEndOffset : this.myStartOffset);
            return false;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange SetStartTo(int offset)
        {
            return new TextRange(offset, this.myEndOffset);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange SetEndTo(int offset)
        {
            return new TextRange(this.myStartOffset, offset);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange Left(int length)
        {
            return new TextRange(this.myStartOffset, this.myStartOffset + length);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange Right(int length)
        {
            return new TextRange(this.myEndOffset - length, this.myEndOffset);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange TrimLeft(int length)
        {
            this.AssertNormalized();
            if (length < 0 || length > this.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
            return new TextRange(this.myStartOffset + length, this.myEndOffset);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange TrimRight(int length)
        {
            this.AssertNormalized();
            if (length < 0 || length > this.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
            return new TextRange(this.myStartOffset, this.myEndOffset - length);
        }

        [DebuggerStepThrough]
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange ExtendLeft(int length)
        {
            this.AssertNormalized();
            if (length > this.myStartOffset)
                throw new ArgumentOutOfRangeException(nameof(length));
            return new TextRange(this.myStartOffset - length, this.myEndOffset);
        }

        [Pure]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange ExtendRight(int length)
        {
            this.AssertNormalized();
            return new TextRange(this.myStartOffset, this.myEndOffset + length);
        }

        [DebuggerStepThrough]
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange Shift(int delta)
        {
            if (!this.IsValid)
                return TextRange.InvalidRange;
            return new TextRange(this.myStartOffset + delta, this.myEndOffset + delta);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange Join(TextRange textRange)
        {
            if (!this.IsValid)
                return TextRange.InvalidRange;
            int num1 = this.myStartOffset <= this.myEndOffset ? this.myStartOffset : this.myEndOffset;
            int num2 = textRange.myStartOffset <= textRange.myEndOffset ? textRange.myStartOffset : textRange.myEndOffset;
            int num3 = this.myEndOffset >= this.myStartOffset ? this.myEndOffset : this.myStartOffset;
            int num4 = textRange.myEndOffset >= textRange.myStartOffset ? textRange.myEndOffset : textRange.myStartOffset;
            return new TextRange(num1 < num2 ? num1 : num2, num3 > num4 ? num3 : num4);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange JoinLeft(TextRange textRange)
        {
            this.AssertNormalized();
            return new TextRange(Math.Min(this.myStartOffset, textRange.myStartOffset), this.myEndOffset);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange JoinRight(TextRange textRange)
        {
            this.AssertNormalized();
            return new TextRange(this.myStartOffset, Math.Max(this.myEndOffset, textRange.myEndOffset));
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(TextRange textRange)
        {
            if ((this.myStartOffset <= this.myEndOffset ? this.myStartOffset : this.myEndOffset) <= (textRange.myEndOffset >= textRange.myStartOffset ? textRange.myEndOffset : textRange.myStartOffset))
                return (this.myEndOffset >= this.myStartOffset ? this.myEndOffset : this.myStartOffset) >= (textRange.myStartOffset <= textRange.myEndOffset ? textRange.myStartOffset : textRange.myEndOffset);
            return false;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StrictIntersects(TextRange textRange)
        {
            if ((this.myStartOffset <= this.myEndOffset ? this.myStartOffset : this.myEndOffset) < (textRange.myEndOffset >= textRange.myStartOffset ? textRange.myEndOffset : textRange.myStartOffset))
                return (this.myEndOffset >= this.myStartOffset ? this.myEndOffset : this.myStartOffset) > (textRange.myStartOffset <= textRange.myEndOffset ? textRange.myStartOffset : textRange.myEndOffset);
            return false;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange Intersect(TextRange textRange)
        {
            if (!this.Intersects(textRange))
                return TextRange.InvalidRange;
            int num1 = this.myStartOffset <= this.myEndOffset ? this.myStartOffset : this.myEndOffset;
            int num2 = textRange.myStartOffset <= textRange.myEndOffset ? textRange.myStartOffset : textRange.myEndOffset;
            int num3 = this.myEndOffset >= this.myStartOffset ? this.myEndOffset : this.myStartOffset;
            int num4 = textRange.myEndOffset >= textRange.myStartOffset ? textRange.myEndOffset : textRange.myStartOffset;
            return new TextRange(num1 > num2 ? num1 : num2, num3 < num4 ? num3 : num4);
        }

        /// <summary>
        /// Should be replaced with <see cref="T:System.Nullable`1" /> of <see cref="T:JetBrains.Util.TextRange" /> wherever possible.
        /// Avoid using in new code.
        /// Checks that the range is not the <see cref="F:JetBrains.Util.TextRange.InvalidRange" />.
        /// </summary>
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (this.myStartOffset == -1)
                    return this.myEndOffset != -1;
                return true;
            }
        }

        /// <summary>
        /// Asserts that this range is valid, which means that it's not an <see cref="F:JetBrains.Util.TextRange.InvalidRange" />.
        /// Throws in ASSERT mode only.
        /// </summary>
        [Conditional("JET_MODE_ASSERT")]
        public void AssertValid()
        {
            Debug.Assert(this.IsValid, "The text range must be valid.");
        }

        /// <summary>
        /// Asserts that this range is normalized, which means that its <see cref="P:JetBrains.Util.TextRange.Length" /> is nonnegative.
        /// Includes <see cref="M:JetBrains.Util.TextRange.AssertValid" />.
        /// Throws in ASSERT mode only.
        /// </summary>
        [Conditional("JET_MODE_ASSERT")]
        public void AssertNormalized()
        {
            Debug.Assert(this.IsValid, "The text range must be valid.");
            Debug.Assert(this.IsNormalized, $"The text range must be normalized. Start: {this.myStartOffset}, End: {this.myEndOffset}");
        }

        [Conditional("JET_MODE_ASSERT")]
        public void AssertContainedIn(TextRange rangeContainer)
        {
            if (!this.ContainedIn(rangeContainer))
                throw new InvalidOperationException(string.Format("The range {0} does not fall within the range {1}.", (object)this, (object)rangeContainer));
        }

        /// <summary>
        /// Gets whether this range is normalized, which means that its <see cref="P:JetBrains.Util.TextRange.Length" /> is nonnegative.
        /// </summary>
        public bool IsNormalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.myStartOffset <= this.myEndOffset;
            }
        }

        /// <summary>
        /// Returns a normalized version of the current text range (with a nonnegative length).
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextRange Normalized()
        {
            if (!this.IsValid)
                return TextRange.InvalidRange;
            return new TextRange(this.GetMinOffset(), this.GetMaxOffset());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TextRange range1, TextRange range2)
        {
            if (range1.myStartOffset == range2.myStartOffset)
                return range1.myEndOffset == range2.myEndOffset;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TextRange range1, TextRange range2)
        {
            if (range1.myStartOffset == range2.myStartOffset)
                return range1.myEndOffset != range2.myEndOffset;
            return true;
        }

        /// <summary>
        /// Returns the distance between the <paramref name="offset" /> and the nearest point that belongs to the range.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DistanceTo(int offset)
        {
            int minOffset = this.GetMinOffset();
            int maxOffset = this.GetMaxOffset();
            if (offset < minOffset)
                return minOffset - offset;
            if (offset > maxOffset)
                return offset - maxOffset;
            return 0;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLeftTo(int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), (object)offset, "The offset must be nonnegative.");
            this.AssertNormalized();
            return this.myEndOffset <= offset;
        }

        [Pure]
        public TextRange UpdateRange(int changeStartOffset, int oldLength, int newLength, bool greedyToLeft, bool greedyToRight)
        {
            this.AssertNormalized();
            int num = changeStartOffset + oldLength;
            int startOffset = this.myStartOffset;
            int endOffset = this.myEndOffset;
            if (startOffset == endOffset && !greedyToRight && !greedyToLeft)
            {
                if (startOffset > changeStartOffset && startOffset < num)
                    return TextRange.InvalidRange;
                if (startOffset > num || startOffset == num && oldLength > 0)
                    return new TextRange(startOffset + newLength - oldLength);
            }
            if (endOffset < changeStartOffset || (!greedyToRight || newLength == 0) && endOffset == changeStartOffset)
                return this;
            if (startOffset > num || (!greedyToLeft || newLength == 0) && startOffset == num)
                return this.Shift(newLength - oldLength);
            if (startOffset <= changeStartOffset && endOffset >= num)
                return new TextRange(startOffset, endOffset + newLength - oldLength);
            if (startOffset >= changeStartOffset && startOffset <= num && endOffset > num)
                return new TextRange(changeStartOffset + newLength, endOffset + newLength - oldLength);
            if (endOffset >= changeStartOffset && endOffset <= num && startOffset < changeStartOffset)
                return new TextRange(startOffset, changeStartOffset);
            if (startOffset >= changeStartOffset && startOffset <= num && (endOffset >= changeStartOffset && endOffset <= num))
                return TextRange.InvalidRange;
            return this;
        }
    }

    public static class TextRangeEx
    {
        
        [Pure]
        public static string Substring( this string str, TextRange range)
        {
            return str.Substring(range.StartOffset, range.Length);
        }

        
        [Pure]
        public static string ToString( this StringBuilder builder, TextRange range)
        {
            return builder.ToString(range.StartOffset, range.Length);
        }

        [Pure]
        public static bool RangeStartsWith( this string str, TextRange range, string value)
        {
            return str.RangeStartsWith(range, value, StringComparison.CurrentCulture);
        }

        [Pure]
        public static bool RangeEndsWith( this string str, TextRange range, string value)
        {
            return str.RangeEndsWith(range, value, StringComparison.CurrentCulture);
        }

        [Pure]
        public static bool RangeEquals( this string str, TextRange range, string value)
        {
            return str.RangeEquals(range, value, StringComparison.CurrentCulture);
        }

        [Pure]
        public static bool RangeStartsWith( this string str, TextRange range, string value, StringComparison comparison)
        {
            int length = value.Length;
            if (range.Length < length)
                return false;
            return str.RangeEquals(range.Left(length), value, comparison);
        }

        [Pure]
        public static bool RangeEndsWith( this string str, TextRange range, string value, StringComparison comparison)
        {
            int length = value.Length;
            if (range.Length < length)
                return false;
            return str.RangeEquals(range.Right(length), value, comparison);
        }

        [Pure]
        public static bool RangeEquals( this string str, TextRange range, string value, StringComparison comparison)
        {
            if (value.Length != range.Length)
                return false;
            return string.Compare(str, range.StartOffset, value, 0, range.Length, comparison) == 0;
        }

        
        public static IList<TextRange> Merge( this IList<TextRange> ranges)
        {
            if (ranges.Count < 2)
                return ranges;
            List<TextRange> textRangeList = new List<TextRange>();
            int startOffset = ranges[0].StartOffset;
            int endOffset = startOffset;
            foreach (TextRange range in (IEnumerable<TextRange>)ranges)
            {
                Debug.Assert(range.StartOffset >= endOffset, "Ranges must be sorted!");
                if (range.StartOffset > endOffset)
                {
                    if (endOffset > startOffset)
                        textRangeList.Add(new TextRange(startOffset, endOffset));
                    startOffset = range.StartOffset;
                }
                endOffset = range.EndOffset;
            }
            if (endOffset > startOffset)
                textRangeList.Add(new TextRange(startOffset, endOffset));
            return (IList<TextRange>)textRangeList;
        }
    }
}
