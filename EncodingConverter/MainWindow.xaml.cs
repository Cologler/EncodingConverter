using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public MainWindow()
        {
            this.InitializeComponent();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ComboBoxItem selected = null;
            var encodings = Encoding.GetEncodings().Select(z => z.GetEncoding());
            foreach (var encoding in encodings)
            {
                var encodingItem = new ComboBoxItem
                {
                    Content = encoding.EncodingName,
                    DataContext = encoding
                };
                if (encoding == Encoding.UTF8)
                {
                    selected = encodingItem;
                }
                this.TargetEncoding.Items.Add(encodingItem);
            }
            this.TargetEncoding.SelectedItem = selected ?? this.TargetEncoding.Items.OfType<object>().FirstOrDefault();

            this.FilesListView.ItemsSource = this.Items;
        }

        public Encoding SelectedEncoding
        {
            get
            {
                return (Encoding)((ComboBoxItem)this.TargetEncoding.SelectedItem).DataContext;
            }
        }

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

        private void ConvertAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in this.Items.Where(z => z.IsEnabledConvert))
            {
                _ = item.ConvertAsync(this.SelectedEncoding, this.ToNewFile);
            }
        }
    }
}
