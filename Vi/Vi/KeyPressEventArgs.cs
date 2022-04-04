using System;

namespace Task.Vi
{
    public sealed class KeyPressEventArgs : EventArgs
    {
        public char Key
        {
            get;
        }

        public KeyPressEventArgs(char key)
        {
            Key = key;
        }
    }
}

