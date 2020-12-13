/* This file was created automatically, do not edit! */

using System.Runtime.Serialization;

namespace Iviz.Msgs.Tf2Msgs
{
    [DataContract (Name = "tf2_msgs/LookupTransformResult")]
    public sealed class LookupTransformResult : IDeserializable<LookupTransformResult>, IResult<LookupTransformActionResult>
    {
        [DataMember (Name = "transform")] public GeometryMsgs.TransformStamped Transform { get; set; }
        [DataMember (Name = "error")] public Tf2Msgs.TF2Error Error { get; set; }
    
        /// <summary> Constructor for empty message. </summary>
        public LookupTransformResult()
        {
            Transform = new GeometryMsgs.TransformStamped();
            Error = new Tf2Msgs.TF2Error();
        }
        
        /// <summary> Explicit constructor. </summary>
        public LookupTransformResult(GeometryMsgs.TransformStamped Transform, Tf2Msgs.TF2Error Error)
        {
            this.Transform = Transform;
            this.Error = Error;
        }
        
        /// <summary> Constructor with buffer. </summary>
        public LookupTransformResult(ref Buffer b)
        {
            Transform = new GeometryMsgs.TransformStamped(ref b);
            Error = new Tf2Msgs.TF2Error(ref b);
        }
        
        public ISerializable RosDeserialize(ref Buffer b)
        {
            return new LookupTransformResult(ref b);
        }
        
        LookupTransformResult IDeserializable<LookupTransformResult>.RosDeserialize(ref Buffer b)
        {
            return new LookupTransformResult(ref b);
        }
    
        public void RosSerialize(ref Buffer b)
        {
            Transform.RosSerialize(ref b);
            Error.RosSerialize(ref b);
        }
        
        public void RosValidate()
        {
            if (Transform is null) throw new System.NullReferenceException(nameof(Transform));
            Transform.RosValidate();
            if (Error is null) throw new System.NullReferenceException(nameof(Error));
            Error.RosValidate();
        }
    
        public int RosMessageLength
        {
            get {
                int size = 0;
                size += Transform.RosMessageLength;
                size += Error.RosMessageLength;
                return size;
            }
        }
    
        public string RosType => RosMessageType;
    
        /// <summary> Full ROS name of this message. </summary>
        [Preserve] public const string RosMessageType = "tf2_msgs/LookupTransformResult";
    
        /// <summary> MD5 hash of a compact representation of the message. </summary>
        [Preserve] public const string RosMd5Sum = "3fe5db6a19ca9cfb675418c5ad875c36";
    
        /// <summary> Base64 of the GZip'd compression of the concatenated dependencies file. </summary>
        [Preserve] public const string RosDependenciesBase64 =
                "H4sIAAAAAAAAE71WXW/bNhR916+4aB6aDIm8JV0xBE2BoHE6YYmdOU6wPRmMdCURkUiVpOJ6v36H1Ifd" +
                "ptj2sCYwYvHyfp9zr1ywrtmZzaq2hZ0sjVA216a+daJuOCM3CCKXH/c6l8dTY7Qh9v+js//5L7q+/XhK" +
                "xT+mFe3RspSW+HNj2Fq2JLaZUm50TanWJpNKOMZZ1Ewli4xNHA4r6V04Ta7k55ppKatstVUcotUIJQom" +
                "/6itqzbUWrToYRPcQOudoNJwfvaqdK45nUzW8lHGRttYm2Li8lfvXf5uIt5TI9JHOIq9zS3DobOU6bSt" +
                "WTnhpFaEOhDD4Er5koIwjqJfQw19KZF1Rqriq3RpL2TTVYKjzrsivVInjcZu7qD7nWC0LusQ7DL39Tqh" +
                "MmEydNOJTDgRai1lUbI5qviJKxh13Au3btOwjQcI8ClYsRHV0H2AmOq6bpVMPYJOAqVde1hKBXo0wjiZ" +
                "tpUwzwD33vGx/KlllTIlF6fQUZbT1kkktIGH1LCwvtvJBUWtVO7k2BtEe8u1PsKRC+AyBkfLhaMdgmYk" +
                "7Cli/NAVF8M3msOIklnaD7IVjvaAEAQpcKPTkvaR+c3GlSCEx/BJGCkeqkDAFB2A19fe6PXBjmcVXCuh" +
                "9OC+87iN8V/cqtGvr+moBGaVr962BRoIxcboJ5lt2Z9WEuSlSj4YYTaRt+pCRnuXgYrOwxcQwbewVqcS" +
                "AGS0lq4cmDyO3MsulYFchj1YKMOGkrYL5YHdmhndWutn5LGeXrnBFFuMNbgU3XPqtDnp7KswutHvLQyM" +
                "8qNtdDfjL1Nkn8w3ShT0FO6+yt9PQhK4qxWYX7MArBiy0RKGmTQw9SsJXhkbD5vqEFsMSwz9UNrBRy0e" +
                "4ZJBJG8tmgbOxG5PvBgm+xwX8SGtS/Q3aHkihLENgy5TMrKQOy+i0VhQX9wh4d0EIlVVl3MXDBDCydDt" +
                "g5iSnDa6pbUvCA+m3y8a8I55hTlwWh/65dK7+LKhNxrTvn0VKOuw2YB6Xmnh3r6hz+PTZnz660Wg3nLs" +
                "W2gr0kaO75cvMPenT1uC+ib/a0HD0/p70fjZz42wdX+h2Xw1XSzmCzqjH3vR1Xz+293NKP6pF3+Yz2bT" +
                "D8vkPln+OV4e95fTP5aL85v51fkymc/G25P+Npndn18lF6vzxce76+lsOSq86RWWyfV0freV/zzIF+ez" +
                "28v54nq8eRv1V92vpX7ThcOqO0TR35ncu6uECQAA";
                
    }
}