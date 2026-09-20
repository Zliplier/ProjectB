namespace Zlipacket.Core.Tools.Extension
{
    public static class CharExtension
    {
        /// <summary>
        /// Computes the FNV-1a hash for the input char. 
        /// The FNV-1a hash is a non-cryptographic hash function known for its speed and good distribution properties.
        /// Useful for creating Dictionary keys instead of using strings.
        /// https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="char">The input char to hash.</param>
        /// <returns>An integer representing the FNV-1a hash of the input char.</returns>
        public static int ComputeFNV1aHash(this char c) {
            uint hash = 2166136261;
            hash = (hash ^ c) * 16777619;
            return unchecked((int)hash);
        }
    }
}