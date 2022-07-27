using System;
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

using EncodingConverter.Models;
using EncodingConverter.Windows;

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

        bool TryHandleException(Exception exception)
        {
            if (exception is IOException)
            {
                MessageBox.Show(exception.Message);
                return true;
            }

            if (exception is UnauthorizedAccessException)
            {
                MessageBox.Show(exception.Message);
                return true;
            }

            return false;
        }

        private async void PreviewDecoded_Click(object sender, RoutedEventArgs e)
        {
            var svm = (TextFileViewModel)((FrameworkElement)sender).DataContext;
            Debug.Assert(svm is not null);

            byte[] fileContent;
            try
            {
                fileContent = await File.ReadAllBytesAsync(svm.Path);
            }
            catch (Exception exc) when (TryHandleException(exc))
            {
                return;
            }

            var pvm = new PreviewWindowViewModel(fileContent)
            {
                SelectedEncoding = svm.DetectedEncoding
            };

            var pw = new PreviewWindow
            {
                Owner = this,
                DataContext = pvm
            };

            pw.Title += $" ({svm.Path})";

            pw.ShowDialog();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            FilesListView.SelectedItems
                .OfType<TextFileViewModel>()
                .ToList()
                .ForEach(x => this.Items.Remove(x));
        }
    }
}
