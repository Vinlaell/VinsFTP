using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VinsFTP
{
    public class RefreshLocalList
    {
    
        public string[] GetFileList()
        {
            //creates andreturns a string array of files in selected local directory
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
                returns.Append(fiArr[x]);
                returns.Append("\n");
                x++;
            }
            returns.Remove(returns.ToString().LastIndexOf('\n'), 1);
            return returns.ToString().Split('\n');
        }
    }
}
