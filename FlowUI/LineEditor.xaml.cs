using FlowCompiler;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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
            (DataContext as LineEditorViewModel)!.NotifyCodeChanged += () =>
            {
                _textDisplay.Inlines.Clear();

                foreach (var token in (DataContext as LineEditorViewModel)!.Tokens)
                {
                    _textDisplay.Inlines.Add(token is ErrorToken error ?
                    new Underline(
                        new Run(token.Value) {
                            Foreground = Brushes.Black,
                            ToolTip = new Label() { Content = error.Value } })
                    { Foreground = Brushes.Red } :
                    new Run
                    {
                        Foreground = token switch
                        {
                            Keyword => Brushes.Blue,
                            FlowCompiler.Name => Brushes.Green,
                            _ => Brushes.Black
                        },
                        Text = token.Value
                    });
                    _textDisplay.Inlines.Add(" ");
                }
            };
        }
    }
}
