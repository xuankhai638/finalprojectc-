using System;
using System.IO;
using System.Windows.Forms;

namespace EnglishLearningApp
{
    static class Program
    {
        private static readonly string logFilePath = "application.log";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try
            {
                Log("Ứng dụng đã khởi động. / Application started.");

                string userFilePath = "users.txt";
                if (!File.Exists(userFilePath))
                {
                    File.Create(userFilePath).Close();
                    Log("Tệp users.txt đã được tạo thành công. / users.txt file created successfully.");
                }

                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Log($"Lỗi khởi động: {ex.Message} / Startup error: {ex.Message}");
                MessageBox.Show($"Đã xảy ra lỗi khi khởi động ứng dụng: {ex.Message} / An error occurred while starting the application: {ex.Message}", "Lỗi khởi động / Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Log($"Lỗi luồng giao diện: {e.Exception.Message}\nStack Trace: {e.Exception.StackTrace} / Unhandled UI thread exception: {e.Exception.Message}\nStack Trace: {e.Exception.StackTrace}");
            MessageBox.Show($"Đã xảy ra lỗi không mong muốn: {e.Exception.Message}\nỨng dụng sẽ thoát. / An unexpected error occurred: {e.Exception.Message}\nThe application will now exit.", "Lỗi / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Log($"Lỗi luồng không phải giao diện: {ex?.Message}\nStack Trace: {ex?.StackTrace} / Unhandled non-UI thread exception: {ex?.Message}\nStack Trace: {ex?.StackTrace}");
            MessageBox.Show($"Đã xảy ra lỗi nghiêm trọng: {ex?.Message}\nỨng dụng sẽ thoát. / A critical error occurred: {ex?.Message}\nThe application will now exit.", "Lỗi nghiêm trọng / Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        private static void Log(string message)
        {
            try
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể ghi vào tệp nhật ký: {ex.Message} / Failed to write to log file: {ex.Message}", "Lỗi ghi nhật ký / Logging Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}