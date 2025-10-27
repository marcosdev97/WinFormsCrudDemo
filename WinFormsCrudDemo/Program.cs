using System;
using System.Windows.Forms;
using WinFormsCrudDemo.Utils;

namespace WinFormsCrudDemo
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.ThreadException += (s, e) =>
            {
                Logger.Log(e.Exception, "UI ThreadException");
                ShowFriendly(e.Exception);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    Logger.Log(ex, "AppDomain UnhandledException");
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void ShowFriendly(Exception ex)
        {
            var path = Logger.GetLogPath();
            MessageBox.Show(
                "Ha ocurrido un error inesperado.\n\n" +
                "Se ha guardado un registro en:\n" + path,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
