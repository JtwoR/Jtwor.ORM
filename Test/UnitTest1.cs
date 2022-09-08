using Jtwor.ORM;
using NUnit.Framework;
using System.Diagnostics;

namespace Test
{
    public class Tests
    {
        //[SetUp]
        //public void Setup()
        //{
        //}

        [Test]
        public void Test1()
        {
            DbSet<User> test = new DbSet<User>();
            test.Where(w => (w.name == "123" || w.name == "654") || (w.sex == "321" && (w.sex == "456" || w.birthday == "789")))
                .GroupBy(g => new object[] { g.name, g.sex })//.Select(s=>new { nnn=s.name});
                .Select(s => new 
                {
                    NameStr = s.name,
                    SexStr = s.sex
                });

            Debug.WriteLine(test.CheckSql());
        }
    }


    public class User
    {
        public string name { get; set; }

        public string sex { get; set; }

        public string birthday { get; set; }
    }
}