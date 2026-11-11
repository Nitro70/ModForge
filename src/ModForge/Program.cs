using System;
using System.Linq;
using System.Runtime.InteropServices;
using ModForge.Core;

namespace ModForge
{
    public static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int processId);

        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--cli")
            {
                AttachConsole(-1);
                return Cli.Run(args.Skip(1).ToArray());
            }

            var app = new App();
            app.InitializeComponent();

            // --screenshot <png> [--select <templateId>] [--theme]: draw the real window to a file without showing it.
            int shot = Array.IndexOf(args, "--screenshot");
            if (shot >= 0 && shot + 1 < args.Length)
            {
                int sel = Array.IndexOf(args, "--select");
                int game = Array.IndexOf(args, "--game");
                int show = Array.IndexOf(args, "--show");
                return Screenshot.Take(args[shot + 1],
                    sel >= 0 && sel + 1 < args.Length ? args[sel + 1] : null,
                    game >= 0 && game + 1 < args.Length ? args[game + 1] : null,
                    show >= 0 && show + 1 < args.Length ? args[show + 1] : null);
            }

            app.Run(new MainWindow());
            return 0;
        }
    }
}
