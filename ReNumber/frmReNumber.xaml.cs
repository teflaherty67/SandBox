using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for frmReNumber.xaml
    /// </summary>
    public partial class frmReNumber : Window
    {

        ObservableCollection<RenumberData> listRenumElems { get; set; }

        public frmReNumber(List<string> catList)
        {
            InitializeComponent();

            listRenumElems = new ObservableCollection<RenumberData>();

            foreach (var cat in catList)
            {
                RenumberData tempData = new RenumberData(cat, false);
                listRenumElems.Add(tempData);
            }

            lbxReNumElem.ItemsSource = listRenumElems;
        }

        // add code for radio button result

        // add code for textbox result
        public (bool containsLetter, bool containsNumber) GetStartNumber()
        {
            string startNumber = tbxStartNum.Text;

            bool containsLetter = Regex.IsMatch(startNumber, @"[a-zA-Z]");
            bool containsNumber = Regex.IsMatch(startNumber, @"\d");

            return (containsLetter, containsNumber);
        }


        internal bool GetCheckBoxExclude()
        {
            if (cbxExclude.IsChecked == true)
                return true;

            return false;
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

        }
    }

    public class RenumberData
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }

        public RenumberData(string name, bool result)
        {
            Name = name;
            IsChecked = result;
        }
    }
}
