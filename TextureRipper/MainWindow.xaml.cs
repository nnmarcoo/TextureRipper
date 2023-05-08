using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace TextureRipper //todo array of lines so don't have to instantiate every time
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string? _filename;
        private Point _dragMouseOrigin; // for panning
        private Point _lastMousePosition; // for dragging point
        private bool _isDraggingPoint;
        private Rectangle? _selectedPoint;
        private readonly SolidColorBrush _lineStroke = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); // refactored to avoid SOH
        private readonly DoubleCollection _strokeDashArray = new DoubleCollection() { 3, 1 }; // refactored to avoid SOH
        private ArrayList? lines; // array of lines
                                  // each time 2 points are added, make new line
                                  // only move them around on transform, not recreate

        public MainWindow()
        {
            InitializeComponent();
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
        
        //BUTTONS
        private void FileButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Create OpenFileDialog
            var openFileDialog = new OpenFileDialog
            {
                // Set filter for file extension and default file extension
                Filter = "Image Files (*.jpeg, *.jpg, *.png, *.tiff, *.ico, *.jpe, *.tif)|*.jpeg;*.jpg;*.png;*.tiff;*.ico;*.jpe;*.tif",
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
        
        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            if (_filename == null) return;
            _filename = null;
            CenterImage(SourceImage);
            SourceImage.Source = new BitmapImage();
            FileImage.Visibility = Visibility.Visible;
            FileText.Visibility = Visibility.Visible;
            DeleteAllPoints();
            DeleteAllLines();
            DisplayWarnings();
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            //Application.Current.Shutdown();
            Window.Close();
        }
        
        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            
        }
        

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void ImageDrop(object sender, DragEventArgs e)
        {
            if (_filename != null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); // why is this warning
                
            if (Path.GetExtension(files[0]).Equals(".png")  ||
                Path.GetExtension(files[0]).Equals(".jpeg") ||
                Path.GetExtension(files[0]).Equals(".jpg")  ||
                Path.GetExtension(files[0]).Equals(".tiff") ||
                Path.GetExtension(files[0]).Equals(".tif")  ||
                Path.GetExtension(files[0]).Equals(".ico")  ||
                Path.GetExtension(files[0]).Equals(".jpe")   )
            {
                _filename = files[0];
                InitializeCanvas();
            }
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

                    foreach (var point in Canvas.Children.OfType<Rectangle>())
                    {
                        Canvas.SetLeft(point, Canvas.GetLeft(point) + (Canvas.GetLeft(SourceImage) - prevX));
                        Canvas.SetTop(point, Canvas.GetTop(point) + (Canvas.GetTop(SourceImage) - prevY));
                    }
                }
            }
            DrawQuads();
            DisplayWarnings();
        }

        private void CanvasOnDragOver(object sender, DragEventArgs e)
        {
            Point dropPosition = e.GetPosition(Canvas);
            
            //translate image
            Canvas.SetLeft(SourceImage, Canvas.GetLeft(SourceImage) + (dropPosition.X - _dragMouseOrigin.X));
            Canvas.SetTop(SourceImage, Canvas.GetTop(SourceImage) + (dropPosition.Y - _dragMouseOrigin.Y));
            
            //translate points
            foreach (var point in Canvas.Children.OfType<Rectangle>())
            {
                Canvas.SetLeft(point, Canvas.GetLeft(point) + (dropPosition.X - _dragMouseOrigin.X));
                Canvas.SetTop(point, Canvas.GetTop(point) + (dropPosition.Y - _dragMouseOrigin.Y));
            }
            _dragMouseOrigin = dropPosition;

            if (Canvas.Children.OfType<Rectangle>().Count() < 20)
                DrawQuads();
            DisplayWarnings();
        }

        private void ZoomImage(object sender, MouseWheelEventArgs e)
        {
            if (_filename == null) return;
            if ((SourceImage.ActualWidth * (e.Delta < 0 ? 0.7 : 1.3)) > (4 * ActualWidth) || 
                (SourceImage.ActualWidth * (e.Delta < 0 ? 0.7 : 1.3)) < (0.1 * ActualWidth)) return;

            var zoom = e.Delta < 0 ? 0.7 : 1.3;

            foreach (FrameworkElement element in Canvas.Children) //Canvas.Children.OfType<Rectangle>()
            {
                ApplyZoom(element, zoom, e);
            }

            DrawQuads(); //required
            DisplayWarnings();
        }

        private void ApplyZoom(FrameworkElement element, double zoom, MouseWheelEventArgs e)
        {
            double newWidth = element.ActualWidth * zoom; // new width after zoom
            double newHeight = element.ActualHeight * zoom; // new height after zoom

            double dWidth = newWidth - element.ActualWidth; // difference between new width and old width
            double dHeight = newHeight - element.ActualHeight; // difference between new height and old height

            double offsetX = e.GetPosition(element).X * dWidth / element.ActualWidth;
            double offsetY = e.GetPosition(element).Y * dHeight / element.ActualHeight;

            if (element is not Rectangle)
            {
                element.Width = newWidth; // resize width
                element.Height = newHeight; // resize height
            }

            //translate element
            Canvas.SetLeft(element, Canvas.GetLeft(element) - offsetX);
            Canvas.SetTop(element, Canvas.GetTop(element) - offsetY);
        }
        
        private void CenterImage(FrameworkElement img)
        {
            Canvas.SetLeft(img, Canvas.ActualWidth/2 - img.ActualWidth/2);
            Canvas.SetTop(img, Canvas.ActualHeight/2 - img.ActualHeight/2);
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
                Rectangle point = new Rectangle();
                point.Width = 30;
                point.Height = 30;
                point.StrokeThickness = 1;
                point.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f04747"));
                point.Fill = new SolidColorBrush(Color.FromArgb(64, 114, 137, 218));

                // Set the position of the rectangle to the position of the mouse
                Canvas.SetLeft(point, e.GetPosition(Canvas).X);
                Canvas.SetTop(point, e.GetPosition(Canvas).Y);
                
                point.MouseEnter += PointMouseEnter;
                point.MouseLeave += PointMouseLeave;

                // Add the rectangle to the canvas
                Canvas.Children.Add(point);
                DrawQuads();
                DisplayWarnings();
            }
        }

        private void DragPoint(object sender, MouseButtonEventArgs e)
        {
            if (!_isDraggingPoint) return;
            DisplayWarnings();
            _isDraggingPoint = false;
            _selectedPoint?.ReleaseMouseCapture();
        }

        private void DrawQuad(Point p1, Point p2, Point p3, Point p4) // todo refractor with Quad object so that new lines don't need to be created
        {
            var points = Quad.OrderPointsClockwise(p1, p2, p3, p4);
            Line? side;

            foreach (var point in Canvas.Children.OfType<Rectangle>())
            {
                foreach (var line in Canvas.Children.OfType<Line>())
                {
                    //find line if it exists
                }
            }

            for (int i = 0; i < points.Length-1; i++) // draw 3 sides
            {
                side = new Line();

                side.Stroke = _lineStroke;
                side.X1 = points[i].X; 
                side.Y1 = points[i].Y;
                side.X2 = points[i+1].X;
                side.Y2 = points[i+1].Y;
                side.StrokeThickness = 2;
                side.StrokeDashArray = _strokeDashArray;
                Canvas.Children.Add(side);
            }

            side = new Line(); // connect quad
            side.Stroke = _lineStroke;
            side.X1 = points[3].X; 
            side.Y1 = points[3].Y;
            side.X2 = points[0].X;
            side.Y2 = points[0].Y;
            side.StrokeThickness = 2;
            side.StrokeDashArray = _strokeDashArray;
            Canvas.Children.Add(side);
        }

        private void DrawQuads() // todo: draw grid in quads if not mem issue
        {
            if (Canvas.Children.OfType<Rectangle>().Count() < 4) return; // if there are less than 4 points
            DeleteAllLines();
            
            var points = Canvas.Children.OfType<Rectangle>().ToList();

            for (int i = 0; i < points.Count; i += 4)
            {
                if (i + 3 >= points.Count) break; // Not enough points for a complete quad

                    DrawQuad(new Point(Canvas.GetLeft(points[i]), Canvas.GetTop(points[i])), //refactor
                    new Point(Canvas.GetLeft(points[i + 1]), Canvas.GetTop(points[i + 1])),
                    new Point(Canvas.GetLeft(points[i + 2]), Canvas.GetTop(points[i + 2])),
                    new Point(Canvas.GetLeft(points[i + 3]), Canvas.GetTop(points[i + 3])));
            }
        }
        
        private void PointMouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeAll;
        }

        private void PointMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }
        
        
        //WARNING
        private int CountMissingPoints()
        {
            return Canvas.Children.OfType<Rectangle>().Count(point => 
                Canvas.GetLeft(point) > Canvas.ActualWidth || 
                Canvas.GetLeft(point) < 0 || 
                Canvas.GetTop(point) > Canvas.ActualHeight || 
                Canvas.GetTop(point) < 0);
        }

        private string MissingPointsFormat()
        {
            var missingPoints = CountMissingPoints();
            if (missingPoints == 0) return "";
            if (missingPoints == 1) return "Warning: 1 point outside of work area\n";
            return "Warning: " + missingPoints + " points outside of work area\n";
        }

        private bool HasCollinearQuad()
        {
            if (Canvas.Children.OfType<Rectangle>().Count() < 4) return false;

            var points = Canvas.Children.OfType<Rectangle>().ToList();

            for (int i = 0; i < points.Count; i += 4)
            {
                if (i + 3 >= points.Count) // Not enough points for a complete quad
                    break;

                if ((Canvas.GetLeft(points[i]).Equals(Canvas.GetLeft(points[i+1])) &&
                    Canvas.GetLeft(points[i]).Equals(Canvas.GetLeft(points[i+2])) &&
                    Canvas.GetLeft(points[i]).Equals(Canvas.GetLeft(points[i+3]))) ||
                    Canvas.GetTop(points[i]).Equals(Canvas.GetTop(points[i+1])) &&
                    Canvas.GetTop(points[i]).Equals(Canvas.GetTop(points[i+2])) &&
                    Canvas.GetTop(points[i]).Equals(Canvas.GetTop(points[i+3])))
                    return true;
            }
            return false;
        }

        private int InvalidNumPoints()
        {
            return Canvas.Children.OfType<Rectangle>().Count() % 4;
        }

        private string InvalidNumPointsFormat()
        {
            var invalidPoints = InvalidNumPoints();
            if (invalidPoints == 0) return "";
            if (invalidPoints == 1) return "Warning: 1 disjointed point\n";
            return "Warning: " + invalidPoints + " disjointed points\n";
        }
        
        private string CollinearQuadFormat()
        {
            return HasCollinearQuad() ? "Warning: Collinear quad\n" : "";
        }

        private void DisplayWarnings()
        {
            var warning = "";

            if (Canvas.Children.OfType<Rectangle>().Count() > 19)
                warning += "Performance mode on\n";

            warning += MissingPointsFormat();
            warning += CollinearQuadFormat();
            warning += InvalidNumPointsFormat();

            Warning.Text = warning;
        }


        //MODIFY
        private void DeleteAllPoints()
        {
            List<UIElement> pointsToRemove = new List<UIElement>();
            
            foreach (var point in Canvas.Children.OfType<Rectangle>()) // convert to linq? etc.
            {
                pointsToRemove.Add(point);
            }
            foreach (var point in pointsToRemove) // refactored to avoid SOH
            {
                Canvas.Children.Remove(point);
            }
        }
        
        private void DeleteAllLines()
        {
            List<UIElement> linesToRemove = new List<UIElement>();
            
            foreach (var line in Canvas.Children.OfType<Line>())
            {
                linesToRemove.Add(line);
            }
            foreach (var line in linesToRemove) // refactored to avoid SOH
            {
                Canvas.Children.Remove(line);
            }
        }
        
        
        //WINDOW CONTROL
        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control) // undo
            {
                var points = Canvas.Children.OfType<Rectangle>().ToList();
                if (points.Count > 0)
                {
                    DeleteAllLines();
                    Canvas.Children.Remove(points.Last());
                    DrawQuads();
                }
            }
            if (e.Key == Key.C) Info.Visibility = Info.IsVisible ? Visibility.Collapsed : Visibility.Visible;
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

        private void InfoButtonClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/nnmarcoo",
                UseShellExecute = true
            });
        }
    }
}