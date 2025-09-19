using SandBox.Common;
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

namespace SandBox
{
    /// <summary>
    /// Interaction logic for frmSchedOrg.xaml
    /// </summary>
    public partial class frmSchedOrg : Window
    {
        private string _url;
        public frmSchedOrg(string url)
        {
            InitializeComponent();
            _url = url;
        }

        private void InstructionsLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the URL in the default browser
                Process.Start(new ProcessStartInfo(_url) { UseShellExecute = true });

                // Close the dialog
                this.Close();
            }
            catch (System.Exception ex)
            {
                Utils.TaskDialogError("Error", "Browser Organization", $"Could not open link: {ex.Message}");                
            }
        }
    }
}
