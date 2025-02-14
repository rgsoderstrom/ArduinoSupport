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

namespace PioneerSensors
{
    public partial class SaveDialogBox : Window
    {
        public string FileName
        {
            set {FileName_TB.Content = value;}
            get {return FileName_TB.Content as string;}
            //set {FileName_TB.Text = value;}
            //get {return FileName_TB.Text;}
        }

        public string FileComments
        {
            set {FileComments_TB.Text = value;}
            get {return FileComments_TB.Text;}
        }

        public SaveDialogBox ()
        {
            InitializeComponent ();
        }

        private void okButton_Click (object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
