﻿using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using FunkyGrep.UI.Util;
using FunkyGrep.UI.ViewModels;
using Prism.Validation;

namespace FunkyGrep.UI.Views
{
    public partial class MainWindow
    {
        SearchOperationViewModel _lastSearchOperation;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DataContextProperty)
            {
                if (e.OldValue != null && e.OldValue is MainWindowViewModel oldViewModel)
                {
                    oldViewModel.Search.PropertyChanged -= this.HandleSearchPropertyChanged;
                    oldViewModel.ErrorsChanged -= this.HandleModelPropertyChanged;
                }

                if (e.NewValue != null && e.NewValue is MainWindowViewModel newViewModel)
                {
                    this.HandleSearchPropertyChanged(
                        newViewModel.Search,
                        new PropertyChangedEventArgs(nameof(newViewModel.Search.Operation)));
                    newViewModel.Search.PropertyChanged += this.HandleSearchPropertyChanged;
                    newViewModel.ErrorsChanged += this.HandleModelPropertyChanged;
                }
            }

            base.OnPropertyChanged(e);
        }

        void HandleSearchPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var searchViewModel = (SearchViewModel)sender;

            if (e.PropertyName != nameof(searchViewModel.Operation))
            {
                return;
            }

            if (this._lastSearchOperation != null)
            {
                BindingOperations.DisableCollectionSynchronization(this._lastSearchOperation.Results);
                BindingOperations.DisableCollectionSynchronization(this._lastSearchOperation.SearchErrors);
            }

            BindingOperations.EnableCollectionSynchronization(
                searchViewModel.Operation.Results,
                searchViewModel.Operation.ResultsLocker);
            BindingOperations.EnableCollectionSynchronization(
                searchViewModel.Operation.SearchErrors,
                searchViewModel.Operation.SearchErrorsLocker);

            this._lastSearchOperation = searchViewModel.Operation;
        }

        void HandleModelPropertyChanged(object sender, DataErrorsChangedEventArgs e)
        {
            var viewModel = (BindableValidator)sender;

            if (e.PropertyName.Length == 0 && viewModel.Errors[string.Empty].Count > 0)
            {
                this.Dispatcher?.Invoke(
                    () =>
                        MessageBox.Show(
                            this,
                            viewModel.Errors[string.Empty][0],
                            "Error during search",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error));
            }
        }

        void HandleDirectoryAutoCompleteBoxPopulating(object sender, PopulatingEventArgs e)
        {
            var autoCompleteBox = (AutoCompleteBox)sender;
            string text = autoCompleteBox.Text;
            string[] subDirectories = null;

            if (!string.IsNullOrWhiteSpace(text))
            {
                string directoryName = Path.GetDirectoryName(text);
                if (DirectoryUtil.ExistsOrNullIfTimeout(directoryName ?? text, TimeSpan.FromSeconds(2)) ?? false)
                {
                    try
                    {
                        subDirectories = Directory.GetDirectories(
                            directoryName ?? text,
                            "*",
                            SearchOption.TopDirectoryOnly);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            autoCompleteBox.ItemsSource = subDirectories;
            autoCompleteBox.PopulateComplete();
        }

        void HandleDataGridPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var depObj = (DependencyObject)e.OriginalSource;

            if (depObj is Inline inline)
            {
                depObj = inline.Parent;
                if (depObj == null)
                {
                    return;
                }
            }

            while (depObj != null)
            {
                depObj = VisualTreeHelper.GetParent(depObj);

                if (depObj is DataGridColumnHeader)
                {
                    e.Handled = true;
                    return;
                }

                if (depObj is DataGridRow dataGridRow)
                {
                    dataGridRow.IsSelected = true;
                    return;
                }
            }
        }

        void HandleDataGridRowPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = (DataGridRow)sender;
            DragDrop.DoDragDrop(
                row,
                new DataObject(DataFormats.FileDrop, new[] { ((IFileItem)row.Item).AbsoluteFilePath }),
                DragDropEffects.All);
        }

        void HandleDataGridRowPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = (DataGridRow)sender;
            var fileItem = (IFileItem)row.Item;
            var viewModel = (MainWindowViewModel)this.DataContext;

            var parameters = new OpenFileInEditorParameters
            {
                FileItem = fileItem,
                Editor = viewModel.Settings.Editors[viewModel.Settings.DefaultEditorIndex]
            };
            
            if (viewModel.OpenFileInEditorCommand.CanExecute(parameters))
            {
                viewModel.OpenFileInEditorCommand.Execute(parameters);
            }
        }

        void HandleMainWindowClosing(object sender, CancelEventArgs e)
        {
            var viewModel = (MainWindowViewModel)this.DataContext;
            viewModel.SaveSettings();
        }
    }
}
