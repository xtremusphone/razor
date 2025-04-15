using System.Runtime.InteropServices;
using System.Security;

namespace Factory
{
    public class WinAuth:IDisposable
    {
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_INTERACTIVE = 2;
        private bool disposedValue;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
            int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        public bool Auth(string username, string domain, SecureString password)
        {
            IntPtr tokenHandle;
            bool returnValue = LogonUser(username, domain, password.ToCString(),
                LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out tokenHandle);

            if (returnValue)
            {
                // Authentication successful, use the tokenHandle as needed
                // ...

                // Close the token handle when done
                CloseHandle(tokenHandle);
                return true;
            }
            else
            {
                // Authentication failed
                int errorCode = Marshal.GetLastWin32Error();
                Console.WriteLine("LogonUser failed with error code: " + errorCode);
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WinAuth()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
