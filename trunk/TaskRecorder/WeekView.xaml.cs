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
                year = value;
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
                return FirstDateOfWeek(Year, Week).Date;
            }
        }

        private DateTime FirstDateOfWeek(int year, int weekNum)
        {
            DateTime jan1 = new DateTime(year, 1, 1);

            int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
            DateTime firstMonday = jan1.AddDays(daysOffset);

            CultureInfo myCI = new CultureInfo("en-US");
            var cal = myCI.Calendar;
            int firstWeek = cal.GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            DateTime result = firstMonday.AddDays(weekNum * 7);

            return result;
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
            CultureInfo myCI = new CultureInfo("en-US");
            return myCI.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
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
        }

        private void SpinWeek(object sender, Microsoft.Windows.Controls.SpinEventArgs e)
        {
            Microsoft.Windows.Controls.ButtonSpinner spinner = (Microsoft.Windows.Controls.ButtonSpinner)sender;
            int change = e.Direction == Microsoft.Windows.Controls.SpinDirection.Increase ? 1 : -1;
            Week += change;
        }


    }
}
