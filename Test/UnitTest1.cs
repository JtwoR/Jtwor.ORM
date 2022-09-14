using Jtwor.ORM;
using Newtonsoft.Json;
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
            test.Where(w => (w.name == "≤‚ ‘"));

            var result = test.ToList();

            Debug.WriteLine(JsonConvert.SerializeObject(result));
        }
    }

    public class User
    {
        public string name { get; set; }

        public string sex { get; set; }

        public string birthday { get; set; }
    }
}