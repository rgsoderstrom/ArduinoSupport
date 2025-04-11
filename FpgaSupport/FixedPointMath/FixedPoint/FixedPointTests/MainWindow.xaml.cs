using System;
using System.Collections.Generic;
using System.Windows;

using Common;
using Plot2D_Embedded;

//using FixedPt = FixedPointLib.FixedPoint_1_7_24;
using FixedPt = FixedPointLib.FixedPoint_1_5_10;

using System.Windows.Controls;
using System.Threading;
using System.Windows.Media;

#pragma warning disable IDE0051
#pragma warning disable IDE0052

namespace FixedPointTests
{
    public delegate void PrintCallback (string str);

    public partial class MainWindow : System.Windows.Window
    {        
        //private readonly IFixedPtTest testObject = new FirFilterTest (); 
        private readonly IFixedPtTest testObject = new WindowedSineTest (); 

        private bool WindowIsLoaded   = false;
        private bool ButtonsAreLoaded = false;
        private bool TextAreaIsLoaded = false;
        private bool PlotAreaIsLoaded = false;
        private bool CalculationsRun  = false;

        public MainWindow ()
        {
            InitializeComponent ();
            EventLog.Open (@"..\..\log.txt");

            // only this thread can access WPF objects
            WpfThread = Thread.CurrentThread.ManagedThreadId;
        }

        //*********************************************************************************

        // only the thread that created WPF objects can access them. others must use Invoke () to
        // run a task on that thread. Its ID stored here
        readonly int WpfThread;

        //*********************************************************************************
        //
        // See if we are ready to initialize
        //
        private void CheckWindowStatus ()
        {
            if (WindowIsLoaded   == true
             && ButtonsAreLoaded == true
             && TextAreaIsLoaded == true
             && PlotAreaIsLoaded == true
             && CalculationsRun  == false)
            {
                testObject.Print = Print;
                testObject.PlotArea = PlotArea;

                ButtonPanel.IsEnabled = true;
                ZoomX_Button.IsChecked = true;

                testObject.DoCalculations ();
                testObject.DoPlots ();

                EventLog.WriteLine ("Calculations and Plots done");
                CalculationsRun = true;
            }
        }




        private void Window_Loaded (object sender, RoutedEventArgs e)
        {
            WindowIsLoaded = true;
            CheckWindowStatus ();
            EventLog.WriteLine ((sender as FrameworkElement).Name + " loaded");
        }

        private void PlotArea_Loaded (object sender, RoutedEventArgs e)
        {
            PlotAreaIsLoaded = true;
            CheckWindowStatus ();
            EventLog.WriteLine ((sender as FrameworkElement).Name + " loaded");
        }

        private void TextArea_Loaded (object sender, RoutedEventArgs e)
        {
            TextAreaIsLoaded = true;
            CheckWindowStatus ();
            EventLog.WriteLine ((sender as FrameworkElement).Name + " loaded");
        }

        private void ButtonArea_Loaded (object sender, RoutedEventArgs e)
        {
            ButtonsAreLoaded = true;
            CheckWindowStatus ();
            EventLog.WriteLine ((sender as FrameworkElement).Name + " loaded");
        }


        //************************************************************************************

        private void ZoomOptionButton_Checked (object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                string tag = rb.Tag as string;

                switch (tag)
                {
                    case "Zoom_Both":
                        PlotArea.ZoomX = true;
                        PlotArea.ZoomY = true;
                        break;

                    case "Zoom_X":
                        PlotArea.AxesEqual = false;
                        PlotArea.ZoomX = true;
                        PlotArea.ZoomY = false;
                        break;

                    case "Zoom_Y":
                        PlotArea.AxesEqual = false;
                        PlotArea.ZoomX = false;
                        PlotArea.ZoomY = true;
                        break;

                    default:
                        throw new Exception ("Invalid zoom option");
                }
            }
        }

        //*******************************************************************************************************

        static int localLineNumber = 1;
        object LocalTextBoxLock = new object ();

        void AddTextToLocalTextBox (string str)
        {
            EventLog.WriteLine (str);

            lock (LocalTextBoxLock)
            {
                TextDisplay.Text += string.Format ("{0}: ", localLineNumber++);
                TextDisplay.Text += str;
                TextDisplay.Text += "\n";
            }

            TextDisplay.ScrollToEnd ();
        }

        public void Print (string str)
        {
            int callingThread = Thread.CurrentThread.ManagedThreadId;

            //EventLog.WriteLine ("Print: " + str + ", calling thread " + callingThread.ToString () + ", WPF Thread " + WpfThread);

            if (callingThread == WpfThread)
            {
                AddTextToLocalTextBox (str);
            }
            else
            {
                Dispatcher.BeginInvoke ((PrintCallback)AddTextToLocalTextBox, str);
            }
        }
    }
}
