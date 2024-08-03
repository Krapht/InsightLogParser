namespace InsightLogParser.UI {
    internal static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Subscribe to unhandled exception events
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // Parse command-line arguments to get the port number
            int port = ParsePortArgument(args);

            Application.Run(new Main(port));
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

        // Parse the port argument from the command-line arguments
        static int ParsePortArgument(string[] args) {
            for (int i = 0; i < args.Length; i++) {
                if (args[i] == "-port" && i + 1 < args.Length) {
                    if (int.TryParse(args[i + 1], out int port)) {
                        return port;
                    }
                }
            }
            throw new ArgumentException("Port number not specified or invalid.");
        }

    }
}