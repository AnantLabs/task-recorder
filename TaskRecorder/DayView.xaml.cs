using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Win32;

namespace TaskRecorder
{
    /// <summary>
    /// Interaction logic for DayView.xaml
    /// </summary>
    public partial class DayView : UserControl, INotifyPropertyChanged
    {
        private readonly int intervalMinutes = 15;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public event PropertyChangedEventHandler PropertyChanged;

        public DayView()
        {
            InitializeComponent();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, intervalMinutes, 0);
        }

        public bool NotifyOn { get; set; }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            UnAssignedMinutes += intervalMinutes;
            if (NotifyOn)
            {
                Notify();
            }
        }

        private void Notify()
        {
            Window window = Window.GetWindow(this);
            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }
            window.Activate();
        }

        private int unAssignedMinutes;
        public int UnAssignedMinutes
        {
            get { return unAssignedMinutes; }
            set
            {
                unAssignedMinutes = value;
                NotifyPropertyChanged("UnAssignedMinutes");
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            TaskService.Instance.Tasks.Add(new Task { Name = "Unnamed", Date = DateTime.Now });
        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Start();
            NotifyTimerEvents();
        }

        private void PauseTimer_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            NotifyTimerEvents();
        }

        private void ResetTimer_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            UnAssignedMinutes = 0;
            NotifyTimerEvents();
        }

        private void NotifyTimerEvents()
        {
            NotifyPropertyChanged("TimerRunning");
            NotifyPropertyChanged("TimerNotRunning");
        }

        public bool TimerRunning
        {
            get
            {
                return dispatcherTimer.IsEnabled;
            }
        }

        public bool TimerNotRunning
        {
            get
            {
                return !TimerRunning;
            }
        }

        private void DateSpinner_Spin(object sender, Microsoft.Windows.Controls.SpinEventArgs e)
        {
            Microsoft.Windows.Controls.ButtonSpinner spinner = (Microsoft.Windows.Controls.ButtonSpinner)sender;
            int days = e.Direction == Microsoft.Windows.Controls.SpinDirection.Increase ? 1 : -1;
            TaskService.Instance.CurrentDate = TaskService.Instance.CurrentDate.AddDays(days);

            NotifyPropertyChanged("TimerVisibility");
            NotifyPropertyChanged("TotalTime");
        }

        public Visibility TimerVisibility
        {
            get
            {
                return TaskService.Instance.CurrentDate == DateTime.Now.Date ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public int TotalTime
        {
            get
            {
                int counter = 0;
                foreach (Task task in TaskService.Instance.Tasks)
                {
                    counter += task.Time;
                }
                return counter;
            }
        }

        private void TimeSpinner_Spin(object sender, Microsoft.Windows.Controls.SpinEventArgs e)
        {
            Microsoft.Windows.Controls.ButtonSpinner spinner = (Microsoft.Windows.Controls.ButtonSpinner)sender;
            Task task = (Task)spinner.DataContext;

            if (e.Direction == Microsoft.Windows.Controls.SpinDirection.Increase)
            {
                if (task.IsToday)
                {
                    if (UnAssignedMinutes >= intervalMinutes)
                    {
                        task.Time += intervalMinutes;
                        UnAssignedMinutes -= intervalMinutes;
                    }
                }
                else
                {
                    task.Time += intervalMinutes;
                }
            }
            else
            {
                if (task.Time >= intervalMinutes)
                {
                    task.Time -= intervalMinutes;
                    if (task.IsToday)
                    {
                        UnAssignedMinutes += intervalMinutes;
                    }
                }
            }

            NotifyPropertyChanged("TotalTime");
        }

        private void dgData_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == DataGrid.DeleteCommand)
            {
                DataGrid dg = (DataGrid)e.Source;
                Task task = (Task)dg.CurrentItem;

                if (MessageBox.Show("Delete task \"" + task.Name + "\"?", "Confirm delete", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Handled = true;
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
    }
}
