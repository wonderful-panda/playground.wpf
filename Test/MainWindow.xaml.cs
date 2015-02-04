using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.IO;

namespace Test
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
    }

    public class File
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public long Size { get; private set; }
        public DateTime LastWriteTime { get; private set; }
        public File(FileInfo fi)
        {
            this.Name = fi.Name;
            this.Path = System.IO.Path.GetDirectoryName(fi.FullName);
            this.Size = fi.Length;
            this.LastWriteTime = fi.LastWriteTime;
        }
    }

    public class MainWindowViewModel
    {
        public ObservableCollection<File> Files { get; private set; }
        public MainWindowViewModel()
        {
            var directory = new DirectoryInfo(Environment.CurrentDirectory);
            this.Files = new ObservableCollection<File>(directory.GetFiles().Select(fi => new File(fi)));
        }
    }
}
