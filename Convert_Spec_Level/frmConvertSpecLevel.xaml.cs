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
using System.Diagnostics;

namespace SandBox
{
    /// <summary>
    /// Interaction logic for frmConvertSpecLevel.xaml
    /// </summary>
    public partial class frmConvertSpecLevel : Window
    {
        public frmConvertSpecLevel()
        {
            InitializeComponent();

            // create a list of LGI division clients
            List<string> listClients = new List<string> { "Central Texas", "Dallas/Ft Worth",
                "Florida", "Houston", "Maryland", "Minnesota", "Oklahoma", "Pennsylvania",
                "Southeast", "Virginia", "West Virginia" };

            // add each client to the combobox
            foreach (string client in listClients)
            {
                cmbClient.Items.Add(client);
            }

            // set the default selection to the first client in the list (Central Texas)
            cmbClient.SelectedIndex = 0;

            // create a list of MW cabinet heights
            List<string> listMWCabinets = new List<string> { "18\"", "21\"", "24\"", "27\"", "30\"" };

            // add each client to the comboxbox
            foreach (string height in listMWCabinets)
            {
                cmbMWCabHeight.Items.Add(height);
            }

            // set the default selection to the first height in the list
            cmbMWCabHeight.SelectedIndex = 0;
        }

        public string GetSelectedClient()
        {
            return cmbClient.SelectedItem as string;
        }        

        public string GetSelectedSpecLevel()
        {
            if (rbCompleteHome.IsChecked == true)
            {
                return rbCompleteHome.Content.ToString();
            }
            else
            {
                 return rbCompleteHomePlus.Content.ToString();
            }           
        }

        public string GetSelectedMWCabHeight()
        {
            return cmbMWCabHeight.SelectedItem as string;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // launch the help site with user's default browser
                string helpUrl = "https://lifestyle-usa-design.atlassian.net/wiki/spaces/MFS/pages/472711169/Spec+Level+Conversion?atlOrigin=eyJpIjoiMmU4MzM3NzFmY2NlNDdiNjk1MjY2M2MyYzZkMjY2YWQiLCJwIjoiYyJ9";
                Process.Start(new ProcessStartInfo
                {
                    FileName = helpUrl,
                    UseShellExecute = true
                });

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An error occurred while trying to display help: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
