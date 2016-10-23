using System.Text;

namespace RapidPliant.Util
{
    public interface IText
    {
        string Text { get; }
        int Length { get; }

        IText Append(string str, params object[] args);
        IText AppendLine(string str, params object[] args);
        IText Newline();
        IText Clear();
    }

    public class StringBuilderText : IText
    {
        private string _str;
        private StringBuilder _sb;

        public StringBuilderText()
            : this(new StringBuilder())
        {
        }

        public StringBuilderText(StringBuilder sb)
        {
            _sb = sb;
        }

        public string Text
        {
            get
            {
                if (_str == null)
                {
                    _str = _sb.ToString();
                }

                return _str;
            }
        }

        public int Length
        {
            get { return _sb.Length; }
        }

        public IText Append(string str, params object[] args)
        {
            _sb.AppendFormat(str, args);
            _str = null;
            return this;
        }

        public IText AppendLine(string str, params object[] args)
        {
            _sb.AppendFormat(str, args);
            _sb.AppendLine();
            _str = null;
            return this;
        }

        public IText Newline()
        {
            _sb.AppendLine();
            _str = null;
            return this;
        }

        public IText Clear()
        {
            _sb.Clear();
            return this;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
