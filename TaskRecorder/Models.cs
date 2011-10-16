using System;
using System.ComponentModel;

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
}
