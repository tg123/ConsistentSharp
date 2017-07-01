using System.Collections.Generic;
using System.Threading;

namespace ConsistentSharp
{
    public partial class ConsistentHash
    {
        internal Dictionary<uint, string> Circle => _circle;
        internal long Count => _count;
        internal uint[] SortedHashes => _sortedHashes;
    }
}