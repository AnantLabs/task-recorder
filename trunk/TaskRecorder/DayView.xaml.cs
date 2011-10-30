using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

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

            TaskService.Instance.PropertyChanged += new PropertyChangedEventHandler(TaskService_PropertyChanged);
        }

        public void ReloadView()
        {
            TaskService.Instance.Reload();
        }

        void TaskService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Tasks")
            {
                NotifyPropertyChanged("TotalTime");
            }
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            UnAssignedMinutes += intervalMinutes;
            if (Settings.Instance.NotifyOn)
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

            NotifyPropertyChanged("VisibleIfToday");

            timeSpentSpinnerCol.Visibility = VisibleIfToday;
            timeSpentNoSpinnerCol.Visibility = NotVisibleIfToday;
        }

        public Visibility VisibleIfToday
        {
            get
            {
                return TaskService.Instance.CurrentDate == DateTime.Now.Date ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility NotVisibleIfToday
        {
            get
            {
                return TaskService.Instance.CurrentDate != DateTime.Now.Date ? Visibility.Visible : Visibility.Hidden;
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

    }
}
