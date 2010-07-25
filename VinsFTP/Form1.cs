using System;
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
            //initialize refresh local list and remote list functions
            RefreshLocalList locallist = new RefreshLocalList();
            RefreshRemoteList remotelist = new RefreshRemoteList();
            //execute local and remote GetFileList() functions to create 2 string arrays containing lists of files of both dirs
            string[] llist = locallist.GetFileList();
            string[] rlist = remotelist.GetFileList();
            //if locallist function returns results clear out local listview and initialize columns
            if (llist != null)
            {
                listView1.Clear();
                listView1.Columns.Add("Name", 180);
                listView1.Columns.Add("Size", 80);
                listView1.Columns.Add("Date", 80);
                //iterate through the local list array adding each name to the listview1
                foreach (string lval in llist)
                {
                    ListViewItem llview = new ListViewItem();
                    listView1.Items.Add(lval);
                }
            }

            //if remotelist function returns results clear out remote listview and initialize columns
            if (rlist != null)
            {
                listView2.Clear();
                listView2.Columns.Add("Name",180);
                listView2.Columns.Add("Size",80);
                listView2.Columns.Add("Date",80);
                //iterate through the remote list array adding each name to the listview2

                foreach (string rval in rlist)
                {
                    ListViewItem lvi = new ListViewItem();
                    listView2.Items.Add(rval);
                }
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
            //undone, was unable to figgure out how to open windows explorer to download destination without using
            //internet as a reference since i was away from home while building this app
            System.Diagnostics.Process.Start("explorer");
        }

        private void button6_Click(object sender, EventArgs e)
        {   
            //download button clicked
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
        }

        public void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Download(Properties.Settings.Default.dlname);
        }

        public void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //bug: still trying to apply a value larger than maximum of progress bar
            if ((toolStripProgressBar1.Value + e.ProgressPercentage) > Properties.Settings.Default.dlsize)
                toolStripProgressBar1.Value = Properties.Settings.Default.dlsize;
            else
            toolStripProgressBar1.Value += e.ProgressPercentage;
        }

        public void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            //cancel isnt working
            backgroundWorker1.CancelAsync();
        }



    }
}
