using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace TextureRipper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string? _filename;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Create OpenFileDialog
            var openFileDialog = new OpenFileDialog
            {
                // Set filter for file extension and default file extension
                Filter = "Image Files (*.jpeg, *.jpg, *.png, *.tiff)|*.jpeg;*.jpg;*.png;*.tiff",
                DefaultExt = ".png"
            };

            var result = openFileDialog.ShowDialog();
            
            if (result == true)
                _filename = openFileDialog.FileName;
            
            MessageBox.Show(_filename);
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LineButtonClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void GridButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            
        }
        private void ImageDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (Path.GetExtension(files[0]).Equals(".png")  ||
                    Path.GetExtension(files[0]).Equals(".jpeg") ||
                    Path.GetExtension(files[0]).Equals(".jpg")  ||
                    Path.GetExtension(files[0]).Equals(".tiff"  ))
                    _filename = files[0];
            }
            MessageBox.Show(_filename);
        }
    }
}