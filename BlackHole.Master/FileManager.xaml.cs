using System.Windows.Input;

namespace BlackHole.Master
{
    /// <summary>
    /// Logique d'interaction pour FileManager.xaml
    /// </summary>
    public partial class FileManager : SlaveWindow
    {
        /// <summary>
        /// 
        /// </summary>
        public FileManager()
        {
            InitializeComponent();      
        }

        private void TxtBoxDirectory_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter:
                    NavigateToTypedFolder();
                    break;
            }
        }

        private void TxtBoxUpload_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    UploadTypedFile();
                    break;
            }
        }
    }
}
