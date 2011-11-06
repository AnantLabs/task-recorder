using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
                return CultureInfo.CurrentCulture.Calendar;
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

            NotifyPropertyChanged("WeekReportRows");
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

            NotifyPropertyChanged("WeekReportRows");
        }

        public void ReloadView()
        {
            NotifyPropertyChanged("WeekReportRows");
        }

        public IList<WeekReportRow> WeekReportRows
        {
            get
            {
                try
                {
                    IList<Task> tasks = TaskService.Instance.FindByDateRange(WeekStartDate, WeekEndDate);
                    ICollection<WeekReportRow> result = SortByColumnAndDirection(BuildReportRows(tasks));
                    return AddTotalRow(result);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return new List<WeekReportRow>();
                }
            }
        }

        private ICollection<WeekReportRow> SortByColumnAndDirection(ICollection<WeekReportRow> rows)
        {
            List<WeekReportRow> result = new List<WeekReportRow>();
            result.AddRange(rows);
            result.Sort(rowComparer);
            return result;
        }

        private bool groupByCategory = false;
        public bool GroupByCategory
        {
            get
            {
                return groupByCategory;
            }
            set
            {
                groupByCategory = value;
                nameColumn.Visibility = groupByCategory ? Visibility.Hidden : Visibility.Visible;
                ReloadView();
            }
        }

        private ICollection<WeekReportRow> BuildReportRows(IList<Task> tasks)
        {
            Dictionary<string, WeekReportRow> map = new Dictionary<string, WeekReportRow>();

            foreach (Task task in tasks)
            {
                DayOfWeek day = task.Date.DayOfWeek;

                string key = GroupByCategory ? task.Category : task.Name + "/" + task.Category;
                WeekReportRow row;

                if (!map.TryGetValue(key, out row))
                {
                    if (GroupByCategory)
                    {
                        row = new WeekReportRow() { Name = "", Category = task.Category };
                    }
                    else
                    {
                        row = new WeekReportRow() { Name = task.Name, Category = task.Category };
                    }
                    map[key] = row;
                }

                row[day] += task.Time;
            }

            return map.Values;
        }

        private IList<WeekReportRow> AddTotalRow(ICollection<WeekReportRow> rows)
        {
            WeekReportRow totalRow = GetTotalRow(rows);

            List<WeekReportRow> result = new List<WeekReportRow>();
            result.AddRange(rows);
            result.Add(totalRow);
            return result;
        }

        private WeekReportRow GetTotalRow(ICollection<WeekReportRow> rows)
        {
            WeekReportRow totalRow = new WeekReportRow() { Name = "Total" };
            if (GroupByCategory)
            {
                totalRow.Category = "Total";
            }

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                totalRow[day] = rows.Sum(row => row[day]);
            }

            return totalRow;
        }

        private RowComparer rowComparer = new RowComparer() { PropertyName = "Name", SortDirection = ListSortDirection.Ascending };

        public class RowComparer : IComparer<WeekReportRow>
        {
            public string PropertyName { get; set; }
            public ListSortDirection SortDirection { get; set; }

            public int Compare(WeekReportRow x, WeekReportRow y)
            {
                IComparable valX = GetPropValue(x, PropertyName);
                IComparable valY = GetPropValue(y, PropertyName);

                return SortDirection == ListSortDirection.Ascending ? valX.CompareTo(valY) : valY.CompareTo(valX);
            }

            private IComparable GetPropValue(WeekReportRow row, string propName)
            {
                // reflection approach would be "cleaner" but hinders clarity...
                switch (propName)
                {
                    case "Name": return row.Name;
                    case "Category": return row.Category;
                    case "Total": return row.Total;
                    case "[0]": return row[DayOfWeek.Sunday];
                    case "[1]": return row[DayOfWeek.Monday];
                    case "[2]": return row[DayOfWeek.Tuesday];
                    case "[3]": return row[DayOfWeek.Wednesday];
                    case "[4]": return row[DayOfWeek.Thursday];
                    case "[5]": return row[DayOfWeek.Friday];
                    case "[6]": return row[DayOfWeek.Saturday];
                    default: return null;
                }
            }
        }

        private ListSortDirection direction = ListSortDirection.Ascending;

        private void weekGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            DataGridColumn column = e.Column;

            rowComparer = new RowComparer() { PropertyName = column.SortMemberPath, SortDirection = direction };

            NotifyPropertyChanged("WeekReportRows");

            foreach (DataGridColumn c in weekGrid.Columns)
            {
                c.SortDirection = null;
            }
            column.SortDirection = direction;
            direction = direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        }
    }
}
