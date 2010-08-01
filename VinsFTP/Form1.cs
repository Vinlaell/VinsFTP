﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace VinsFTP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        public void Download(string dlname)
        {
            //int z = 0;
            //initialize some path variables from program settings and a ftpwebrequest object
            FtpWebRequest reqFTP;
            string fileName = dlname;
            string filePath = Properties.Settings.Default.localdir;
            string ftpServerIP = Properties.Settings.Default.host;
            string ftpUserID = Properties.Settings.Default.user;
            string ftpPassword = Properties.Settings.Default.pass;
            try
            {
                //initialize filestream
                FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new
                Uri("ftp://" + ftpServerIP + "/" + fileName));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 10240;
                int readCount;

                byte[] buffer = new byte[bufferSize];
                int y = 0;
                int buf = 0;
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                //loop to do the actual transfer
                while (readCount > 0)
                {
                    //write bytes to file
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                    // if statement to reduce number of times reportprogress gets called to save some cpu
                    buf += bufferSize;
                    if (y == 5)
                    //execute backgroundwoeker.reportprogress to send number of bytes sent to main form to update progress bar
                    backgroundWorker1.ReportProgress(buf);
                    y++;
                    if (y > 5)
                        y = 0;
                }
                //close streams
                ftpStream.Close();
                outputStream.Close();
                response.Close();
            }

            catch (Exception ex)
            {
                //error during download so show error message
                MessageBox.Show(ex.Message);
            }
            //return z;
        }

        private void close(object sender, FormClosedEventArgs e)
        {
            //when main from closes save settings to disk
            Properties.Settings.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //execute local and remote GetFileList() functions to create 2 string arrays containing lists of files of both dirs
            //GetRemoteFileSize rfsize = new GetRemoteFileSize();
            RefreshListing refresher = new RefreshListing();
            int x = 0;
            int y = 0;
            refresher.RefreshNames();
                listView1.Clear();
                listView1.Columns.Add("Name", 180);
                listView1.Columns.Add("Size", 80);
                listView1.Columns.Add("Date", 135);
                listView2.Clear();
                listView2.Columns.Add("Name",180);
                listView2.Columns.Add("Size",80);
                listView2.Columns.Add("Date",135);
                foreach (string val in VinsFTP.Properties.Settings.Default.lnamelist)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = val;
                    lvi.SubItems.Add(refresher.GetArrayOfLocalSizes()[x]);
                    lvi.SubItems.Add(refresher.GetArrayOfLocalDates()[x]);
                    listView1.Items.Add(lvi);
                    x++;
                }

                foreach (string val in VinsFTP.Properties.Settings.Default.rnamelist)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = val;
                    lvi.SubItems.Add(refresher.GetArrayOfRemoteSizes()[y]);
                    lvi.SubItems.Add(refresher.GetArrayOfRemoteDates()[y]);
                    listView2.Items.Add(lvi);
                    y++;
                }

        }

        private void button9_Click(object sender, EventArgs e)
        {
            //start dialog to select local directory
            folderBrowserDialog1.ShowDialog();
            //apply program setting string "localdir" to the selected folder from the dialog
            Properties.Settings.Default.localdir = folderBrowserDialog1.SelectedPath;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //button to open localdir (save destination) with your os explorer
            System.Diagnostics.Process.Start(VinsFTP.Properties.Settings.Default.localdir);
        }

        private void button6_Click(object sender, EventArgs e)
        {   
            //download button clicked
            if (VinsFTP.Properties.Settings.Default.split == false)
            {
                //initialize GetRemoteFileSize function
                GetRemoteFileSize rsize = new GetRemoteFileSize();
                //set program property string "dlname" to the selected files name
                Properties.Settings.Default.dlname = listView2.SelectedItems[0].Text;
                //execute function GetSize() to apply program property int"dlsize" to the size of remote file
                rsize.GetSize();
                //set progressbar's max value to the size of remote file
                toolStripProgressBar1.Maximum = Properties.Settings.Default.dlsize;
                //start backgroundworker to download on a seperate thread so main form stays responsive
                backgroundWorker1.RunWorkerAsync();
                toolStripStatusLabel1.Text = "Downloading:";
                toolStripStatusLabel2.Text = listView2.SelectedItems[0].Text;
            }
            else
            {

            }
        }

        public void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Download(Properties.Settings.Default.dlname);
        }

        public void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //bug:(fixed i think) still trying to apply a value larger than maximum of progress bar
            if (e.ProgressPercentage > Properties.Settings.Default.dlsize)
                toolStripProgressBar1.Value = Properties.Settings.Default.dlsize;
            else
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        public void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //when backgroundworker is completed, set progressbar(s) to full (just to be sure because my code doesnt seem to fill the bar when files
            //are smaller than the buffer itself) and then a moment later reset bar(s) to 0
            toolStripProgressBar1.Maximum = 100;
            toolStripProgressBar1.Value = 100;
            System.Threading.Thread.Sleep(500);
            toolStripProgressBar1.Value = 0;
            toolStripStatusLabel1.Text = "Idle:";
            toolStripStatusLabel2.Text = "";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //cancel isnt working
            backgroundWorker1.CancelAsync();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //clicking checkbox for file splitting makes visible or invisible the second progressbar, one shows progress of a chunk, other is of the whole
            if (checkBox1.Checked == true)
                toolStripProgressBar2.Visible = true;
            else
                toolStripProgressBar2.Visible = false;
        }



    }
}
