using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;

namespace VinsFTP
{
    public class GetRemoteFileSize
    {
        public void GetSize()
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
                Uri("ftp://" + ftpServerIP + "/" + Properties.Settings.Default.dlname));
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long cl = response.ContentLength;
                //apply program setting int "dlsize" to contentlength of ftpwebrequest responce  of GetFileSize
                //Properties.Settings.Default.dlsize = response.
                Properties.Settings.Default.dlsize = Convert.ToInt32(cl);
                response.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }
    }
}
