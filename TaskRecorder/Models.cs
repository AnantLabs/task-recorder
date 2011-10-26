using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace TaskRecorder
{
    public class Task : INotifyPropertyChanged
    {
        public string Id { get; set; }

        private DateTime date = DateTime.Now.Date;
        public DateTime Date
        {
            get { return date; }
            set
            {
                date = value.Date;
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private string category;
        public string Category
        {
            get { return category != null ? category : ""; }
            set
            {
                category = value;
                NotifyPropertyChanged("Category");
            }
        }

        private int time;
        public int Time
        {
            get { return time; }
            set
            {
                this.time = value;
                NotifyPropertyChanged("Time");
            }
        }

        public bool IsToday
        {
            get
            {
                return DateTime.Now.Date == Date;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }


    public class WeekReportRow
    {
        public string Name { get; set; }
        public string Category { get; set; }

        private Dictionary<DayOfWeek, int> dayValues = new Dictionary<DayOfWeek, int>()
        {
            {DayOfWeek.Monday, 0},
            {DayOfWeek.Tuesday, 0},
            {DayOfWeek.Wednesday, 0},
            {DayOfWeek.Thursday, 0},
            {DayOfWeek.Friday, 0},
            {DayOfWeek.Saturday, 0},
            {DayOfWeek.Sunday, 0},
        };

        public Dictionary<DayOfWeek, int> DayValues
        {
            get { return dayValues; }
        }

        public int this[DayOfWeek key]
        {
            get
            {
                return dayValues[key];
            }
            set
            {
                dayValues[key] = value;
            }
        }

        public int Total
        {
            get
            {
                return dayValues.Values.Sum();
            }
        }
    }

    public class Settings
    {
        private static readonly Settings instance = new Settings() { NotifyOn = true };

        private Settings() { }

        public static Settings Instance { get { return instance; } }

        public bool NotifyOn { get; set; }

    }
}
