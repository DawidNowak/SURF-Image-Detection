﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageRecognition.Converters
{
    public class InvertBooleanToVisibilityConverter : IValueConverter
    {
	    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	    {
		    if (value is bool b && !b) return Visibility.Visible;
		    return Visibility.Collapsed;
	    }

	    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	    {
		    throw new NotImplementedException();
	    }
    }
}
