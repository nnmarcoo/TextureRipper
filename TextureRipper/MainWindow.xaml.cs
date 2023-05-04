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
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }


        private void ExitButtonUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void FileButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void LineButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void GridButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void SaveButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}