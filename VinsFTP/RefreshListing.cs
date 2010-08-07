using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace VinsFTP
{
    public class RefreshListing
    {
        public void RefreshNames()
        {
            GetRemoteFileList();
            GetLocalFileList();
        }

        public void GetRemoteFileList()
        {
            //initialize a ftpwebrequest object and some program settings path variables
            string ftpServerIP = Properties.Settings.Default.host;
            string ftpUserID = Properties.Settings.Default.user;
            string ftpPassword = Properties.Settings.Default.pass;
            StringBuilder result = new StringBuilder();
            FtpWebRequest reqFTP;
            try
            {
                //make a ftpwebrequest
                reqFTP = (FtpWebRequest)FtpWebRequest.Create
                   (new Uri("ftp://" + ftpServerIP + "/"));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID,
                                                           ftpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new
                StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                    //bug FIX this filter!
                {
                    if (VinsFTP.Properties.Settings.Default.hidechunks != true)
                    {
                        result.Append(line);
                        result.Append("\n");
                    }
                    else if (VinsFTP.Properties.Settings.Default.hidechunks == true)
                    {
                        if (line.Substring(line.Length - 7) == "001.cnk")
                    {
                        result.Append(line);
                        result.Append("\n");
                    }
                    if (line.Substring(line.Length - 4) != ".cnk")
                    {
                        result.Append(line);
                        result.Append("\n");
                    }
                }
                    line = reader.ReadLine();
                }
                // to remove the trailing '\n'
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();
                string[] res = result.ToString().Split('\n');
                VinsFTP.Properties.Settings.Default.rnamelist.Clear();
                VinsFTP.Properties.Settings.Default.rnamelist.Add("empty");
                VinsFTP.Properties.Settings.Default.rnamelist.AddRange(res);
                VinsFTP.Properties.Settings.Default.rnamelist.RemoveAt(0);
            }
            catch (Exception ex)
            {
                //error happened so show error msg
                MessageBox.Show(ex.Message);
            }
        }

        public void GetLocalFileList()
        {
            //creates and returns a string array of files in selected local directory
            int x = 0;
            //if program setting localdir is empty set a default value of C:\\
            if (Properties.Settings.Default.localdir == "")
                Properties.Settings.Default.localdir = "C:\\";
            //create a directoryinfo object from the localdir var from program settings
            DirectoryInfo di = new DirectoryInfo(Properties.Settings.Default.localdir);
            //create a stringbuilder string to make a string array to return when function ends
            StringBuilder returns = new StringBuilder();
            //make a array of fileinfo objects of local dir
            FileInfo[] fiArr = di.GetFiles();
            //loop through the array and add each files name to stringbuilder string with a null character after each one(to make a propper array of strings)
            while (x < fiArr.Length)
            {
                                    if (VinsFTP.Properties.Settings.Default.hidechunks != true)
                    {
                returns.Append(fiArr[x]);
                returns.Append("\n");
                    }
                                    else if (VinsFTP.Properties.Settings.Default.hidechunks == true)
                                    {
                                        if (fiArr[x].Name.Substring(fiArr[x].Name.Length - 7) == "001.cnk")
                                        {
                                            returns.Append(fiArr[x].Name);
                                            returns.Append("\n");
                                        }
                                        if (fiArr[x].Name.Substring(fiArr[x].Name.Length - 4) != ".cnk")
                                        {
                                            returns.Append(fiArr[x].Name);
                                            returns.Append("\n");
                                        }
                                    }
                x++;
            }
            returns.Remove(returns.ToString().LastIndexOf('\n'), 1);
            string[] res = returns.ToString().Split('\n');
            VinsFTP.Properties.Settings.Default.lnamelist.Clear();
            VinsFTP.Properties.Settings.Default.lnamelist.Add("empty");
            VinsFTP.Properties.Settings.Default.lnamelist.AddRange(res);
            VinsFTP.Properties.Settings.Default.lnamelist.RemoveAt(0);
        }

        public string[] GetArrayOfRemoteSizes()
        {
            int count = VinsFTP.Properties.Settings.Default.rnamelist.Count;
            int x = 0;
            string[] array = new string[count];
            foreach (string name in VinsFTP.Properties.Settings.Default.rnamelist)
            {
                try
                {
                    //gets the filesize of file on remote server
                    //grabs some path variables from program settings
                    string ftpServerIP = Properties.Settings.Default.host;
                    string ftpUserID = Properties.Settings.Default.user;
                    string ftpPassword = Properties.Settings.Default.pass;
                    //initialize a ftpwebrequest object
                    FtpWebRequest reqFTP;
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new
                    Uri("ftp://" + ftpServerIP + "/" + name));
                    reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    long cl = response.ContentLength;
                    array.SetValue(cl.ToString(), x);
                    response.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                x++;
            }
            return array;
        }

        public string[] GetArrayOfLocalSizes()
        {
            int count = VinsFTP.Properties.Settings.Default.lnamelist.Count;
            int x = 0;
            string[] array = new string[count];
            DirectoryInfo di = new DirectoryInfo(Properties.Settings.Default.localdir);
            FileInfo[] fiArr = di.GetFiles();
            foreach (FileInfo file in fiArr)
            {
                //bug FIX this
                array.SetValue(file.Length.ToString(), x);
                x++;
            }
            return array;
        }

        public string[] GetArrayOfLocalDates()
        {
            int count = VinsFTP.Properties.Settings.Default.lnamelist.Count;
            int x = 0;
            string[] array = new string[count];
            DirectoryInfo di = new DirectoryInfo(Properties.Settings.Default.localdir);
            FileInfo[] fiArr = di.GetFiles();
            foreach (FileInfo file in fiArr)
            {
                array.SetValue(file.LastWriteTime.ToString(), x);
                x++;
            }
            return array;
        }

        public string[] GetArrayOfRemoteDates()
        {
            int count = VinsFTP.Properties.Settings.Default.rnamelist.Count;
            int x = 0;
            string[] array = new string[count];
            foreach (string name in VinsFTP.Properties.Settings.Default.rnamelist)
            {
                try
                {
                    //gets the filesize of file on remote server
                    //grabs some path variables from program settings
                    string ftpServerIP = Properties.Settings.Default.host;
                    string ftpUserID = Properties.Settings.Default.user;
                    string ftpPassword = Properties.Settings.Default.pass;
                    //initialize a ftpwebrequest object
                    FtpWebRequest reqFTP;
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new
                    Uri("ftp://" + ftpServerIP + "/" + name));
                    reqFTP.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    array.SetValue(response.LastModified.ToString(),x);
                    response.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                x++;
            }
            return array;
        }
    }
}
