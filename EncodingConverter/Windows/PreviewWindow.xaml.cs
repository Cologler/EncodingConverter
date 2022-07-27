using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EncodingConverter.Windows;
/// <summary>
/// PreviewWindow.xaml 的交互逻辑
/// </summary>
public partial class PreviewWindow : Window
{
    public PreviewWindow()
    {
        InitializeComponent();

        DecodeEncoding.ItemsSource = EncodingsManager.Encodings;
    }

    private void ChooseButton_Click(object sender, RoutedEventArgs e) => this.DialogResult = true;
}
