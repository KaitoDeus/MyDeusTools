using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyDeusTools.App.ViewModels;

namespace MyDeusTools.App.Views.Pages
{
    public partial class PdfToolkitPage : Page
    {
        public PdfToolkitViewModel ViewModel { get; }

        public PdfToolkitPage(PdfToolkitViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }

        private void OnMergeDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    ViewModel.AddMergeFilePaths(files);
                }
            }
        }

        private void OnSplitDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var pdf = files?.FirstOrDefault(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(pdf))
                {
                    ViewModel.LoadSplitSource(pdf);
                }
            }
        }

        private void OnImagesDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    ViewModel.AddImageFilePaths(files);
                }
            }
        }

        private void OnProtectDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var pdf = files?.FirstOrDefault(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(pdf))
                {
                    ViewModel.LoadProtectSource(pdf);
                }
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
    }
}
