
namespace Iviz.Msgs.std_msgs
{
    public sealed class Int8MultiArray : IMessage
    {
        // Please look at the MultiArrayLayout message definition for
        // documentation on all multiarrays.
        
        public MultiArrayLayout layout; // specification of data layout
        public sbyte[] data; // array of data
        
    
        /// <summary> Full ROS name of this message. </summary>
        public const string MessageType = "std_msgs/Int8MultiArray";
    
        public IMessage Create() => new Int8MultiArray();
    
        public int GetLength()
        {
            int size = 4;
            size += layout.GetLength();
            size += 1 * data.Length;
            return size;
        }
    
        /// <summary> Constructor for empty message. </summary>
        public Int8MultiArray()
        {
            layout = new MultiArrayLayout();
            data = System.Array.Empty<0>();
        }
        
        public unsafe void Deserialize(ref byte* ptr, byte* end)
        {
            layout.Deserialize(ref ptr, end);
            BuiltIns.Deserialize(out data, ref ptr, end, 0);
        }
    
        public unsafe void Serialize(ref byte* ptr, byte* end)
        {
            layout.Serialize(ref ptr, end);
            BuiltIns.Serialize(data, ref ptr, end, 0);
        }
    
        /// <summary> MD5 hash of a compact representation of the message. </summary>
        public const string Md5Sum = "d7c1af35a1b4781bbe79e03dd94b7c13";
    
        /// <summary> Base64 of the GZip'd compression of the concatenated dependencies file. </summary>
        public const string DependenciesBase64 =
                "H4sIAAAAAAAACr1UXWvbMBR9N+Q/XJKXNkuzfJTSFvIQKOylhUEHY4RQVOs6VmJbQZKbdb9+R/Jn2tcx" +
                "YbB9P885utKIvmcsLFOm9YGEI5cyPZWZU2tjxPujeNelo5ytFTsmyYkqlFO6oESbaERSx2XOhRPBhkdk" +
                "GeU+Xfh0O42iT8Uoq9/VGpE9cqwSFddFEpLCiToqUoW73WybYL+Ct10jCp2atCgaRKt/vAbR0/O3e7JO" +
                "vuR2Z79+ZDSAED8gW8cbQsWZMGxJ0I4LNiquvFdSQS4LniLrgAsUOArjVFwiqyLo3o88JXpo4lHKMGkj" +
                "2bCkxOic0JoN5do65DtNqijq/zPZ2xKQEe2h2LpVrHHR0egjAwHbqITky0VA8aKTxHJvq45CSlXsiDP2" +
                "2w5QzmMpXKc/yscx5kUbSzbVZSZp/fhz/euZXplORjnHBaASsOf2HIR1RklGBVHIZipANvC88rx6sYky" +
                "nueI8HTCX6jJfnK4pFUAs+lz+OKTX6oWm/l2rM4ti+14D8thG408BWABCGHkhJZXcSogbUY317Pf17cz" +
                "Urk/DCflUhABNpygN+CMdaYN1cEWVU6BPWh3XIS9Dw3QeTPbTjPxirqAO0xZ7VI37FxW/WFoviJ07FkD" +
                "WliXY6AZezQrulvMb2YzootCO64jazFJWdqXUC6Ug9oB+2VdcN5HcFLSpcPO0wJAo571DADe87tF4170" +
                "y9U6DDtfW3DZs7Xlgiyfd9JwwpgkjLe/mbzkRp8mtMcH9C7zYhKm5eD/q47T/3oFPDQTOYg8F5yNWgKc" +
                "luoLou/UG4a+Hd7miNWC+Cuw3p0PgXThDwpuAipx7drLNrFSzSdWX59TB9FfaUVwadsFAAA=";
                
    }
}
