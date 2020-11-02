using CommandLine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public class Options
    {
        [Option("server", Required = false, HelpText = "Specifies whether or not AstroModLoader is being ran for a server.")]
        public bool ServerMode { get; set; }

        [Option("data", Required = false, HelpText = "Specifies the %localappdata% folder, or the equivalent of it.")]
        public string LocalDataPath { get; set; }
    }

    public static class Program
    {
        public static Options CommandLineOptions;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                CommandLineOptions = o;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Form1 f1 = new Form1();
                Application.Run(f1);
            });
        }
    }
}
