using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsistentSharp.Test
{
    // https://github.com/stathat/consistent/blob/master/consistent_test.go
    [TestClass]
    public class ConsistentHashTests
    {
        private static bool IsSorted<T>(T[] arr) where T : IComparable
        {
            for (var i = 1; i < arr.Length; i++)
            {
                if (arr[i - 1].CompareTo(arr[i]) > 0)
                {
                    return false;
                }
            }
            return true;
        }

        [TestMethod]
        public void TestNew()
        {
            var x = new ConsistentHash();
            Assert.AreEqual(20, x.NumberOfReplicas);
        }

        [TestMethod]
        public void TestAdd()
        {
            var x = new ConsistentHash();
            x.Add("abcdefg");

            Assert.AreEqual(20, x.Circle.Count);
            Assert.AreEqual(20, x.SortedHashes.Length);

            Assert.IsTrue(IsSorted(x.SortedHashes));

            x.Add("qwer");

            Assert.AreEqual(40, x.Circle.Count);
            Assert.AreEqual(40, x.SortedHashes.Length);

            Assert.IsTrue(IsSorted(x.SortedHashes));
        }

        [TestMethod]
        public void TestRemove()
        {
            var x = new ConsistentHash();
            x.Add("abcdefg");
            x.Remove("abcdefg");

            Assert.AreEqual(0, x.Circle.Count);
            Assert.AreEqual(0, x.SortedHashes.Length);
        }

        [TestMethod]
        public void TestRemoveNonExisting()
        {
            var x = new ConsistentHash();
            x.Add("abcdefg");
            x.Remove("abcdefghijk");
            Assert.AreEqual(20, x.Circle.Count);
        }

        [TestMethod]
        public void TestGetEmpty()
        {
            var x = new ConsistentHash();

            try
            {
                x.Get("asdfsadfsadf");
            }
            catch (EmptyCircleException)
            {
                return;
            }

            Assert.Fail("expected error");
        }


        [TestMethod]
        public void TestGetSingle()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");

            Quick.Check<string>(s =>
            {
                var y = x.Get(s);
                return y == "abcdefg";
            });
        }

        [TestMethod]
        public void TestGetMultiple()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            foreach (var gmtest in new[]
            {
                // new[] { in, out }
                new[] {"ggg", "abcdefg"},
                new[] {"hhh", "opqrstu"},
                new[] {"iiiii", "hijklmn"}
            })
            {
                Assert.AreEqual(gmtest[1], x.Get(gmtest[0]));
            }
        }

        [TestMethod]
        public void TestGetMultipleQuick()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            Quick.Check<string>(s =>
            {
                var y = x.Get(s);
                return y == "abcdefg" || y == "hijklmn" || y == "opqrstu";
            });
        }

        [TestMethod]
        public void TestGetMultipleRemove()
        {
            var x = new ConsistentHash();
            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            foreach (var gmtest in new[]
            {
                // new[] { in, out }
                new[] {"ggg", "abcdefg"},
                new[] {"hhh", "opqrstu"},
                new[] {"iiiii", "hijklmn"}
            })
            {
                Assert.AreEqual(gmtest[1], x.Get(gmtest[0]));
            }

            x.Remove("hijklmn");

            foreach (var gmtest in new[]
            {
                // new[] { in, out }
                new[] {"ggg", "abcdefg"},
                new[] {"hhh", "opqrstu"},
                new[] {"iiiii", "opqrstu"}
            })
            {
                Assert.AreEqual(gmtest[1], x.Get(gmtest[0]));
            }
        }

        [TestMethod]
        public void TestGetMultipleRemoveQuick()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            x.Remove("opqrstu");

            Quick.Check<string>(s =>
            {
                var y = x.Get(s);
                return y == "abcdefg" || y == "hijklmn";
            });
        }

        [TestMethod]
        public void TestGetTwo()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            var (a, b) = x.GetTwo("99999999");

            Assert.AreNotEqual(a, b);
            Assert.AreEqual("abcdefg", a);
            Assert.AreEqual("hijklmn", b);
        }

        [TestMethod]
        public void TestGetTwoQuick()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            Quick.Check<string>(s =>
            {
                var (a, b) = x.GetTwo(s);

                if (a == b)
                {
                    return false;
                }

                if (a != "abcdefg" && a != "hijklmn" && a != "opqrstu")
                {
                    return false;
                }

                if (b != "abcdefg" && b != "hijklmn" && b != "opqrstu")
                {
                    return false;
                }

                return true;
            });
        }

        [TestMethod]
        public void TestGetTwoOnlyTwoQuick()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");

            Quick.Check<string>(s =>
            {
                var (a, b) = x.GetTwo(s);

                if (a == b)
                {
                    return false;
                }

                if (a != "abcdefg" && a != "hijklmn")
                {
                    return false;
                }

                if (b != "abcdefg" && b != "hijklmn")
                {
                    return false;
                }

                return true;
            });
        }

        [TestMethod]
        public void TestGetTwoOnlyOneInCircle()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");

            var (a, b) = x.GetTwo("99999999");
            Assert.AreNotEqual(a, b);

            Assert.AreEqual("abcdefg", a);
            Assert.AreEqual(default(string), b);
        }


        [TestMethod]
        public void TestGetN()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            var members = x.GetN("9999999", 3).ToArray();

            Assert.AreEqual(3, members.Length);
            Assert.AreEqual("opqrstu", members[0]);
            Assert.AreEqual("abcdefg", members[1]);
            Assert.AreEqual("hijklmn", members[2]);
        }

        [TestMethod]
        public void TestGetNLess()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            var members = x.GetN("99999999", 2).ToArray();

            Assert.AreEqual(2, members.Length);
            Assert.AreEqual("abcdefg", members[0]);
            Assert.AreEqual("hijklmn", members[1]);
        }

        [TestMethod]
        public void TestGetNMore()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            var members = x.GetN("9999999", 5).ToArray();

            Assert.AreEqual(3, members.Length);
            Assert.AreEqual("opqrstu", members[0]);
            Assert.AreEqual("abcdefg", members[1]);
            Assert.AreEqual("hijklmn", members[2]);
        }

        [TestMethod]
        public void TestGetNQuick()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            Quick.Check<string>(s =>
            {
                var members = x.GetN(s, 3).ToArray();

                if (members.Length != 3)
                {
                    return false;
                }

                var set = new HashSet<string>();

                foreach (var member in members)
                {
                    if (set.Contains(member))
                    {
                        return false;
                    }

                    set.Add(member);

                    if (member != "abcdefg" && member != "hijklmn" && member != "opqrstu")
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        [TestMethod]
        public void TestGetNLessQuick()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            Quick.Check<string>(s =>
            {
                var members = x.GetN(s, 2).ToArray();

                if (members.Length != 2)
                {
                    return false;
                }

                var set = new HashSet<string>();

                foreach (var member in members)
                {
                    if (set.Contains(member))
                    {
                        return false;
                    }

                    set.Add(member);

                    if (member != "abcdefg" && member != "hijklmn" && member != "opqrstu")
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        [TestMethod]
        public void TestGetNMoreQuick()
        {
            var x = new ConsistentHash();

            x.Add("abcdefg");
            x.Add("hijklmn");
            x.Add("opqrstu");

            Quick.Check<string>(s =>
            {
                var members = x.GetN(s, 5).ToArray();

                if (members.Length != 3)
                {
                    return false;
                }

                var set = new HashSet<string>();

                foreach (var member in members)
                {
                    if (set.Contains(member))
                    {
                        return false;
                    }

                    set.Add(member);

                    if (member != "abcdefg" && member != "hijklmn" && member != "opqrstu")
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        [TestMethod]
        public void TestSet()
        {
            var x = new ConsistentHash();

            x.Add("abc");
            x.Add("def");
            x.Add("ghi");

            {
                x.Set(new[] {"jkl", "mno"});

                Assert.AreEqual(2, x.Count);

                var (a, b) = x.GetTwo("qwerqwerwqer");
                Assert.IsTrue(a == "jkl" || a == "mno");
                Assert.IsTrue(b == "jkl" || b == "mno");
                Assert.AreNotEqual(a, b);
            }

            {
                x.Set(new[] {"pqr", "mno"});

                Assert.AreEqual(2, x.Count);

                var (a, b) = x.GetTwo("qwerqwerwqer");
                Assert.IsTrue(a == "pqr" || a == "mno");
                Assert.IsTrue(b == "pqr" || b == "mno");
                Assert.AreNotEqual(a, b);
            }
        }

        [TestMethod]
        public void TestAddCollision()
        {
            // These two strings produce several crc32 collisions after "|i" is
            // appended added by Consistent.eltKey.
            const string s1 = "abear";
            const string s2 = "solidiform";

            var x = new ConsistentHash();
            x.Add(s1);
            x.Add(s2);

            var elt1 = x.Get(s1);

            var y = new ConsistentHash();
            y.Add(s2);
            y.Add(s1);

            var elt2 = y.Get(s1);

            Assert.AreEqual(elt1, elt2);
        }

        [TestMethod]
        public void TestConcurrentGetSet()
        {
            var x = new ConsistentHash();
            x.Set(new[] {"abc", "def", "ghi", "jkl", "mno"});

            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (var j = 0; j < 1000; j++)
                    {
                        x.Set(new[] {"abc", "def", "ghi", "jkl", "mno"});
                        await Task.Delay(TimeSpan.FromMilliseconds(Rand.Intn(10)));

                        x.Set(new[] {"pqr", "stu", "vwx"});
                        await Task.Delay(TimeSpan.FromMilliseconds(Rand.Intn(10)));
                    }
                }));
            }

            for (var i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (var j = 0; j < 1000; j++)
                    {
                        var a = x.Get("xxxxxxx");

                        Assert.IsTrue(a == "def" || a == "vwx");

                        await Task.Delay(TimeSpan.FromMilliseconds(Rand.Intn(10)));
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}