using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Flee.PublicTypes;
using System.IO;
using System.Windows.Forms;

namespace PVPlus
{
    class Program
    {
        //Squirrel --releasify PVPlus.1.0.0.nupkg

        [STAThread]
        static void Main(string[] args)
        {
            Application.Run(new UI.MainForm());
        }
    }

}
