using CommandLine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public class Options
    {
        [Option("base", Required = false, HelpText = "Specifies the local Astro folder (the parent of the SaveGames directory).")]
        public string BasePath { get; set; }
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
