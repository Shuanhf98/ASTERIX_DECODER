﻿using System;
using System.Collections.Generic;
using System.Data;
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
using CLASSES;
using Microsoft.Win32;
using GMap.NET;
using GMap.NET.MapProviders;

namespace ASTERIX_APP
{
    public partial class MainWindow : Window
    {
        Fichero F;
        int category;

        //lat and lon os cat10 files 
        double latindegrees;
        double lonindegrees;

        //lat lon os MLAT system of reference (at LEBL airport)
        double MLAT_lat = 41.29694444;
        double MLAT_lon = 2.07833333;
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer(); 

        public MainWindow()
        {
            InitializeComponent();        
                       
            Instructions_Label.Visibility = Visibility.Visible; ;
            Instructions_Label.FontSize = 18;
            Instructions_Label.Content = "Welcome to ASTERIX APP!" + '\n' + '\n' + "We need some file to read!" + '\n' +
                "Please, load a '.ast' format file with the 'Load File' button above.";
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Are you sure you want to exit?");
            this.Close();
        }
        public void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            Track_Table.ItemsSource = null;
            Track_Table.Items.Clear();

            OpenFileDialog OpenFile = new OpenFileDialog();
            Instructions_Label.Content = "Loading...";
            OpenFile.ShowDialog();            
            F = new Fichero(OpenFile.FileName);            
            F.leer();

            Instructions_Label.Content = "Perfectly read!" + '\n' + "1) View the displayed data by clicking on 'Tracking Table'" +
                    '\n' + "2) Run a data simulation by clicking on 'Tracking Map'";

            MapButton.Visibility = Visibility.Visible; ;
            TableButton.Visibility = Visibility.Visible; ;
        }
        private void TableTrack_Click(object sender, RoutedEventArgs e)
        {
            Instructions_Label.Visibility = Visibility.Hidden;
            Track_Table.Visibility = Visibility.Visible;
            map.Visibility = Visibility.Hidden;
            gridlista.Visibility = Visibility.Hidden;            

            if (F.CAT_list[0] == 10)
            {
                bool IsMultipleCAT = F.CAT_list.Contains(21);
                if (IsMultipleCAT == true)
                { 
                    Track_Table.ItemsSource = F.getTablaMixtCAT().DefaultView;
                    category = 1021;
                }
                else
                { 
                    Track_Table.ItemsSource = F.getTablaCAT10().DefaultView;
                    category = 10;
                }
            }
            if (F.CAT_list[0] == 21)
            {
                bool IsMultipleCAT = F.CAT_list.Contains(10);
                if (IsMultipleCAT == true) 
                {
                    Track_Table.ItemsSource = F.getTablaMixtCAT().DefaultView;
                    category = 1021;
                }
                else 
                { 
                    Track_Table.ItemsSource = F.getTablaCAT21().DefaultView;
                    category = 21;
                }
            }            
        }
        private void MapTrack_Click(object sender, RoutedEventArgs e)
        {
            Instructions_Label.Visibility = Visibility.Hidden;
            Track_Table.Visibility = Visibility.Hidden;
            map.Visibility = Visibility.Visible;
            gridlista.Visibility = Visibility.Visible;
            

            if (F.CAT_list[0] == 10)
            {
                bool IsMultipleCAT = F.CAT_list.Contains(21);
                if (IsMultipleCAT == true) { gridlista.ItemsSource = F.getmultiplecattablereducida().DefaultView; }
                else { gridlista.ItemsSource = F.gettablacat10reducida().DefaultView; }
            }
            if (F.CAT_list[0] == 21)
            {
                bool IsMultipleCAT = F.CAT_list.Contains(10);
                if (IsMultipleCAT == true) { gridlista.ItemsSource = F.getmultiplecattablereducida().DefaultView; }
                else { gridlista.ItemsSource = F.gettablacat21reducida().DefaultView; }
            }
        }
        private void Map_Load(object sender, RoutedEventArgs e)
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            map.MapProvider = OpenStreetMapProvider.Instance;
            map.MinZoom = 8;
            map.MaxZoom = 16;
            map.Zoom = 14;           
            map.Position = new PointLatLng(MLAT_lat,MLAT_lon);
            map.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            map.CanDragMap = true;
            map.DragButton = MouseButton.Left;
      
        }
       
        private PointLatLng cartesiantolatlonMLAT(double X, double Y)
        {
            double RAD = 6371 * 1000;
            double d = Math.Sqrt((X * X) + (Y * Y));
            double b = Math.Atan2(Y, -X) - (Math.PI / 2);
            double phi1 =MLAT_lat * (Math.PI / 180);
            double lamda1 = MLAT_lon * (Math.PI / 180);
            var phi2 = Math.Asin(Math.Sin(phi1) * Math.Cos(d / RAD) + Math.Cos(phi1) * Math.Sin(d / RAD) * Math.Cos(b));
            var lamda2 = lamda1 + Math.Atan2(Math.Sin(b) * Math.Sin(d / RAD) * Math.Cos(phi1), Math.Cos(d / RAD) - Math.Sin(phi1) * Math.Sin(phi2));
            PointLatLng latlonMLAT = new PointLatLng(phi2 * (180 / Math.PI), lamda2 * (180 / Math.PI));
            return latlonMLAT;
        }
        public double getlatMLAT()
        {
            return latindegrees;
        }
        public double getlonMLAT()
        {
            return lonindegrees;
        }
        void ClickDataGrid(object sender, RoutedEventArgs e) // When we click over a clickable cell
        {
            DataGridCell cell = (DataGridCell)sender;
            // pick the column number of the clicked cell
            int Col_Num = cell.Column.DisplayIndex;
            // pick the row number of the clicked cell
            DataGridRow row = DataGridRow.GetRowContainingElement(cell);
            int Row_Num = row.GetIndex();

            // CAT 10 case
            if (category == 10)
            {
                CAT10 pack = F.getCAT10(Row_Num);
                if (Col_Num == 4 && pack.Target_Rep_Descript != null)
                {
                    string[] TRD = pack.Target_Rep_Descript;
                    MessageBox.Show("Target Report:\n\nTYP: " + TRD[0] + "\nDCR: " + TRD[1] + "\nCHN: " + TRD[2] + "\nGBS: " + TRD[3] +
                        "\nCRT: " + TRD[4] + "\nSIM: " + TRD[5] + "\nTST: " + TRD[6] + "\nRAB" + TRD[7] + "\nLOP: " + TRD[8] + "\nTOT: " +
                        TRD[9] + "\nSPI" + TRD[10]);
                }
                if (Col_Num == 12 && pack.Track_Status != null)
                {
                    string[] TS = pack.Track_Status;
                    MessageBox.Show("Track Status:\n\nCNF: " + TS[0] + "\nTRE: " + TS[1] + "\nCST: " + TS[2] + "\nMAH: " + TS[3] +
                        "\nTCC: " + TS[4] + "\nSTH: " + TS[5] + "\nTOM: " + TS[6] + "\nDOU: " + TS[7] + "\nMRS: " + TS[8] + "\nGHO: " + TS[9]);
                }
                if (Col_Num == 13 && pack.Mode3A_Code != null)
                {
                    string[] M3A = pack.Mode3A_Code;
                    MessageBox.Show("Mode 3/A Code:\n\nV: " + M3A[0] + "\nG: " + M3A[1] + "\nL: " + M3A[2] + "\n Code: " + M3A[3]);
                }
                if (Col_Num == 15 && pack.Mode_SMB != null)
                {
                    string[] MSMB = pack.Mode_SMB;
                    MessageBox.Show("Mode S MB:\n\nREP: " + MSMB[0] + "\nMB: " + MSMB[1] + "\nBDS 1: " + MSMB[2] + "\nBDS 2: " + MSMB[3]);
                }
                if (Col_Num == 21 && pack.Sys_Status != null)
                {
                    string[] SS = pack.Sys_Status;
                    MessageBox.Show("System Status:\n\nNOGO: " + SS[0] + "\nOVL: " + SS[1] + "\nTSV: " + SS[2] + "\nDIV: " + SS[3] +
                        "\nTTF: " + SS[5]);
                }
                if (Col_Num == 22 && pack.Pre_Prog_Message != null)
                {
                    string[] PPM = pack.Pre_Prog_Message;
                    MessageBox.Show("Pre-Programmed Message:\n\nTRB: " + PPM[0] + "Message: " + PPM[1]);
                }
                if (Col_Num == 25 && pack.Presence != null)
                {
                    double[] P = pack.Presence;
                    MessageBox.Show("Presence:\n\nREP: " + P[0] + "\nDifference of Rho: " + P[1] + "\nDifference of Theta: " + P[2]);
                }
            }
            // CAT 21 case
            if (category == 21)
            {
                CAT21 pack = F.getCAT21(Row_Num);
                if(Col_Num == 4 && pack.Target_Report_Desc != null)
                {
                    string[] TRD = pack.Target_Report_Desc;
                }
                if (Col_Num == 10 && pack.Op_Status != null)
                {
                    string[] OS = pack.Op_Status;
                }
                if (Col_Num == 16 && pack.MOPS != null)
                {
                    string[] MOPS = pack.MOPS;
                }
                if (Col_Num == 21 && pack.Met_Report != null)
                {
                    string[] MR = pack.Met_Report;
                }
                if (Col_Num == 24 && pack.Target_Status != null)
                {
                    string[] TS = pack.Target_Status;
                }
                if (Col_Num == 27 && pack.Quality_Indicators != null)
                {
                    string[] QI = pack.Quality_Indicators;
                }
                if (Col_Num == 28 && pack.Mode_S != null)
                {
                    int[] MS = pack.Mode_S;
                }
                if (Col_Num == 35 && pack.TMRP_HP != null)
                {
                    string[] TMRP = pack.TMRP_HP;
                }
                if (Col_Num == 36 && pack.TMRV_HP != null)
                {
                    string[] TMRV = pack.TMRV_HP;
                }
                if (Col_Num == 38 && pack.Trajectory_Intent != null)
                {
                    string[] TI = pack.Trajectory_Intent;
                }
                if (Col_Num == 39 && pack.Data_Ages != null)
                {
                    double[] DA = pack.Data_Ages;
                }
            }
            // Mixt category
            if (category == 1021)
            {

            }
        }
    }
}
