using FundsManagement.Models;
using LiteDB;
using System.Diagnostics;

namespace TestProject1
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

            Test1();

        }

        [Test]
        public void Test1()
        {
            LiteDatabase db = new LiteDatabase("dd.db");
            var c = db.GetCollection<ICustomer>();

            c.Insert(new NaturalCustomer() { Name = "11", Credential = new Credential { Id = "fdsf" } });
            c.Insert(new ProductCustomer() { Name = "11", Credential = new Credential { Id = "fdsf" } });

            var a = c.FindAll().ToArray();

            Debug.Assert(true);
        }
    }
}
