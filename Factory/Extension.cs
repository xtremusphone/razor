using System.Runtime.InteropServices;
using System.Security;

namespace Factory
{
    public static class Extension
    {
        public static SecureString ToSecureString(this string source)
        {
            SecureString result = new SecureString();
            foreach (char c in source.ToCharArray())
                result.AppendChar(c);
            return result;
        }

        public static string ToCString(this SecureString secureString)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
