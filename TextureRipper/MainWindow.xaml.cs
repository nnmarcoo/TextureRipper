using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextureRipper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            
        }
        
        private void LeftButton1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Handle the left button 1 click event
        }

        private void LeftButton2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Handle the left button 2 click event
        }

        private void LeftButton3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Handle the left button 3 click event
        }

        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Close the application
            Application.Current.Shutdown();
        }

    }
}