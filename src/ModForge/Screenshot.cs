using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ModForge.Core;

namespace ModForge
{
    /// <summary>
    /// Draws the real window to a PNG without ever showing it, so the UI can be checked with nothing popping up.
    /// Settings are read-only in this mode.
    /// </summary>
    public static class Screenshot
    {
        public static int Take(string png, string templateId, string gameName, string show)
        {
            try
            {
                string info;
                var pack = PackStore.LoadAll(out info);
                bool demo = Environment.GetCommandLineArgs().Contains("--demo");
                GameScanner.Demo = demo;
                // Demo mode reads nothing of this PC: no saved settings, no library, a made-up output folder.
                var settings = demo ? new Settings() : Settings.Load();
                settings.ReadOnly = true;
                var vm = new MainVM(pack, info, settings, true);
                if (demo) vm.OutputDir = @"D:\Mods";
                vm.SelectForTest(templateId, gameName);
                string temp = null;
                if (show == "source") vm.ShowSourceForTest();
                if (show == "done")
                {
                    // Really create the project, through the same command the button uses, into a throwaway folder.
                    temp = Path.Combine(Path.GetTempPath(), "modforge-shot-" + Guid.NewGuid().ToString("N"));
                    vm.OutputDir = temp;
                    vm.CreateFolder.Execute(null);
                    Console.WriteLine("status: " + vm.Status);
                    Console.WriteLine("files on disk: " + (Directory.Exists(temp) ? Directory.GetFiles(temp, "*", SearchOption.AllDirectories).Length : 0));
                }
                var window = new MainWindow(vm);
                var root = (FrameworkElement)window.Content;
                const int w = 1500, h = 900;
                root.Measure(new Size(w, h));
                root.Arrange(new Rect(0, 0, w, h));
                for (int i = 0; i < 3; i++)
                {
                    window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
                    root.UpdateLayout();
                }

                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                    dc.DrawRectangle(window.Background, null, new Rect(0, 0, w, h));
                var bmp = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(visual);
                bmp.Render(root);

                var enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bmp));
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(png)));
                using (var fs = File.Create(png)) enc.Save(fs);
                if (temp != null && Directory.Exists(temp)) Directory.Delete(temp, true);
                Console.WriteLine("wrote " + png);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("screenshot failed: " + ex);
                return 2;
            }
        }
    }
}
