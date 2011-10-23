using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

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
            int minutes = (int)Math.Round(remainderHours * 60);

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
            string text = (string)value;

            string regexHours = "(?<hours>[0-9]+)\\s*h";
            string regexMins = "(?<minutes>[0-9]+)\\s*m";
            string pattern = "^\\s*(" + regexHours + ")?\\s*(" + regexMins + ")?\\s*$";

            Match match = Regex.Match(text, pattern);
            if (match.Success)
            {
                string hoursText = match.Groups["hours"].Value;
                string minutesText = match.Groups["minutes"].Value;

                int hours = hoursText != string.Empty ? int.Parse(hoursText) : 0;
                int minutes = minutesText != string.Empty ? int.Parse(minutesText) : 0;

                if (hours > 0 || minutes > 0)
                {
                    return hours * 60 + minutes;
                }
                else
                {
                    return value;
                }
            }
            else
            {
                return value;
            }
        }
    }
}
