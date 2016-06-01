using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace KohdAndArt.MediaTransfer
{
    /// <summary>
    /// Interaction logic for AboutBoxWindow.xaml
    /// </summary>
    public partial class AboutBoxWindow : Window
    {
        public AboutBoxWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = hyperLink.NavigateUri.AbsoluteUri
                }
            };
            p.Start();
        }
    }
}
