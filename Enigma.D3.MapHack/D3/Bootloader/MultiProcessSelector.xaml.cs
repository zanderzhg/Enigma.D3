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

namespace Enigma.D3.Bootloader
{
    /// <summary>
    /// Interaction logic for MultiProcessSelector.xaml
    /// </summary>
    public partial class MultiProcessSelector : Window
    {
        public MultiProcessSelector(Process[] processes)
        {
            Processes = processes;
            DataContext = this;
            InitializeComponent();
        }

        public Process[] Processes { get; }

        public Process SelectedProcess { get; private set; }

        private void OnMouseDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            SelectedProcess = (sender as ListBoxItem).DataContext as Process;
            DialogResult = true;
        }
    }
}
