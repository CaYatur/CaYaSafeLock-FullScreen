using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using System.IO;

namespace CYSL
{
    public partial class Service1 : ServiceBase
    {
        // Importing necessary functions to handle user sessions
        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSEnumerateSessions(IntPtr hServer, int reserved, int version, ref IntPtr ppSessionInfo, ref int pCount);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSQueryUserToken(uint SessionId, out IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes, int ImpersonationLevel, int TokenType, out IntPtr phNewToken);

        private const int WTS_CURRENT_SERVER_HANDLE = 0;
        private const int TOKEN_QUERY = 0x0008;
        private const int TOKEN_DUPLICATE = 0x0002;
        private const int TOKEN_ASSIGN_PRIMARY = 0x0001;
        private const int STARTF_USESHOWWINDOW = 0x00000001;
        private const int CREATE_NEW_CONSOLE = 0x00000010;

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public int SessionID;
            public string pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        private enum WTS_INFO_CLASS
        {
            WTSUserName = 5,
            WTSDomainName = 7
        }

        private enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Check for an active user session every 10 seconds
            Timer timer = new Timer(CheckActiveSession, null, 0, 10000);
        }

        protected override void OnStop()
        {
            // Stop any running processes if needed (cleanup)
        }

        private void CheckActiveSession(object state)
        {
            IntPtr pSessionInfo = IntPtr.Zero;
            int sessionCount = 0;

            try
            {
                // Get a list of sessions
                if (WTSEnumerateSessions(IntPtr.Zero, 0, 1, ref pSessionInfo, ref sessionCount))
                {
                    int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                    for (int i = 0; i < sessionCount; i++)
                    {
                        IntPtr currentSession = new IntPtr(pSessionInfo.ToInt64() + (i * dataSize));
                        WTS_SESSION_INFO sessionInfo = (WTS_SESSION_INFO)Marshal.PtrToStructure(currentSession, typeof(WTS_SESSION_INFO));

                        // Check if the session is active
                        if (sessionInfo.State == WTS_CONNECTSTATE_CLASS.WTSActive)
                        {
                            // Attempt to start the GUI application in the active session
                            //StartProcessInSession(sessionInfo.SessionID);
                            StartProcessInActiveSession();
                            break;
                        }
                    }
                }
            }
            finally
            {
                if (pSessionInfo != IntPtr.Zero)
                {
                    WTSFreeMemory(pSessionInfo);
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WTSGetActiveConsoleSessionId();

        private void StartProcessInActiveSession()
        {
            uint sessionId = WTSGetActiveConsoleSessionId(); // Aktif oturumu al
            if (sessionId == 0xFFFFFFFF) // Geçerli oturum yoksa
            {
                EventLog.WriteEntry("No active session found.");
                return;
            }

            StartProcessInSession((int)sessionId); // Doğru oturumda başlat
        }


        private void StartProcessInSession(int sessionId)
        {
            IntPtr hToken;
            if (WTSQueryUserToken((uint)sessionId, out hToken))
            {
                IntPtr hDupedToken;
                if (DuplicateTokenEx(hToken, TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_QUERY, IntPtr.Zero, 2, 1, out hDupedToken))
                {
                    // Set up the process start info
                    STARTUPINFO si = new STARTUPINFO();
                    si.cb = Marshal.SizeOf(si);
                    si.lpDesktop = @"winsta0\default"; // Ensure the desktop is set correctly
                    si.dwFlags = STARTF_USESHOWWINDOW;
                    si.wShowWindow = 1;

                    PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

                    // Start the process as the user in the correct session
                    bool result = CreateProcessAsUser(
                        hDupedToken,
                        null,
                        @"C:\Users\cagan\OneDrive\Masaüstü\CaYaChat.exe", // Path to your application
                        IntPtr.Zero,
                        IntPtr.Zero,
                        false,
                        CREATE_NEW_CONSOLE,
                        IntPtr.Zero,
                        null,
                        ref si,
                        out pi
                    );

                    if (!result)
                    {
                        int error = Marshal.GetLastWin32Error();
                        // Log the error or handle it as needed
                        EventLog.WriteEntry($"Failed to start process. Error: {error}");
                    }
                    else
                    {
                        EventLog.WriteEntry("Process started successfully.");
                    }
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    EventLog.WriteEntry($"Failed to duplicate token. Error: {error}");
                }
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                EventLog.WriteEntry($"Failed to query user token. Error: {error}");
            }
        }

    }
}
