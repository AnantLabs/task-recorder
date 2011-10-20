using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace TaskRecorder
{
    [ValueConversion(typeof(DateTime), typeof(String))]
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            return date.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            DateTime resultDateTime;
            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }
            return DependencyProperty.UnsetValue;
        }
    }

    [ValueConversion(typeof(Int32), typeof(String))]
    public class MinutesToTimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Int32 number = (Int32)value;
            int hours = number / 60;
            double remainderHours = number / 60.0 - hours;
            int minutes = (int) Math.Round(remainderHours * 60);

            if (hours != 0 && minutes != 0)
            {
                return hours + "h " + minutes + "m";
            }
            else if (hours != 0)
            {
                return hours + "h";
            }
            else if (minutes != 0)
            {
                return minutes + "m";
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
