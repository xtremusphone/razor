namespace Factory
{
    public struct DomainHelper
    {
        public static string GetPCDomainName()
        {
            try
            {
                return System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().Name;
            }
            catch (Exception)
            {
                return Environment.UserDomainName;
            }
        }
    }
}
