using        System.Collections.Generic;
using        System.IO;
using        System.Linq;
using static System.Math;
using        System.Text;

namespace Task.Vi
{
    using Line = List<char>;

    public struct Buffer
    {
        private const char Empty = ' ';

        public List<Line> Lines
        {
            get;
        }

        public string Filename
        {
            get;
        }

        #region Ориентация.
        // # Подводные камни
        //
        // `System.String` использует кодировку UTF-16, а это означает,
        // что текст может иметь символы как и 2-байтные, так и 4-байтые.
        // Поэтому обычное перемещение по строкам через индексы не работает.
        public char this[int line, int column]
        {
            get => line >= Lines.Count || column >= Lines[line].Count
                ? Empty
                : Lines[line][column];
            set
            {
                AdjustBuffer(line, column);
                Lines[line][column] = value;
            }
        }

        private void AdjustBuffer(int line, int column)
        {
            if (line >= Lines.Count)
            {
                var diff = line - Lines.Count + 1;
                for (var i = 0; i < diff; i++)
                {
                    Lines.Add(new Line());
                }
            }

            if (column >= Lines[line].Count)
            {
                var diff = column - Lines[line].Count + 1;
                for (var i = 0; i < diff; i++)
                {
                    Lines[line].Add(Empty);
                }
            }
        }

        public char this[Cursor cursor]
        {
            get => this[cursor.Left, cursor.Top];
            set => this[cursor.Left, cursor.Top] = value;
        }
        #endregion

        #region Открытие / сохранение.
        public Buffer(string filename, bool createNewFile = false)
        {
            Filename = filename;
            // Тренируемся с LINQ...
            Lines = createNewFile
                ? new List<Line>()
                : (from line in File.ReadAllLines(filename) select line.ToList()).ToList();
        }

        public void SaveChanges()
        {
            const int SpaceForNewline = 1;
            var builder = new StringBuilder(
                // Тренируемся с LINQ...
                capacity: (from line in Lines select line.Count + SpaceForNewline).Sum()
            );

            foreach (Line line in Lines)
            {
                builder.AppendLine(new string(line.ToArray()));
            }

            File.WriteAllText(Filename, builder.ToString());
        }
        #endregion

        #region Вставка / удаление.
        public void Insert(Cursor cursor, char c) =>
            Insert(cursor.Left, cursor.Top, c);

        public void Insert(int left, int top, char c)
        {
            if (c == '\n')
            {
                var line = new Line();
                if (left != Lines[top].Count)
                {
                    char[] after = new char[Lines[top].Count - left];
                    Lines[top].CopyTo(left, after, 0, after.Length);
                    line.AddRange(after);
                    Lines[top].RemoveRange(left, Lines[top].Count - left);
                }
                Lines.Insert(top + 1, line);
            }
            else
            {
                Lines[top].Insert(left, c);
            }
        }

        public void Remove(Cursor cursor) =>
            Remove(cursor.Left, cursor.Top);

        public void Remove(int left, int top)
        {
            if (top == 0)
            {
                return;
            }

            if (left == 0)
            {
                Lines[top - 1].AddRange(Lines[top]);
                Lines.RemoveAt(top);
            }
            else
            {
                Lines[top].RemoveAt(left);
            }
        }
        #endregion

        #region Задачи на существование символов в буфере.
        public bool IsCursorOutOfBounds(Cursor cursor, int leftOffset = 0, int topOffset = 0) =>
            IsCursorOutOfBounds(cursor.Left, cursor.Top, leftOffset, topOffset);

        public bool IsCursorOutOfBounds(int left, int top, int leftOffset = 0, int topOffset = 0)
        {
            var x = left + leftOffset;
            var y = top  +  topOffset;
            return
                (y < 0 || Lines.Count    <= y)
             || (x < 0 || Lines[y].Count <= x);
        }
        #endregion

        public string Project(int left, int top, int width, int height)
        {
            var builder = new StringBuilder();
            for (var y = top; y < top + height; y++)
            {
                for (var x = left; x < left + width; x++)
                {
                    builder.Append(this[y, x]);
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}

