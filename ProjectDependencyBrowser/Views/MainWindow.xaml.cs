﻿//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>http://projectdependencybrowser.codeplex.com/license</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using MyToolkit.Build;
using MyToolkit.Model;
using MyToolkit.Mvvm;
using MyToolkit.Utilities;
using ProjectDependencyBrowser.ViewModels;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListBox = System.Windows.Controls.ListBox;

namespace ProjectDependencyBrowser.Views
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow : Window
    {
        /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class. </summary>
        public MainWindow()
        {
            InitializeComponent();

            ViewModelHelper.RegisterViewModel(Model, this);

            Closed += delegate { Model.CallOnUnloaded(); };
            Activated += delegate { FocusProjectNameFilter(); };

            Model.PropertyChanged += async (sender, args) =>
            {
                if (args.IsProperty<MainWindowModel>(i => i.IsLoaded))
                {
                    Tabs.SelectedIndex = 1;
                    await Task.Delay(250);
                    FocusProjectNameFilter();
                }
            };

            CheckForApplicationUpdate();
        }

        /// <summary>Gets the view model. </summary>
        public MainWindowModel Model
        {
            get { return (MainWindowModel)Resources["ViewModel"]; }
        }

        private void FocusProjectNameFilter()
        {
            Keyboard.Focus(ProjectNameFilter);

            ProjectNameFilter.Focus();
            ProjectNameFilter.SelectAll();
        }

        private void OnProjectNameFilterGotFocus(object sender, RoutedEventArgs e)
        {
            ProjectNameFilter.SelectAll();
        }

        private async void CheckForApplicationUpdate()
        {
            var updater = new ApplicationUpdater(GetType().Assembly, "http://rsuter.com/Projects/ProjectDependencyBrowser/updates.xml");
            await updater.CheckForUpdate(this);
        }

        private void OnSelectDirectory(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            dlg.SelectedPath = Model.RootDirectory;
            dlg.Description = "Select root directory: ";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Model.RootDirectory = dlg.SelectedPath;
        }

        private void OnOpenHyperlink(object sender, RoutedEventArgs e)
        {
            var uri = ((Hyperlink)sender).NavigateUri;
            Process.Start(uri.ToString());
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    e.Handled = true;

                    if (Model.SelectedProjectSolutions.Any())
                        Model.TryOpenSolution(Model.SelectedProjectSolutions.First());
                }
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Key.Down)
                {
                    e.Handled = true;
                    ProjectList.Focus();

                    if (Model.FilteredProjects.Any())
                    {
                        ProjectList.SelectedIndex = 0;
                        ProjectList.ScrollIntoView(ProjectList.SelectedItem);
                        ((ListBoxItem)ProjectList.ItemContainerGenerator.ContainerFromItem(ProjectList.SelectedItem)).Focus();
                    }
                }
            }
        }

        private void OnSolutionDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var solution = (VsSolution)((ListBox) sender).SelectedItem;
            if (solution != null)
                Model.TryOpenSolution(solution);
        }

        private void OnSolutionKeyUp(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    var solution = (VsSolution)((ListBox)sender).SelectedItem;
                    if (solution != null)
                        Model.TryOpenSolution(solution);
                }
            }
        }

        private void OnProjectDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var project = (VsProject)((ListBox)sender).SelectedItem;
            if (project != null)
            {
                Model.SelectProject(project);
                //Dispatcher.InvokeAsync(() =>
                //{
                //    var item = ProjectList.ItemContainerGenerator.ContainerFromIndex(ProjectList.SelectedIndex);
                //    ((ListBoxItem)item).Focus();
                //});
                // TODO: Jump to selected item
            }
        }

        private void OnProjectKeyUp(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    var project = (VsProject)((ListBox)sender).SelectedItem;
                    if (project != null)
                        Model.SelectProject(project);
                }
            }
        }
    }
}
