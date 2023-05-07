using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using ColorConverter = System.Windows.Media.ColorConverter;
using Path = System.IO.Path;
using Point = System.Windows.Point;

namespace TextureRipper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string? _filename;
        private Point _dragMouseOrigin; // for panning
        private bool _isDraggingPoint;
        private Rectangle? _selectedPoint;
        private Point _lastMousePosition; // for dragging point

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
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            //Application.Current.Shutdown();
            Window.Close();
        }

        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            if (_filename == null) return;
            _filename = null;
            CenterImage(SourceImage);
            SourceImage.Source = new BitmapImage();
            FileImage.Visibility = Visibility.Visible;
            FileText.Visibility = Visibility.Visible;
            DeleteAllPoints();
        }

        private void GridButtonClick(object sender, RoutedEventArgs e)
        {
            //if (_filename == null) return;
            
            
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

        private void CanvasMouseRightButtonDown(object sender, MouseButtonEventArgs e) 
        {
            _dragMouseOrigin = e.GetPosition(Canvas);// get original position of mouse
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingPoint && _selectedPoint != null) // if dragging point
            {
                Point currentPosition = e.GetPosition(Canvas);
                double deltaX = currentPosition.X - _lastMousePosition.X;
                double deltaY = currentPosition.Y - _lastMousePosition.Y;

                // Move the rectangle by the delta
                Canvas.SetLeft(_selectedPoint, Canvas.GetLeft(_selectedPoint) + deltaX);
                Canvas.SetTop(_selectedPoint, Canvas.GetTop(_selectedPoint) + deltaY);

                _lastMousePosition = currentPosition;
            }
            else // if panning image
            {
                const int margin = 100;

                if (e.RightButton == MouseButtonState.Pressed)
                {
                    DragDrop.DoDragDrop(SourceImage, SourceImage, DragDropEffects.Move);
                }

                if (Canvas.GetLeft(SourceImage) > Canvas.ActualWidth - margin ||
                    SourceImage.ActualWidth + Canvas.GetLeft(SourceImage) < margin ||
                    Canvas.GetTop(SourceImage) > Canvas.ActualHeight - margin ||
                    SourceImage.ActualHeight + Canvas.GetTop(SourceImage) < margin)
                {
                    var prevX = Canvas.GetLeft(SourceImage);
                    var prevY = Canvas.GetTop(SourceImage);
                    
                    CenterImage(SourceImage);

                    foreach (UIElement point in Canvas.Children)
                    {
                        if (point is Rectangle)
                        {
                            Canvas.SetLeft(point, Canvas.GetLeft(point) + (Canvas.GetLeft(SourceImage) - prevX));
                            Canvas.SetTop(point, Canvas.GetTop(point) + (Canvas.GetTop(SourceImage) - prevY));
                        }
                    }
                }
            }
        }

        private void CanvasOnDragOver(object sender, DragEventArgs e)
        {
            Point dropPosition = e.GetPosition(Canvas);
            
            //translate image
            Canvas.SetLeft(SourceImage, Canvas.GetLeft(SourceImage) + (dropPosition.X - _dragMouseOrigin.X));
            Canvas.SetTop(SourceImage, Canvas.GetTop(SourceImage) + (dropPosition.Y - _dragMouseOrigin.Y));
            
            //translate points
            foreach (var item in Canvas.Children)
            {
                if (item is Rectangle point)
                {
                    Canvas.SetLeft(point, Canvas.GetLeft(point) + (dropPosition.X - _dragMouseOrigin.X));
                    Canvas.SetTop(point, Canvas.GetTop(point) + (dropPosition.Y - _dragMouseOrigin.Y));
                }
            }
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

            //translate image
            Canvas.SetLeft(SourceImage, Canvas.GetLeft(SourceImage) - offsetX);
            Canvas.SetTop(SourceImage, Canvas.GetTop(SourceImage) - offsetY);

            foreach (UIElement point in Canvas.Children) // points don't scale with zoom properly
            {
                if (point is Rectangle)
                {
                    
                }
            }
        }
        
        private void CenterImage(FrameworkElement img)
        {
            Canvas.SetLeft(img, Canvas.ActualWidth/2 - img.ActualWidth/2);
            Canvas.SetTop(img, Canvas.ActualHeight/2 - img.ActualHeight/2);
        }
        
        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Create the animation for opacity
            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(.2));
            opacityAnimation.Completed += (_, _) => Application.Current.Shutdown();

            // Trigger the animation
            e.Cancel = true;
            this.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }

        private void ManipulatePoint(object sender, MouseButtonEventArgs e)
        {
            if (_filename == null) return;
            
            if (e.OriginalSource is Rectangle) // if clicking on a rectangle
            {
                _selectedPoint = e.OriginalSource as Rectangle;
                _lastMousePosition = e.GetPosition(Canvas);
                _isDraggingPoint = true;
                _selectedPoint?.CaptureMouse();
            }
            else // Create a new rectangle element
            {
                Rectangle rect = new Rectangle();
                rect.Width = 20;
                rect.Height = 20;
                rect.StrokeThickness = 1;
                rect.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f04747"));
                rect.Fill = new SolidColorBrush(Color.FromArgb(64, 114, 137, 218));

                // Set the position of the rectangle to the position of the mouse
                Canvas.SetLeft(rect, e.GetPosition(Canvas).X - rect.Width / 2);
                Canvas.SetTop(rect, e.GetPosition(Canvas).Y - rect.Height / 2);

                // Add the rectangle to the canvas
                Canvas.Children.Add(rect);
            }
        }
        
        private void DeleteAllPoints()
        {
            // Loop through all children of the canvas
            for (var i = Canvas.Children.Count - 1; i >= 0; i--)
            {
                // Check if the child is a rectangle
                if (Canvas.Children[i] is Rectangle)
                {
                    // Remove the rectangle from the canvas
                    Canvas.Children.RemoveAt(i);
                }
            }
        }

        private void DragPoint(object sender, MouseButtonEventArgs e)
        {
            if (!_isDraggingPoint) return;
            _isDraggingPoint = false;
            _selectedPoint?.ReleaseMouseCapture();
        }
    }
}