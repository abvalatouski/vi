namespace Task.Vi
{
    public struct Cursor
    {
        public int Left
        {
            get;
            set;
        }

        public int Top
        {
            get;
            set;
        }

        public Cursor(int left, int top)
        {
            Left = left;
            Top  = top;
        }
    }
}

