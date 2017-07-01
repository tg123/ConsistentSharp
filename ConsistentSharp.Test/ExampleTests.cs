using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsistentSharp.Test
{
    [TestClass]
    public class ExampleTests
    {
        // https://github.com/stathat/consistent/blob/master/example_test.go
        [TestMethod]
        public void TestAdd()
        {
            var c = new ConsistentHash();

            c.Add("cacheA");
            c.Add("cacheB");
            c.Add("cacheC");

            var users = new[] { "user_mcnulty", "user_bunk", "user_omar", "user_bunny", "user_stringer" };

            Dump(users, c);

            c.Add("cacheD");
            c.Add("cacheE");

            Dump(users, c);

            c.Remove("cacheD");
            c.Remove("cacheE");
            Dump(users, c);

            c.Remove("cacheC");
            Dump(users, c);
        }

        private static void Dump(string[] users, ConsistentHash c)
        {
            foreach (var user in users)
            {
                Console.WriteLine(user + "=>" + c.Get(user));
            }

            Console.WriteLine();
        }
    }

}
