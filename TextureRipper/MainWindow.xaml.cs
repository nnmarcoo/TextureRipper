using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        //private int points;

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

        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            if (_filename == null) return;
            _filename = null;
            CenterImage(SourceImage);
            SourceImage.Source = new BitmapImage();
            FileImage.Visibility = Visibility.Visible;
            FileText.Visibility = Visibility.Visible;
        }

        private void GridButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            
        }
        private void ImageDrop(object sender, DragEventArgs e)
        {
            if (_filename != null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                
            if (Path.GetExtension(files[0]).Equals(".png")  ||
                Path.GetExtension(files[0]).Equals(".jpeg") ||
                Path.GetExtension(files[0]).Equals(".jpg")  ||
                Path.GetExtension(files[0]).Equals(".tiff")  )
            {
                _filename = files[0];
                InitializeCanvas();
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
            SourceImage.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
        }

        private void PanSourceImageDown(object sender, MouseButtonEventArgs e) 
        {
            _dragMouseOrigin = e.GetPosition(Canvas);// get original position of mouse
        }

        private void PanImage(object sender, MouseEventArgs e)
        {
            const int margin = 100;

            if (e.RightButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(SourceImage, SourceImage, DragDropEffects.Move);

            if (Canvas.GetLeft(SourceImage) > Canvas.ActualWidth-margin        ||
                SourceImage.ActualWidth + Canvas.GetLeft(SourceImage) < margin ||
                Canvas.GetTop(SourceImage) > Canvas.ActualHeight-margin        ||
                SourceImage.ActualHeight + Canvas.GetTop(SourceImage) < margin  )
                
                CenterImage(SourceImage);
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
            if (_filename == null) return;
            if ((SourceImage.ActualWidth * (e.Delta < 0 ? 0.7 : 1.3)) > (4 * ActualWidth) || 
                (SourceImage.ActualWidth * (e.Delta < 0 ? 0.7 : 1.3)) < (0.25 * ActualWidth)) return;
            
            var zoom = e.Delta < 0 ? 0.7 : 1.3;

            double newWidth = SourceImage.ActualWidth * zoom; // new width after zoom
            double newHeight = SourceImage.ActualHeight * zoom; // new height after zoom

            double dWidth = newWidth - SourceImage.ActualWidth; // difference between new width and old width
            double dHeight = newHeight - SourceImage.ActualHeight; // difference between new height and old height

            double offsetX = e.GetPosition(SourceImage).X * dWidth / SourceImage.ActualWidth;
            double offsetY = e.GetPosition(SourceImage).Y * dHeight / SourceImage.ActualHeight;

            SourceImage.Width = newWidth; // resize width
            SourceImage.Height = newHeight; // resize height

            Canvas.SetLeft(SourceImage, Canvas.GetLeft(SourceImage) - offsetX);
            Canvas.SetTop(SourceImage, Canvas.GetTop(SourceImage) - offsetY);
        }
        
        private void CenterImage(FrameworkElement img) // why no work in InitCanvas?
        {
            Canvas.SetLeft(img, Canvas.ActualWidth/2 - img.ActualWidth/2);
            Canvas.SetTop(img, Canvas.ActualHeight/2 - img.ActualHeight/2);
        }
    }
}