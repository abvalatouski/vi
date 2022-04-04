using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Task.Vi;

namespace Task
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Editor _vi;

        #region Свойства.
        private string _statusBar;

        public string StatusBar
        {
            get => _statusBar;
            set
            {
                _statusBar = value;
                OnPropertyChanged(nameof(StatusBar));
            }
        }
        #endregion

        public MainWindow()
        {
            _statusBar = string.Empty;
            InitializeComponent();
        }

        #region Обновление привязок.
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region Обработчики.
        private void Vi_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_vi == null)
            {
                return;
            }

            char key = e.Text[0];
            _vi.OnKeyPress(sender, new KeyPressEventArgs(key));
            UpdateScreen();
        }

        private void UpdateScreen()
        {
            StatusBar = _vi.StatusBar;

            new TextRange(
                PhysicalScreen.Document.ContentStart,
                PhysicalScreen.Document.ContentEnd
            ) { Text = _vi.GetLogicalScreen() };
            const int MagicAdjustmentForRichTextBox = 2;
            var cursor = new TextRange(
                PhysicalScreen.Document.ContentStart.GetPositionAtOffset(_vi.CursorOffset + MagicAdjustmentForRichTextBox),
                PhysicalScreen.Document.ContentStart.GetPositionAtOffset(_vi.CursorOffset + MagicAdjustmentForRichTextBox + 1)
            );
            cursor.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.White));
            cursor.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Black));

            PhysicalScreen.UpdateLayout();
        }

        private void MenuItem_Create_Click(object sender, RoutedEventArgs e) =>
            WithAskedFile(doesFileExist: false, path =>
            {
                _vi = new Editor(path, createNewFile: true);
                UpdateScreen();
            });

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e) =>
            WithAskedFile(doesFileExist: true, path =>
            {
                _vi = new Editor(path);
                UpdateScreen();
            });

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion

        private void WithAskedFile(bool doesFileExist, Action<string> action)
        {
            var dialog = doesFileExist
                ? (FileDialog)(new OpenFileDialog())
                : (FileDialog)(new SaveFileDialog());

            const bool IsFileSelected = true;
            if (dialog.ShowDialog() == IsFileSelected)
            {
                string path = dialog.FileName;
                action(path);
            }
        }
    }
}

