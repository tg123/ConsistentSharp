namespace ConsistentSharp
{
    public interface IHashAlgorithm
    {
        uint HashKey(string key);
    }
}