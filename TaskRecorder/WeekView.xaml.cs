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
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Itenso.TimePeriod;

namespace TaskRecorder
{
    /// <summary>
    /// Interaction logic for WeekView.xaml
    /// </summary>
    public partial class WeekView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public WeekView()
        {
            DateTime now = DateTime.Now.Date;
            Year = now.Year;
            Week = GetWeekNumber(now);
            InitializeComponent();
        }

        private int year;
        public int Year
        {
            get
            {
                return year;
            }
            set
            {
                year = Math.Max(2000, value);
                NotifyPropertyChanged("Year");
                NotifyPropertyChanged("WeekStartDate");
                NotifyPropertyChanged("WeekEndDate");
            }
        }

        private int week;
        public int Week
        {
            get
            {
                return week;
            }
            set
            {
                week = value;
                NotifyPropertyChanged("Week");
                NotifyPropertyChanged("WeekStartDate");
                NotifyPropertyChanged("WeekEndDate");
            }
        }

        public DateTime WeekStartDate
        {
            get
            {
                return new Week(Year, Week).Start;
            }
        }

        private int WeeksPerYear(int year)
        {
            DateTime lastDay = new DateTime(year, 12, 28);
            return Calendar.GetWeekOfYear(lastDay, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private System.Globalization.Calendar Calendar
        {
            get
            {
                return new CultureInfo("fi-FI").Calendar;
            }
        }

        public DateTime WeekEndDate
        {
            get
            {
                return WeekStartDate.AddDays(6);
            }
        }

        private int GetWeekNumber(DateTime date)
        {
            return Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void SpinYear(object sender, Microsoft.Windows.Controls.SpinEventArgs e)
        {
            Microsoft.Windows.Controls.ButtonSpinner spinner = (Microsoft.Windows.Controls.ButtonSpinner)sender;
            int change = e.Direction == Microsoft.Windows.Controls.SpinDirection.Increase ? 1 : -1;
            Year += change;

            if (Week == 53 && WeeksPerYear(Year) == 52)
            {
                Week = 52;
            }
        }

        private void SpinWeek(object sender, Microsoft.Windows.Controls.SpinEventArgs e)
        {
            Microsoft.Windows.Controls.ButtonSpinner spinner = (Microsoft.Windows.Controls.ButtonSpinner)sender;
            int change = e.Direction == Microsoft.Windows.Controls.SpinDirection.Increase ? 1 : -1;
            int newWeek = Week + change;
            if (newWeek == 0)
            {
                Year -= 1;
                Week = WeeksPerYear(Year);
            }
            else if (newWeek > WeeksPerYear(Year))
            {
                Year += 1;
                Week = 1;
            }
            else
            {
                Week = newWeek;
            }
        }


    }
}
