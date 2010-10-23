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
using System.Speech.Synthesis;
using System.Speech.Recognition;

namespace VinsFTP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
                    
        SpeechSynthesizer speech = new SpeechSynthesizer();
        SpeechRecognitionEngine recognitionEngine; 

        private void Form1_Load(object sender, EventArgs e)
        {
            Initialize();
            if (VinsFTP.Properties.Settings.Default.usereco == true)
            {
                recognitionEngine.UnloadAllGrammars();

                Grammar cg = CreateSampleGrammar();
                recognitionEngine.LoadGrammar(cg);
                recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
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
            if (VinsFTP.Properties.Settings.Default.usetts == true)
                speech.SpeakAsync("Connecting");
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
                ListViewItem lvi2 = new ListViewItem();
                ListViewItem lvi3 = new ListViewItem();
                lvi2.Text = "../Upper directory";
                lvi3.Text = "../Upper directory";
                listView1.Items.Add(lvi2);
                listView2.Items.Add(lvi3);
                foreach (string val in refresher.GetLocalFileList())
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = val;
                    lvi.SubItems.Add(refresher.GetArrayOfLocalSizes()[x]);
                    lvi.SubItems.Add(refresher.GetArrayOfLocalDates()[x]);
                    listView1.Items.Add(lvi);
                    x++;
                }

                foreach (string val in refresher.GetRemoteFileList())
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
            if (listView2.SelectedItems.Count == 0) { MessageBox.Show("No file selected"); return; }
            if (VinsFTP.Properties.Settings.Default.split == false)
            {
                //initialize GetRemoteFileSize function
                GetRemoteFileSize rsize = new GetRemoteFileSize();
                //set program property string "dlname" to the selected files name
                VinsFTP.Properties.Settings.Default.dlname = listView2.SelectedItems[0].Text.Remove(listView2.SelectedItems[0].Text.Length - 1, 1);
                //execute function GetSize() to apply program property int"dlsize" to the size of remote file
                rsize.GetSize();
                //set progressbar's max value to the size of remote file
                toolStripProgressBar1.Maximum = Properties.Settings.Default.dlsize;
                //start backgroundworker to download on a seperate thread so main form stays responsive
                backgroundWorker1.RunWorkerAsync();
                toolStripStatusLabel1.Text = "Downloading:";
                if (VinsFTP.Properties.Settings.Default.usetts == true)
                speech.SpeakAsync("Downloading");
                toolStripStatusLabel2.Text = listView2.SelectedItems[0].Text.Remove(listView2.SelectedItems[0].Text.Length - 1, 1);
            }
            else
            {

            }
        }

        public void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Download(VinsFTP.Properties.Settings.Default.dlname);
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
            if (VinsFTP.Properties.Settings.Default.usetts == true)
                speech.SpeakAsync("Download complete");
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

        private void button4_Click(object sender, EventArgs e)
        {
            //show options dialog
            Form2 frm = new Form2();
            frm.Show();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked == true)
            {
            try
            {
                recognitionEngine.UnloadAllGrammars();

                Grammar cg = CreateSampleGrammar();
                recognitionEngine.LoadGrammar(cg);
                recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            }
            else
            {
                recognitionEngine.RecognizeAsyncStop();
            }
        }

        private void Initialize()
        {
            recognitionEngine = new SpeechRecognitionEngine();
            recognitionEngine.SetInputToDefaultAudioDevice();
            recognitionEngine.SpeechRecognized += (s, args) =>
            {
                foreach (RecognizedWordUnit word in args.Result.Words)
                {
                    // You can change the minimun confidence level here
                    if (word.Confidence > 0.8f)
                    {
                        switch (word.Text)
                        {
                            case "Download": button6.PerformClick();break;
                            case "speech": checkBox4.Checked = true; break;
                            case "Refresh": button1.PerformClick(); break;
                            case "recognition": checkBox5.Checked = false; recognitionEngine.RecognizeAsyncStop(); break;
                            case "Connect": button1.PerformClick(); break;
                            case "select remote": listView2.Select(); break;
                            case "close": closeprog(); break;
                                
                        }
                    }
                }
            };
        }

        private Grammar CreateSampleGrammar()
        {
            Choices commandChoices = new Choices("Download", "speech", "recognition", "Refresh","Connect","select remote","close");
            GrammarBuilder grammarBuilder = new GrammarBuilder("ftp");
            grammarBuilder.Append(commandChoices);
            Grammar g = new Grammar(grammarBuilder);
            g.Name = "Available programs";
            return g;
        }

        public void closeprog()
        {
            VinsFTP.Form1.ActiveForm.Close();
        }

    }
}
