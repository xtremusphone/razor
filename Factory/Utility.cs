namespace Factory
{
    public class Utility
    {
        private static Random __random = new Random();
        private static int shift = 32;
        private static long counter = 0L;
        
        public static long GenerateNextNonce()
        {
            var major = ++counter << shift;
            var minor = (DateTime.UtcNow.Ticks ^ __random.Next()) & (1L << shift - 1);
            return major + minor;
        }
    }
}