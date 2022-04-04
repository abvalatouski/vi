using System;
using System.Text;

namespace Task.Vi
{
    public sealed class StatusBar
    {
        public Mode Mode
        {
            get;
            set;
        }

        public Cursor Cursor
        {
            get;
            set;
        }

        public string Filename
        {
            get;
            set;
        }

        public StatusBar(Mode mode, Cursor cursor, string filename)
        {
            Mode     = mode;
            Cursor   = cursor;
            Filename = filename;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            string mode;
            switch (Mode)
            {
                case Mode.Normal:
                    mode = "NORMAL";
                    break;
                case Mode.Insert:
                    mode = "INSERT";
                    break;
                default:
                    throw new InvalidOperationException(
                        "Обнаружен недостижимый код!"
                    );
            }
            builder.AppendFormat("-- {0} --", mode);
            builder.Append(" | ");
            builder.AppendFormat("\"{0}\"", Filename);
            builder.Append(" | ");
            builder.AppendFormat("LN: {0} COL: {1}", Cursor.Top + 1, Cursor.Left + 1);

            return builder.ToString();
        }
    }
}

