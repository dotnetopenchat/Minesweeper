namespace MinesweeperHost
{
    using System;
    using System.Windows.Data;

    public class BooleanReverseConverter : IValueConverter
    {
        #region IValueConverter 구현
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                return !(bool)value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                return !(bool)value;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion  // IValueConverter 구현
    }
}
