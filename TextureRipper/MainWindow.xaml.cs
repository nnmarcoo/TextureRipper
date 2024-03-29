﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Timer = System.Timers.Timer;

namespace TextureRipper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private BitmapImage? _file;
        private readonly Dictionary<int, Bitmap> _data = new();
        
        private Point _dragMouseOrigin; // for panning
        private Point _dragMouseOriginPreview; // for preview image
        private bool _isDraggingPreview; // for preview image
        private Point _lastMousePosition; // for dragging point
        private bool _isDraggingPoint;
        private bool _isZooming;
        private bool _isPanning;
        private bool _isAddingPoint;
        private bool _changed; // a quad has been changed
        private Rectangle? _selectedPoint;
        private int _previewCycle = 1;
        private bool _livePointOrderUpdate;
        
        private readonly SolidColorBrush _lineStroke = new(Color.FromArgb(255, 255, 0, 0));
        
        private readonly DoubleCollection _strokeDashArray = new() { 3, 1 };

        private readonly SolidColorBrush _pointStroke = new((Color)ColorConverter.ConvertFromString("#f04747"));
        private readonly SolidColorBrush _pointFill = new(Color.FromArgb(64, 114, 137, 218));
        private readonly SolidColorBrush _selectedPointStroke = new(Color.FromArgb(255, 0, 141, 213));

        private Timer? _timer;
        private CancellationTokenSource? _tokenSource;
        
        private const int PxlShift = 5;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeCanvas(string filename)
        {
            FileImage.Visibility = Visibility.Collapsed; // hide file image icon
            FileText.Visibility = Visibility.Collapsed;  // hide file text
            
            _file = new BitmapImage(new Uri(filename)); // make Uri for new file
            
            SourceImage.Source = _file; // set the new bitmap to the source image

            SourceImage.UpdateLayout();

            CenterImage(SourceImage);

            SourceImage.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
            PreviewImage.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
            
            StartTimer();
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
                InitializeCanvas(openFileDialog.FileName);
        }
        
        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            if (_file == null) return;
            _file.UriSource = null; // is this necessary
            _file = null;
            SourceImage.Source = null;
            
            FileImage.Visibility = Visibility.Visible;
            FileText.Visibility = Visibility.Visible;
            PreviewImage.Source = null;

            _data.Clear();
            DeleteAllPoints();
            DeleteAllLines();
            DisplayWarnings();
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            //Application.Current.Shutdown();
            StopTimer();
            _tokenSource?.Dispose(); // should this be somewhere else
            Window.Close();
        }
        
        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (_data.Count != Canvas.Children.OfType<Rectangle>().Count()/4) return;
            int i = 0;
            SaveFileDialog dialog = new SaveFileDialog
            { Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                Title = "Save Image",
                FileName = "image" // Default file name without extension
            };
            var result = dialog.ShowDialog();
            if (result != true || _data.Count == 0) return;

            string extension = Path.GetExtension(dialog.FileName);
            string fileName = $"{dialog.FileName}_{i}{extension}";
            
            BuildBitmap mosaic = new BuildBitmap(new List<Bitmap>(_data.Values));
            
            mosaic.OutBitmap.Save(fileName); // todo crashes sometimes based on names
            
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void ImageDrop(object sender, DragEventArgs e)
        {
            if (_file != null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop); // why is this warning
                
            if (files != null && 
               (Path.GetExtension(files[0]).Equals(".png")  ||
                Path.GetExtension(files[0]).Equals(".jpeg") ||
                Path.GetExtension(files[0]).Equals(".jpg")  ||
                Path.GetExtension(files[0]).Equals(".tiff") ||
                Path.GetExtension(files[0]).Equals(".tif")  ||
                Path.GetExtension(files[0]).Equals(".ico")  ||
                Path.GetExtension(files[0]).Equals(".jpe"))  )
                InitializeCanvas(files[0]);
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
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            _isPanning = true;
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
            
                DrawQuads();
            DisplayWarnings();
        }
        
        private void ZoomPreviewImage(object sender, MouseWheelEventArgs e)
        {
            if (_file == null) return;
            var zoom = e.Delta < 0 ? 0.7 : 1.3;
            const double tolerance = 0.01;
            if (PreviewImage.ActualHeight * zoom > Canvas.ActualHeight && Math.Abs(zoom - 1.3) < tolerance)
                return;
            if (PreviewImage.ActualHeight * zoom < Canvas.ActualHeight / 5 && Math.Abs(zoom - 0.7) < tolerance)
                return;


            double newWidth = PreviewImage.ActualWidth * zoom;

            PreviewImage.Width = newWidth;
        }

        private void ZoomImage(object sender, MouseWheelEventArgs e)
        {
            if (_file == null) return;
            if ((SourceImage.ActualWidth * (e.Delta < 0 ? 0.7 : 1.3)) > (4 * ActualWidth) || 
                (SourceImage.ActualWidth * (e.Delta < 0 ? 0.7 : 1.3)) < (0.1 * ActualWidth)) return;
            _isZooming = true;

            var zoom = e.Delta < 0 ? 0.9 : 1.1;

            foreach (FrameworkElement element in Canvas.Children) //Canvas.Children.OfType<Rectangle>()
            {
                //if (element is not Line) // line transformations are handing by DrawQuads() because they are parented to points
                    ApplyZoom(element, zoom, e);
            }
            DrawQuads(); //required
            DisplayWarnings();
        }

        private static void ApplyZoom(FrameworkElement element, double zoom, MouseWheelEventArgs e)
        {
            double newWidth = element.ActualWidth * zoom; // new width after zoom
            double newHeight = element.ActualHeight * zoom; // new height after zoom

            double dWidth = newWidth - element.ActualWidth; // difference between new width and old width
            double dHeight = newHeight - element.ActualHeight; // difference between new height and old height

            double offsetX = e.GetPosition(element).X * dWidth / element.ActualWidth;
            double offsetY = e.GetPosition(element).Y * dHeight / element.ActualHeight;

            if (element is not Rectangle && element is not Line)
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
            if (_file == null) return;

            if (e.OriginalSource is Rectangle source) // if clicking on a rectangle (dragging)
            {
                if (_selectedPoint != null) _selectedPoint.Stroke = _pointStroke; // reset color
                
                _selectedPoint = source;
                _lastMousePosition = e.GetPosition(Canvas);
                _isDraggingPoint = true;
                _selectedPoint?.CaptureMouse();

                if (_selectedPoint != null) _selectedPoint.Stroke = _selectedPointStroke; // set selected point
            }
            else
            {
                _isAddingPoint = true;
                Line line = new Line // Create a new line element
                {
                    Stroke = _lineStroke,
                    StrokeThickness = 1.5,
                    StrokeDashArray = _strokeDashArray
                };
                Canvas.Children.Add(line);
                
                Rectangle point = new Rectangle // Create a new rectangle element
                {
                    Tag = Canvas.Children.OfType<Rectangle>().Count()+1,
                    Width = 30,
                    Height = 30,
                    StrokeThickness = 1,
                    Stroke = _pointStroke,
                    Fill = _pointFill
                };

                // Set the position of the rectangle to the position of the mouse
                Canvas.SetLeft(point, e.GetPosition(Canvas).X);
                Canvas.SetTop(point, e.GetPosition(Canvas).Y);
                
                point.MouseEnter += PointMouseEnter;
                point.MouseLeave += PointMouseLeave;

                // Add the rectangle to the canvas
                Canvas.Children.Add(point);

                if (_livePointOrderUpdate)
                {
                    var points = Canvas.Children.OfType<Rectangle>().Count(); // # of rectangles
                    var list = Canvas.Children.OfType<Rectangle>().ToList(); // list of the rectangles
                    if (points % 4 == 0)
                    {
                        var quad = Quad.OrderPointsClockwise(new UIElement[]
                        {
                            list[points - 1], list[points - 2], list[points - 3], list[points - 4]
                        });

                        list[points - 4] = (Rectangle)quad[0];
                        list[points - 3] = (Rectangle)quad[1];
                        list[points - 2] = (Rectangle)quad[2];
                        list[points - 1] = (Rectangle)quad[3];
                    }
                }

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

        private void DrawQuads()
        {
            var linesToRemove = Canvas.Children.OfType<Line>().ToList(); // hide extra lines
            if (Canvas.Children.OfType<Rectangle>().Count() % 4 != 0)
                for (var i = Canvas.Children.OfType<Rectangle>().Count() -
                             Canvas.Children.OfType<Rectangle>().Count() % 4;
                     i < Canvas.Children.OfType<Rectangle>().Count();
                     i++)
                {
                    linesToRemove[i].X1 = 0;
                    linesToRemove[i].Y1 = 0;
                    linesToRemove[i].X2 = 0;
                    linesToRemove[i].Y2 = 0;
                }
            
            if (Canvas.Children.OfType<Rectangle>().Count() < 4) return;

            var points = Canvas.Children.OfType<Rectangle>().ToList();
            var lines = Canvas.Children.OfType<Line>().ToList();

            for (var i = 0; i < points.Count; i += 4)
            {
                if (i + 3 >= points.Count) break;

                Point[] quad;
                if (_livePointOrderUpdate)
                {
                    quad = new Point[]
                    {
                        new(Canvas.GetLeft(points[i]), Canvas.GetTop(points[i])),
                        new(Canvas.GetLeft(points[i + 1]), Canvas.GetTop(points[i + 1])),
                        new(Canvas.GetLeft(points[i + 2]), Canvas.GetTop(points[i + 2])),
                        new(Canvas.GetLeft(points[i + 3]), Canvas.GetTop(points[i + 3]))
                    };
                }
                else
                {
                    quad = Quad.OrderPointsClockwise( new Point[] { // this solution is wasteful
                        new (Canvas.GetLeft(points[i]), Canvas.GetTop(points[i])),
                        new (Canvas.GetLeft(points[i + 1]), Canvas.GetTop(points[i + 1])),
                        new (Canvas.GetLeft(points[i + 2]), Canvas.GetTop(points[i + 2])),
                        new (Canvas.GetLeft(points[i + 3]), Canvas.GetTop(points[i + 3]))
                    });
                }

                for (var j = 0; j < 4; j++) // draw quad
                {
                    var lineIndex = i + j;

                    lines[lineIndex].X1 = quad[j].X;
                    lines[lineIndex].Y1 = quad[j].Y;
                    lines[lineIndex].X2 = quad[(j + 1) % 4].X;
                    lines[lineIndex].Y2 = quad[(j + 1) % 4].Y;
                }
                _changed = _isDraggingPoint || _isAddingPoint && Canvas.Children.OfType<Rectangle>().Count() % 4 == 0;
            }
        }

        private async void CalculateBitmaps(bool skip = false) // is this skip param necessary?
        {
            if ((!_isDraggingPoint && !_isZooming && !_isPanning && !_isAddingPoint && _changed) || skip)
            {
                _tokenSource?.Cancel(); // if it's running, cancel it before running again
                _tokenSource = new CancellationTokenSource();
                var token = _tokenSource.Token;
                _changed = false;

                var points = Canvas.Children.OfType<Rectangle>().ToList();

                var selectedQuad = _selectedPoint != null
                    ? (int)Math.Ceiling((double)(int)_selectedPoint.Tag / 4)
                    : (int)Math.Ceiling((double)points.Count / 4);
                _previewCycle = selectedQuad;

                for (var i = 0; i < points.Count; i += 4)
                {
                    if (i + 3 >= points.Count) break;
                    if ((i == 0 ? 1 : (i+4)/4) != selectedQuad) continue; // iterationQuad != selectedQuad
                    
                    Point[] quad;
                    if (_livePointOrderUpdate)
                    {
                        quad = new Point[]
                        {
                            new(Canvas.GetLeft(points[i]), Canvas.GetTop(points[i])),
                            new(Canvas.GetLeft(points[i + 1]), Canvas.GetTop(points[i + 1])),
                            new(Canvas.GetLeft(points[i + 2]), Canvas.GetTop(points[i + 2])),
                            new(Canvas.GetLeft(points[i + 3]), Canvas.GetTop(points[i + 3]))
                        };
                    }
                    else
                    {
                        quad = Quad.OrderPointsClockwise( new Point[] { // this solution is wasteful
                            new (Canvas.GetLeft(points[i]), Canvas.GetTop(points[i])),
                            new (Canvas.GetLeft(points[i + 1]), Canvas.GetTop(points[i + 1])),
                            new (Canvas.GetLeft(points[i + 2]), Canvas.GetTop(points[i + 2])),
                            new (Canvas.GetLeft(points[i + 3]), Canvas.GetTop(points[i + 3]))
                        });
                    }

                    UpdateEverything();
                    Point[] remappedPoints =
                        Quad.RemapCoords(new Point(Canvas.GetLeft(SourceImage), Canvas.GetTop(SourceImage)), quad,
                            SourceImage.ActualWidth, SourceImage.ActualHeight, _file!.PixelWidth,
                            _file.PixelHeight);
                    try
                    {
                        var progress = new Progress<int>(value => { ProgressBar.Value = value; });
                        await Task.Run(
                            () => _data[selectedQuad] = Quad.WarpImage(_file, Quad.CalcH(remappedPoints),
                                remappedPoints, token, progress), token);
                    }
                    catch (Exception) // should be OperationCanceledException but that doesn't work
                    {
                        //ignored
                    }
                    finally
                    {
                        ProgressBar.Value = 0;
                    }
                    UpdatePreview();
                    break;
                }
            }
            _isPanning = false;
            _isZooming = false;
            _isAddingPoint = false;
        }

        private void UpdatePreview()
        {
            if (_data.Values.FirstOrDefault() == null) return;

            var prevWidth = PreviewImage.Source != null ? PreviewImage.ActualWidth : Canvas.ActualWidth / 5;

            PreviewImage!.Source = BitmapToBitmapSource(_data[_previewCycle]);
            PreviewImage.Width = prevWidth;
            GC.Collect(); // is this even doing anything
        }

        private static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        private void PointMouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeAll;
            
        }

        private void PointMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void UpdateEverything() // is this necessary?
        {
            foreach (FrameworkElement element in Canvas.Children)
            {
                element.UpdateLayout();
            }
        }
        
        private void MouseMovePreviewImage(object sender, MouseEventArgs e)
        {
            return; // unused
            var draggedEdge = DraggingEdgeCondition(sender, e);
            
            if (_isDraggingPreview)
            {
                Point currentPoint = e.GetPosition(PreviewImage);
                double deltaX = currentPoint.X - _dragMouseOriginPreview.X;
                double deltaY = currentPoint.Y - _dragMouseOriginPreview.Y;
                //PreviewImage.Stretch = Stretch.None;
                
                if (draggedEdge == 2)
                {
                    Canvas.SetLeft(PreviewImage, Canvas.GetLeft(PreviewImage) + deltaX);
                    PreviewImage.Width -= deltaX;
                }
                else if (draggedEdge == 1)
                {
                    Canvas.SetTop(PreviewImage, Canvas.GetTop(PreviewImage) + deltaY);
                    PreviewImage.Height -= deltaY;
                }
            }

            if (draggedEdge == 2)
                Window.Cursor = Cursors.SizeWE;
            else if (draggedEdge == 1)
                Window.Cursor = Cursors.SizeNS;
            else
                Window.Cursor = null;
        }
        
        private void MouseEnterPreviewImage(object sender, MouseEventArgs e)
        {
            PreviewImage.Opacity = .6;
        }
        
        private void MouseLeavePreviewImage(object sender, MouseEventArgs e)
        {
            PreviewImage.Opacity = 1;
        }
        
        private void LeftButtonDownPreviewImage(object sender, MouseButtonEventArgs e)
        {
            return; // unused
            var draggedEdge = DraggingEdgeCondition(sender, e);
            
            if (draggedEdge == 0) return;
            
            _isDraggingPreview = true;
            PreviewImage.CaptureMouse();
            _dragMouseOriginPreview = e.GetPosition(PreviewImage);
        }

        private void LeftButtonUpPreviewImage(object sender, MouseButtonEventArgs e)
        {
            return; // unused
            if (!_isDraggingPreview) return;
            
            PreviewImage.Stretch = Stretch.Uniform;
            _isDraggingPreview = false;
            PreviewImage.ReleaseMouseCapture();
        }

        private int DraggingEdgeCondition(object sender, MouseEventArgs e)
        {
            return -1; // unused
            var thresholdX = PreviewImage.ActualWidth * .07; // can be simplified?
            var thresholdY = PreviewImage.ActualHeight * .07;
            Point mousePosition = e.GetPosition((UIElement)sender);
            
            if (mousePosition.X < thresholdX && mousePosition.X < mousePosition.Y)
                return 2;
            return mousePosition.Y < thresholdY ? 1 : 0;
        }


        //WARNINGS
        private int CountMissingPoints()
        {
            return Canvas.Children.OfType<Rectangle>().Count(point => 
                Canvas.GetLeft(point) > Canvas.ActualWidth || 
                Canvas.GetLeft(point) < 0 || 
                Canvas.GetTop(point) > Canvas.ActualHeight || 
                Canvas.GetTop(point) < 0); // int
        }

        private string MissingPointsFormat()
        {
            var missingPoints = CountMissingPoints();
            if (missingPoints == 0) return "";
            if (missingPoints == 1) return "Warning: 1 point outside of work area\n";
            return "Warning: " + missingPoints + " points outside of work area\n";
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

        private void DisplayWarnings()
        {
            var warning = "";

                warning += MissingPointsFormat();
                warning += InvalidNumPointsFormat();

                Warning.Text = warning;
        }


        //MODIFY
        private void DeleteAllPoints()
        {
            var pointsToRemove = Canvas.Children.OfType<Rectangle>().Cast<UIElement>().ToList();

            foreach (var point in pointsToRemove) // refactored to avoid SOH
            {
                Canvas.Children.Remove(point);
            }
        }
        
        private void DeleteAllLines()
        {
            var linesToRemove = Canvas.Children.OfType<Line>().Cast<UIElement>().ToList();

            foreach (var line in linesToRemove) // refactored to avoid SOH
            {
                Canvas.Children.Remove(line);
            }
        }
        
        
        //WINDOW CONTROL
        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control) // ctrl + z // undo
            {
                var points = Canvas.Children.OfType<Rectangle>().ToList();
                var lines = Canvas.Children.OfType<Line>().ToList();
                if (points.Count > 0)
                {
                    Canvas.Children.Remove(points.Last());
                    Canvas.Children.Remove(lines.Last());
                    _data.Remove((int)Math.Ceiling(points.Count / 4.0));
                    DrawQuads();
                }
            }
            else if (e.Key is Key.C) Info.Visibility = Info.IsVisible ? Visibility.Collapsed : Visibility.Visible; // show key binds
            
            else if (e.Key is Key.Q or Key.E) // cycle preview images
            {
                var increment = (e.Key == Key.E) ? 1 : -1;
                _previewCycle = (_previewCycle - 1 + increment + _data.Count) % _data.Count + 1;
                UpdatePreview();
            }
            
            else if (e.Key is Key.Tab) CycleSelectedPoint(); // cycle selected point
            
            else if (e.Key is Key.R) // rotate preview image
            {
                try
                {
                    _data[_previewCycle].RotateFlip(RotateFlipType.Rotate90FlipNone);
                    UpdatePreview();
                }
                catch
                {
                    // ignored
                }
            }
            
            else if (e.Key is Key.P) // toggle live point order update
            {
                _livePointOrderUpdate = !_livePointOrderUpdate;
            }

            if (_selectedPoint != null) // pixel shift
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    var quad = GetQuad((int)_selectedPoint.Tag);
                    switch (e.Key)
                    {
                        case Key.W or Key.Up:
                            ShiftQuad(quad, 0, PxlShift);
                            CalculateBitmaps(true);
                            DrawQuads();
                            break;
                        case Key.A or Key.Left:
                            ShiftQuad(quad, PxlShift, 0);
                            CalculateBitmaps(true);
                            DrawQuads();
                            break;
                        case Key.S or Key.Down:
                            ShiftQuad(quad, 0, -PxlShift);
                            CalculateBitmaps(true);
                            DrawQuads();
                            break;
                        case Key.D or Key.Right:
                            ShiftQuad(quad, -PxlShift, 0);
                            CalculateBitmaps(true);
                            DrawQuads();
                            break;
                    }
                }
                else
                {
                    switch (e.Key)
                    {
                        case Key.W or Key.Up:
                            Canvas.SetTop(_selectedPoint, Canvas.GetTop(_selectedPoint) - PxlShift);
                            CalculateBitmaps(true);
                            DrawQuads();
                            break;
                        case Key.A or Key.Left:
                            Canvas.SetLeft(_selectedPoint, Canvas.GetLeft(_selectedPoint) - PxlShift);
                            CalculateBitmaps(true);
                            DrawQuads();
                            break;
                        case Key.S or Key.Down:
                            Canvas.SetTop(_selectedPoint, Canvas.GetTop(_selectedPoint) + PxlShift);
                            CalculateBitmaps(true);
                            DrawQuads();
                            break;
                        case Key.D or Key.Right:
                            Canvas.SetLeft(_selectedPoint, Canvas.GetLeft(_selectedPoint) + PxlShift);
                            CalculateBitmaps(true);
                            DrawQuads();
                            break;
                    }
                }
            }
            DisplayWarnings();
        }

        private void CycleSelectedPoint()
        {
            if (_selectedPoint == null || Canvas.Children.OfType<Rectangle>().Count() < 4) return;
            
            var points = Canvas.Children.OfType<Rectangle>().ToList();
            
            _selectedPoint.Stroke = _pointStroke; // reset color
            
            _selectedPoint = (int)_selectedPoint.Tag % 4 == 0 ? points[(int)_selectedPoint.Tag - 4] : points[(int)_selectedPoint.Tag];

            _selectedPoint.Stroke = _selectedPointStroke;
        }

        private int GetQuad(int point)
        {
            for (var i = 0; i < Canvas.Children.OfType<Rectangle>().Count(); i+=4)
                if (point > i && point < i + 4)
                    return i;
            return 0;
        }

        private void ShiftQuad(int start, int horiz, int vert)
        {
            if (Canvas.Children.OfType<Rectangle>().Count() < 4) return;
            
            var points = Canvas.Children.OfType<Rectangle>().ToList();
            for (var i = 0; i < 4; i++)
            {
                Canvas.SetLeft(points[start + i], Canvas.GetLeft(points[start + i]) - horiz);
                Canvas.SetTop(points[start + i], Canvas.GetTop(points[start + i]) - vert);
            }
        }

        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Create the animation for opacity
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(.2))
            };
            opacityAnimation.Completed += (_, _) => Application.Current.Shutdown();

            // Trigger the animation
            e.Cancel = true;
            BeginAnimation(OpacityProperty, opacityAnimation);
        }

        private void InfoButtonClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/nnmarcoo",
                UseShellExecute = true
            });
        }
        
        
        //TIMER
        private void StartTimer()
        {
            _timer = new Timer();
            _timer.Interval = 10; // is this dumb?
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => CalculateBitmaps());
        }
        
        private void StopTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }
    }
}