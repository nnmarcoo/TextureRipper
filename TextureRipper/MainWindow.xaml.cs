using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
        private Point _dragMouseOrigin;

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

            if (result != true) return;
            if (!openFileDialog.FileName.Equals(""))
            {
                _filename = openFileDialog.FileName;
                InitializeCanvas();
            }

            //MessageBox.Show(_filename);
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
                
                if (Path.GetExtension(files[0]).Equals(".png") ||
                    Path.GetExtension(files[0]).Equals(".jpeg") ||
                    Path.GetExtension(files[0]).Equals(".jpg") ||
                    Path.GetExtension(files[0]).Equals(".tiff"))
                {
                    _filename = files[0];
                    InitializeCanvas();
                }
                
            }
            //MessageBox.Show(_filename);
        }

        private void InitializeCanvas()
        {
            FileImage.Visibility = Visibility.Collapsed; // hide file image icon
            FileText.Visibility = Visibility.Collapsed;  // hide file text
            
            var filePath = new Uri(_filename!); // make Uri for new file
            var newImage = new BitmapImage(filePath); // make bitmap for new image
            
            SourceImage.Source = newImage; // set the new bitmap to the source image

            Canvas.SetLeft(SourceImage, Window.ActualWidth/2 - newImage.Width/2); // center width
            Canvas.SetTop(SourceImage, Window.ActualHeight/2 - newImage.Height/2); // center height
        }

        private void PanSourceImageDown(object sender, MouseButtonEventArgs e) 
        {
            _dragMouseOrigin = e.GetPosition(Canvas);// get original position of mouse
        }

        private void PanImage(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(SourceImage, SourceImage, DragDropEffects.Move);
        }

        private void Canvas_OnDragOver(object sender, DragEventArgs e)
        {
            Point dropPosition = e.GetPosition(Canvas);
            
            Canvas.SetLeft(SourceImage, Canvas.GetLeft(SourceImage) + (dropPosition.X - _dragMouseOrigin.X));
            Canvas.SetTop(SourceImage, Canvas.GetTop(SourceImage) + (dropPosition.Y - _dragMouseOrigin.Y));
            _dragMouseOrigin = dropPosition;
        }

        private void ZoomImage(object sender, MouseWheelEventArgs e)
        {
            var zoom = e.Delta < 0 ? 0.7 : 1.3;

            double newWidth = SourceImage.ActualWidth * zoom;
            double newHeight = SourceImage.ActualHeight * zoom;

            double dWidth = newWidth - SourceImage.ActualWidth;
            double dHeight = newHeight - SourceImage.ActualHeight;

            double offsetX = e.GetPosition(SourceImage).X * dWidth / SourceImage.ActualWidth;
            double offsetY = e.GetPosition(SourceImage).Y * dHeight / SourceImage.ActualHeight;

            SourceImage.Width = newWidth;
            SourceImage.Height = newHeight;

            Canvas.SetLeft(SourceImage, Canvas.GetLeft(SourceImage) - offsetX);
            Canvas.SetTop(SourceImage, Canvas.GetTop(SourceImage) - offsetY);
        }




        private void CenterImage(FrameworkElement img)
        {
            Canvas.SetLeft(img, Canvas.ActualWidth/2 - img.Width/2);
            Canvas.SetTop(img, Canvas.ActualHeight/2 - img.Height/2);
        }
    }
}