using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
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

                await Task.WhenAll(vms.Select(z => z.LoadFileAsync()).ToArray());

                var withErrors = vms
                    .Where(x => x.LoadFileException is not null)
                    .ToList();

                if (withErrors.Any())
                {
                    var nl = Environment.NewLine;
                    MessageBox.Show(string.Join(nl, withErrors.Select(x => $"Unable read file {x.Path}:{nl}{x.LoadFileException.Message}{nl}")));
                }
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

        private void PreviewDecoded_Click(object sender, RoutedEventArgs e)
        {
            var sourceViewModel = (TextFileViewModel)((FrameworkElement)sender).DataContext;

            if (sourceViewModel.DecodeSource is null)
                return;

            var previewViewModel = new PreviewWindowViewModel(sourceViewModel.DecodeSource)
            {
                SelectedEncoding = sourceViewModel.SourceEncoding
            };

            var previewWindow = new PreviewWindow
            {
                Owner = this,
                DataContext = previewViewModel
            };

            previewWindow.Title += $" ({sourceViewModel.Path})";

            if (previewWindow.ShowDialog() == true)
            {
                Debug.Assert(
                    previewViewModel.SelectedEncoding is not null || sourceViewModel.SourceEncoding is null, 
                    "if source encoding not null, selected encoding would not be null.");
                sourceViewModel.SourceEncoding = previewViewModel.SelectedEncoding;
            }
        }

        private void PreviewEncodings_Click(object sender, RoutedEventArgs e)
        {
            var sourceViewModel = (TextFileViewModel)((FrameworkElement)sender).DataContext;

            if (sourceViewModel.DecodeSource is null)
                return;

            var previewViewModel = new PreviewEncodingsWindowViewModel(sourceViewModel.DecodeSource);

            previewViewModel.LoadEncodings();

            var previewWindow = new PreviewEncodingsWindow
            {
                Owner = this,
                DataContext = previewViewModel
            };

            previewWindow.Title += $" ({sourceViewModel.Path})";

            if (previewWindow.ShowDialog() == true)
            {
                if (previewViewModel.SelectedPreviewItem?.Encoding is { } encoding)
                {
                    sourceViewModel.SourceEncoding = encoding;
                }
            }
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
