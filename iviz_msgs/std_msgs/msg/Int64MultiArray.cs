using System.Runtime.Serialization;

namespace Iviz.Msgs.std_msgs
{
    public sealed class Int64MultiArray : IMessage
    {
        // Please look at the MultiArrayLayout message definition for
        // documentation on all multiarrays.
        
        public MultiArrayLayout layout; // specification of data layout
        public long[] data; // array of data
        
    
        /// <summary> Constructor for empty message. </summary>
        public Int64MultiArray()
        {
            layout = new MultiArrayLayout();
            data = System.Array.Empty<long>();
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
    
        [IgnoreDataMember]
        public int RosMessageLength
        {
            get {
                int size = 4;
                size += layout.RosMessageLength;
                size += 8 * data.Length;
                return size;
            }
        }
    
        public IMessage Create() => new Int64MultiArray();
    
        [IgnoreDataMember]
        public string RosType => RosMessageType;
    
        /// <summary> Full ROS name of this message. </summary>
        [Preserve]
        public const string RosMessageType = "std_msgs/Int64MultiArray";
    
        /// <summary> MD5 hash of a compact representation of the message. </summary>
        [Preserve]
        public const string RosMd5Sum = "54865aa6c65be0448113a2afc6a49270";
    
        /// <summary> Base64 of the GZip'd compression of the concatenated dependencies file. </summary>
        [Preserve]
        public const string RosDependenciesBase64 =
                "H4sIAAAAAAAAE71U32vbMBB+919xJC9tlmb5Ucpa6ENgsJcWBh2MEkJQrXOsxJaCJDfr/vp9kh3bafc4" +
                "ZgyW73R33/fpTkP6XrBwTIUxexKefM70WBVeLa0Vbw/izVSeSnZObJkkZ0orr4ymzNhkSNKkVcnai2jD" +
                "K4qCyhAuQribJMmHZFQ03/oZkjtwqjKVNkkyksKLZleitL+5Xq2pe6KXuvBY6RSWJMn9P36Sx6dvd+S8" +
                "3JRu6z6/5wMVfkCzjjRUSgth2ZGgLWu2Kq29V1JBKweSouhQCyQ4COtVWiGqZuffDjwh+nraj1SWyVjJ" +
                "liVl1pSEymypNC4A8IaU1s3/meZtCkiI8pBr2cp1ctHBmgMDAbukgt6LeUSxMVnmuHdOByGl0lvigsOZ" +
                "u9AuwKJ9Jz7SpymaxVhHLjdVIWn58HP5/EQvTEervGcNqATspTsH4bxVkpFBaHlqCZCNPK8Cr97eTNnA" +
                "c0h4O+Ev1Hg33l/SfQSz6nP4FII3dYnVbD1S55b5erSDZb9OhoECsACEsHJMi6s0F5C2oJvr6a/rL1NS" +
                "ZZiEo/I5iAAbxucVOFNTGEvNZocsx8getDsuwt3FAqi8mq4nhXhBXsAd5Ky2uR90Lqd+MwUXKvasES2s" +
                "ixHQjAKae7qdz26mU6ILbTw3OxsxSTnaVVAupoPaEftlk3DWR3BU0ueDztMCQKGe9QwAvrPb+ck976dr" +
                "dBh0vjbhomdr00VZPp6k5YzRSWjvcC0Fya05jmmHBfSuSj2O3bIP/3XFyX+c/3a2kkAEg9Hwx6jUKyi+" +
                "Va/o+LZzT/PVqBEuv+Zo3m2kizAluAaowoXrLtvAWrIQWK/+UuMP7mz+Q9QFAAA=";
                
    }
}