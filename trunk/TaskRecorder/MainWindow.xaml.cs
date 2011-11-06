using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace TaskRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                TabControl tabControl = (TabControl)e.Source;
                TabItem tab = (TabItem)tabControl.SelectedItem;

                if (weekTab.IsSelected)
                {
                    WeekView weekView = (WeekView)tab.Content;
                    weekView.ReloadView();
                }
                else if (dayTab.IsSelected)
                {
                    DayView dayView = (DayView)tab.Content;
                    dayView.ReloadView();
                }
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "export";
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Text files (.csv)|*.csv";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                try
                {
                    TaskService.Instance.ExportCSV(filename);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            MessageBox.Show(Resource.AboutText, Resource.ApplicationName + " " + version.ToString(2), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            WindowState = WindowState.Minimized;
            e.Cancel = true;
        }
    }
}
