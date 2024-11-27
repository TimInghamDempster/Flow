using System.Windows;
using System.Windows.Controls;

namespace FlowUI
{
    /// <summary>
    /// Interaction logic for LineEditor.xaml
    /// </summary>
    public partial class LineEditor : UserControl
    {
        public LineEditor()
        {
            InitializeComponent();
        }

        protected void OnLoaded(object sender, RoutedEventArgs e)
        {
            (DataContext as CodeEditorViewModel)!.NotifyCodeChanged += () =>
                _textDisplay.Text = (DataContext as CodeEditorViewModel)!.Code;
        }
    }
}
