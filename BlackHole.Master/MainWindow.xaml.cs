using BlackHole.Common;
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

namespace BlackHole.Master
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        /// <summary>
        /// 
        /// </summary>
        private MainWindow()
        {
            m_childWindows = new List<Window>();

            InitializeComponent();     
            Initialize();

            Closing += MainWindow_Closing;
        }
    }
}
