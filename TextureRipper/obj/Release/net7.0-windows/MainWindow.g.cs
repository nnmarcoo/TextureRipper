﻿#pragma checksum "..\..\..\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "7406209CDB605BDF5B27E409AB2F908B0B799A4D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace TextureRipper {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 13 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TextureRipper.MainWindow Window;
        
        #line default
        #line hidden
        
        
        #line 70 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border CanvasGrid;
        
        #line default
        #line hidden
        
        
        #line 90 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas Canvas;
        
        #line default
        #line hidden
        
        
        #line 99 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image SourceImage;
        
        #line default
        #line hidden
        
        
        #line 105 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image PreviewImage;
        
        #line default
        #line hidden
        
        
        #line 119 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image FileImage;
        
        #line default
        #line hidden
        
        
        #line 126 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock FileText;
        
        #line default
        #line hidden
        
        
        #line 137 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock Warning;
        
        #line default
        #line hidden
        
        
        #line 146 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock Info;
        
        #line default
        #line hidden
        
        
        #line 241 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ProgressBar ProgressBar;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.9.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TextureRipper;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.9.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.Window = ((TextureRipper.MainWindow)(target));
            
            #line 14 "..\..\..\MainWindow.xaml"
            this.Window.Closing += new System.ComponentModel.CancelEventHandler(this.MainWindowClosing);
            
            #line default
            #line hidden
            
            #line 15 "..\..\..\MainWindow.xaml"
            this.Window.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.MainWindow_OnPreviewKeyDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.CanvasGrid = ((System.Windows.Controls.Border)(target));
            
            #line 74 "..\..\..\MainWindow.xaml"
            this.CanvasGrid.Drop += new System.Windows.DragEventHandler(this.ImageDrop);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Canvas = ((System.Windows.Controls.Canvas)(target));
            
            #line 91 "..\..\..\MainWindow.xaml"
            this.Canvas.MouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.CanvasMouseRightButtonDown);
            
            #line default
            #line hidden
            
            #line 92 "..\..\..\MainWindow.xaml"
            this.Canvas.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.ManipulatePoint);
            
            #line default
            #line hidden
            
            #line 93 "..\..\..\MainWindow.xaml"
            this.Canvas.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.DragPoint);
            
            #line default
            #line hidden
            
            #line 94 "..\..\..\MainWindow.xaml"
            this.Canvas.MouseMove += new System.Windows.Input.MouseEventHandler(this.CanvasMouseMove);
            
            #line default
            #line hidden
            
            #line 95 "..\..\..\MainWindow.xaml"
            this.Canvas.DragOver += new System.Windows.DragEventHandler(this.CanvasOnDragOver);
            
            #line default
            #line hidden
            
            #line 96 "..\..\..\MainWindow.xaml"
            this.Canvas.MouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.ZoomImage);
            
            #line default
            #line hidden
            return;
            case 4:
            this.SourceImage = ((System.Windows.Controls.Image)(target));
            
            #line 100 "..\..\..\MainWindow.xaml"
            this.SourceImage.MouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.CanvasMouseRightButtonDown);
            
            #line default
            #line hidden
            
            #line 101 "..\..\..\MainWindow.xaml"
            this.SourceImage.MouseMove += new System.Windows.Input.MouseEventHandler(this.CanvasMouseMove);
            
            #line default
            #line hidden
            return;
            case 5:
            this.PreviewImage = ((System.Windows.Controls.Image)(target));
            
            #line 108 "..\..\..\MainWindow.xaml"
            this.PreviewImage.MouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.CanvasMouseRightButtonDown);
            
            #line default
            #line hidden
            
            #line 109 "..\..\..\MainWindow.xaml"
            this.PreviewImage.MouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.ZoomPreviewImage);
            
            #line default
            #line hidden
            
            #line 110 "..\..\..\MainWindow.xaml"
            this.PreviewImage.MouseMove += new System.Windows.Input.MouseEventHandler(this.MouseMovePreviewImage);
            
            #line default
            #line hidden
            
            #line 111 "..\..\..\MainWindow.xaml"
            this.PreviewImage.MouseLeave += new System.Windows.Input.MouseEventHandler(this.MouseLeavePreviewImage);
            
            #line default
            #line hidden
            
            #line 112 "..\..\..\MainWindow.xaml"
            this.PreviewImage.MouseEnter += new System.Windows.Input.MouseEventHandler(this.MouseEnterPreviewImage);
            
            #line default
            #line hidden
            
            #line 113 "..\..\..\MainWindow.xaml"
            this.PreviewImage.DragOver += new System.Windows.DragEventHandler(this.CanvasOnDragOver);
            
            #line default
            #line hidden
            
            #line 114 "..\..\..\MainWindow.xaml"
            this.PreviewImage.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.ManipulatePoint);
            
            #line default
            #line hidden
            return;
            case 6:
            this.FileImage = ((System.Windows.Controls.Image)(target));
            
            #line 123 "..\..\..\MainWindow.xaml"
            this.FileImage.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.FileButtonUp);
            
            #line default
            #line hidden
            return;
            case 7:
            this.FileText = ((System.Windows.Controls.TextBlock)(target));
            
            #line 134 "..\..\..\MainWindow.xaml"
            this.FileText.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.FileButtonUp);
            
            #line default
            #line hidden
            return;
            case 8:
            this.Warning = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 9:
            this.Info = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 10:
            
            #line 170 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Border)(target)).MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.DragWindow);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 191 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SaveButtonClick);
            
            #line default
            #line hidden
            return;
            case 12:
            
            #line 207 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ResetButtonClick);
            
            #line default
            #line hidden
            return;
            case 13:
            
            #line 220 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.InfoButtonClick);
            
            #line default
            #line hidden
            return;
            case 14:
            
            #line 233 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ExitButtonClick);
            
            #line default
            #line hidden
            return;
            case 15:
            this.ProgressBar = ((System.Windows.Controls.ProgressBar)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

