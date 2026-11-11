using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;
using ModForge.Core;

namespace ModForge
{
    public partial class MainWindow : Window
    {
        public MainWindow() : this(null) { }

        /// <summary>Pass a view model to draw the window around prepared data (the screenshot mode does).</summary>
        public MainWindow(MainVM vm)
        {
            InitializeComponent();
            if (vm == null)
            {
                string info;
                Pack pack;
                try { pack = PackStore.LoadAll(out info); }
                catch (Exception ex)
                {
                    MessageBox.Show("The templates could not be loaded:\n" + ex.Message, "ModForge", MessageBoxButton.OK, MessageBoxImage.Error);
                    pack = new Pack();
                    info = "no templates";
                }
                vm = new MainVM(pack, info, Settings.Load(), false);
            }
            vm.PickFolder = (title, start) => FolderPicker.Pick(this, title, start);
            vm.PickZipPath = (dir, name) =>
            {
                var dlg = new SaveFileDialog { FileName = name, Filter = "Zip file|*.zip", OverwritePrompt = true, AddExtension = true, DefaultExt = ".zip" };
                try { if (System.IO.Directory.Exists(dir)) dlg.InitialDirectory = dir; } catch (Exception) { }
                return dlg.ShowDialog(this) == true ? dlg.FileName : null;
            };
            DataContext = vm;
            StateChanged += (s, e) => Root.Margin = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
        }

        void Minimize_Click(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }
        void Maximize_Click(object sender, RoutedEventArgs e) { WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; }
        void Close_Click(object sender, RoutedEventArgs e) { Close(); }
    }

    /// <summary>
    /// Visibility from a bool or a string. Parameter "not" inverts; "empty" shows only when the text is empty;
    /// "number" is unrelated to visibility: it turns a zero-based index into "1", "2"...
    /// </summary>
    public sealed class Vis : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string p = parameter as string ?? "";
            if (p == "number") return (System.Convert.ToInt32(value) + 1).ToString();
            bool on = value is bool ? (bool)value : !string.IsNullOrEmpty(value as string);
            if (p == "not" || p == "empty") on = !on;
            return on ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
