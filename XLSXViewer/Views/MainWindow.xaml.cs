using System.Windows;

namespace XLSXViewer.Views
{
    public partial class MainWindow : Window
    {
        private bool willRefresh;
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new ViewModels.AppViewModel();
        }

        private void DescriptionButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Description.Text, "Описание угрозы", MessageBoxButton.OK);
        }
        
        private void DescriptionOldButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(DescriptionOld.Text, "Описание угрозы", MessageBoxButton.OK);
        }

        private void LoaderButton_Click(object sender, RoutedEventArgs e)
        {
            if (Tools.FileFinder.FileSource == null || ViewModels.LoaderViewModel.isLoaded || willRefresh)
            {
                while ((bool)new LoaderWindow().ShowDialog()) { }
            }
            else
            {
                var answer = MessageBox.Show("Файл найден. Открыть?", "Файл найден", MessageBoxButton.YesNoCancel);
                if (answer == MessageBoxResult.Yes)
                {
                    willRefresh = true;
                    ViewModels.LoaderViewModel.Source = Tools.FileFinder.FileSource;
                    ViewModels.LoaderViewModel.isLoaded = true;
                }
                else if (answer == MessageBoxResult.No)
                {
                    while ((bool)new LoaderWindow().ShowDialog()) { }
                }
            }

            if (ViewModels.LoaderViewModel.isLoaded == true)
            {
                DataContext = new ViewModels.AppViewModel(true);
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            Old.Visibility = Visibility.Collapsed;
            New.Visibility = Visibility.Visible;
        }
        
        private void OldButton_Click(object sender, RoutedEventArgs e)
        {
            New.Visibility = Visibility.Collapsed;
            Old.Visibility = Visibility.Visible;
        }
    }
}
