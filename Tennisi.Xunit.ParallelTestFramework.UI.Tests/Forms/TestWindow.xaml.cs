using System.Windows;
using System.Windows.Controls;

namespace Tennisi.Xunit.ParallelTestFramework.UI.Tests.Forms;

/// <summary>
/// Interaction logic for TestWindow.xaml
/// </summary>
public partial class TestWindow : Window
{
    public string OutputText { get; private set; }

    public TestWindow()
    {
        InitializeComponent();
        SampleButton.Click += (sender, args) => OutputText = SampleTextBox.Text;
    }
    
    public new TextBox PublicTextBox { get => FindName("SampleTextBox") as TextBox; }
    public new Button PublicButton { get => FindName("SampleButton") as Button; }
}