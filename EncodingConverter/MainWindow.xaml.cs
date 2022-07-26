using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using UtfUnknown;

namespace EncodingConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Encoding[] Encodings { get; }

        static MainWindow()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Encodings = Encoding.GetEncodings()
                .Select(z => z.GetEncoding())
                .OrderBy(x => x.EncodingName)
                .ToArray();

            Debug.Assert(Encodings.Contains(Encoding.UTF8));
        }

        public MainWindow()
        {
            this.InitializeComponent();

            this.TargetEncoding.ItemsSource = Encodings;
            this.TargetEncoding.SelectedItem = Encoding.UTF8;

            this.FilesListView.ItemsSource = this.Items;
        }

        public Encoding SelectedEncoding => (Encoding)this.TargetEncoding.SelectedItem;

        public bool ToNewFile => this.ToNewFileCheckBox.IsChecked != false;

        private async void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                var vms = paths.Where(z => File.Exists(z)).Select(z => new TextFileViewModel(z)).ToList();
                vms.ForEach(this.Items.Add);
                await Task.WhenAll(vms.Select(z => z.DetectAsync()).ToArray());
            }
        }

        ObservableCollection<TextFileViewModel> Items { get; } = new ObservableCollection<TextFileViewModel>();

        private async void SingleConvertButton_Click(object sender, RoutedEventArgs e)
        {
            await ((TextFileViewModel)((FrameworkElement)sender).DataContext).ConvertAsync(this.SelectedEncoding, this.ToNewFile);
        }

        private void ConvertSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in this.FilesListView.SelectedItems.OfType<TextFileViewModel>().Where(z => z.IsEnabledConvert))
            {
                _ = item.ConvertAsync(this.SelectedEncoding, this.ToNewFile);
            }
        }
    }
}
