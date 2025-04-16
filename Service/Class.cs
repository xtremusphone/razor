using System.Data;

namespace Razor01.Service
{
    public class Class1
    {
        private readonly IDatabaseService _dbcon;

        public Class1(IDatabaseService dbcon)
        {
            _dbcon = dbcon;
        }

        public void test()
        {
            
        }
    }
}
