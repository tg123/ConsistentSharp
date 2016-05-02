using System;
using System.Collections.Generic;
using System.Data.HashFunction.CRCStandards;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsistentSharp
{
    /// from https://github.com/stathat/consistent/blob/master/consistent.go
    public class Consistent
    {
        private readonly Dictionary<uint, string> _circle = new Dictionary<uint, string>();
        private readonly Dictionary<string, bool> _members = new Dictionary<string, bool>();
        private readonly ReaderWriterLockSlim _rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private long _count;

        private uint[] _sortedHashes = new uint[0];

        public int NumberOfReplicas { get; set; } = 20;

        public IEnumerable<string> Members
        {
            get
            {
                _rwlock.EnterReadLock();

                try
                {
                    return _members.Keys.ToArray();
                }
                finally
                {
                    _rwlock.ExitReadLock();
                }
            }
        }


        public void Add(string elt)
        {
            _rwlock.EnterWriteLock();

            try
            {
                _Add(elt);
            }
            finally
            {
                _rwlock.EnterWriteLock();
            }
        }

        private void _Add(string elt)
        {
            for (var i = 0; i < NumberOfReplicas; i++)
            {
                _circle[HashKey(EltKey(elt, i))] = elt;
            }

            _members[elt] = true;
            UpdateSortedHashes();
            _count++;
        }

        public void Remove(string elt)
        {
            _rwlock.EnterWriteLock();
            try
            {
                _Remove(elt);
            }
            finally
            {
                _rwlock.EnterWriteLock();
            }
        }

        private void _Remove(string elt)
        {
            for (var i = 0; i < NumberOfReplicas; i++)
            {
                _circle.Remove(HashKey(EltKey(elt, i)));
            }

            _members.Remove(elt);
            UpdateSortedHashes();
            _count--;
        }

        public void Set(string[] elts)
        {
            _rwlock.EnterWriteLock();
            try
            {
                foreach (var k in _members.Keys)
                {
                    var found = elts.Any(v => k == v);

                    if (!found)
                    {
                        _Remove(k);
                    }
                }

                foreach (var v in elts)
                {
                    if (_members.ContainsKey(v))
                    {
                        continue;
                    }

                    _Add(v);
                }
            }

            finally
            {
                _rwlock.ExitWriteLock();
            }
        }

        public string Get(string name)
        {
            _rwlock.EnterReadLock();

            try
            {
                if (_count == 0)
                {
                    throw new EmptyCircleException();
                }

                var key = HashKey(name);

                var i = Search(key);

                return _circle[_sortedHashes[i]];
            }
            finally
            {
                _rwlock.ExitReadLock();
            }
        }

        // not copied
        // GetTwo
        // GetN

        private int Search(uint key)
        {
            var i = BinarySearch(_sortedHashes.Length, x => _sortedHashes[x] > key);

            if (i >= _sortedHashes.Length)
            {
                i = 0;
            }

            return i;
        }

        /// Search uses binary search to find and return the smallest index i in [0, n) at which f(i) is true
        /// golang sort.Search
        private static int BinarySearch(int n, Func<int, bool> f)
        {
            var s = 0;
            var e = n;

            while (s < e)
            {
                var m = s + (e - s)/2;

                if (!f(m))
                {
                    s = m + 1;
                }
                else
                {
                    e = m;
                }
            }

            return s;
        }

        private void UpdateSortedHashes()
        {
            var hashes = _circle.Keys.ToArray();
            Array.Sort(hashes);
            _sortedHashes = hashes;
        }

        private static string EltKey(string elt, int idx)
        {
            return $"{idx}{elt}";
        }

        protected virtual uint HashKey(string eltKey)
        {
            //var hash = new MurmurHash3();
            var hash = new CRC32();
            var v = hash.ComputeHash(Encoding.UTF8.GetBytes(eltKey));
            return BitConverter.ToUInt32(v, 0);
        }
    }

    public class EmptyCircleException : Exception
    {
    }
}