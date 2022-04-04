using static System.Math;

namespace Task.Vi
{
    public sealed class Editor
    {
        private readonly Buffer _buffer;
        private readonly int    _windowWidth;
        private readonly int    _windowHeight;
        private          Mode   _mode;
        private          int    _left;
        private          int    _top;
        private          Cursor _cursor;

        public string StatusBar =>
            (new StatusBar(_mode, _cursor, _buffer.Filename)).ToString();

        public int CursorOffset
        {
            get
            {
                const int SpaceForNewline = 1, MagicAdjustmentForRichTextBox = 3;
                return (_cursor.Top - _top) * (_windowWidth + SpaceForNewline + MagicAdjustmentForRichTextBox) + (_cursor.Left - _left);
            }
        }

        public Editor(string path, int windowWidth = 80, int windowHeight = 24, bool createNewFile = false)
        {
            _buffer       = new Buffer(path, createNewFile);
            _windowWidth  = windowWidth;
            _windowHeight = windowHeight;
            _mode         = Mode.Normal;
            _left         = 0;
            _top          = 0;
            _cursor       = new Cursor(_left, _top);
        }

        // Вся магия происходит здесь.
        public void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            switch (_mode)
            {
                case Mode.Normal:
                    switch (e.Key)
                    {
                        case 'i':
                            _mode = Mode.Insert;
                            break;
                        case 'I':
                            _mode = Mode.Insert;
                            _cursor.Left = 0;
                            break;
                        case 'a':
                            _mode = Mode.Insert;
                            MoveRight(forceMoveToVacantPlace: true);
                            break;
                        case 'A':
                            _mode = Mode.Insert;
                            _cursor.Left = _buffer.Lines[_cursor.Top].Count;
                            break;

                        case 'h':
                            MoveLeft();
                            break;
                        case 'j':
                            MoveDown();
                            break;
                        case 'k':
                            MoveUp();
                            break;
                        case 'l':
                            MoveRight();
                            break;
                    }
                    break;

                case Mode.Insert:
                    switch (e.Key)
                    {
                        // <Esc>.
                        case '\u001b':
                            _mode = Mode.Normal;
                            break;
                        // <LF/CRLF>
                        case '\n':
                        case '\r':
                            _buffer.Insert(_cursor, e.Key);
                            MoveDown();
                            _cursor.Left = 0;
                            break;
                        // <Backspace>
                        case '\b':
                            if (_cursor.Left == 0 && _cursor.Top != 0)
                            {
                                int anchor = _buffer.Lines[_cursor.Top - 1].Count;
                                _buffer.Lines[_cursor.Top - 1].AddRange(_buffer.Lines[_cursor.Top]);
                                _buffer.Lines.RemoveAt(_cursor.Top);
                                _cursor.Left = anchor;
                                _cursor.Top--;
                            }
                            else if (_cursor.Left != 0)
                            {
                                _buffer.Lines[_cursor.Top].RemoveAt(_cursor.Left - 1);
                                _cursor.Left--;
                            }
                            break;

                        default:
                            _buffer.Insert(_cursor, e.Key);
                            MoveRight(forceMoveToVacantPlace: true);
                            break;
                    }
                    break;
            }
        }

        #region Перемещение.
        private void MoveLeft()
        {
            if (_cursor.Left != 0)
            {
                _cursor.Left--;
            }
            
            if (_cursor.Left < _left)
            {
                _left = _cursor.Left;
            }
        }

        private void MoveRight(bool forceMoveToVacantPlace = false)
        {
            if (!_buffer.IsCursorOutOfBounds(_cursor, leftOffset: 1)
                || (forceMoveToVacantPlace && _cursor.Left + 1 <= _buffer.Lines[_cursor.Top].Count))
            {
                _cursor.Left++;
            }
            
            if (_cursor.Left >= _left + _windowWidth)
            {
                _left = _cursor.Left - _windowWidth + 1;
            }
        }

        private void MoveUp()
        {
            if (!_buffer.IsCursorOutOfBounds(_cursor, topOffset: -1))
            {
                _cursor.Top--;
            }
            else if (_cursor.Top - 1 >= 0)
            {
                _cursor.Top--;
                _cursor.Left = Max(_left, _buffer.Lines[_cursor.Top].Count - 1);
            }
            
            if (_cursor.Top < _top)
            {
                _top = _cursor.Top;
            }
        }

        private void MoveDown()
        {
            if (!_buffer.IsCursorOutOfBounds(_cursor, topOffset: 1))
            {
                _cursor.Top++;
            }
            else if (_cursor.Top + 1 < _buffer.Lines.Count)
            {
                _cursor.Top++;
                _cursor.Left = Max(_left, _buffer.Lines[_cursor.Top].Count - 1);
            }
            
            if (_cursor.Top >= _top + _windowHeight)
            {
                _top = _cursor.Top - _windowHeight + 1;
            }
        }
        #endregion

        public string GetLogicalScreen() =>
            _buffer.Project(_left, _top, _windowWidth, _windowHeight);
    }
}

