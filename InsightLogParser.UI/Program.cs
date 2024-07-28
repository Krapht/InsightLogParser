namespace InsightLogParser.UI {
    internal static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Subscribe to unhandled exception events
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.Run(new Main());
        }

        // Handle UI thread exceptions
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e) {
            ShowException(e.Exception);
        }

        // Handle non-UI thread exceptions
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            ShowException(e.ExceptionObject as Exception);
        }

        // Show exception message
        static void ShowException(Exception ex) {
            if (ex != null) {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}