using System.Runtime.Serialization;

namespace Iviz.Msgs.sensor_msgs
{
    public sealed class MagneticField : IMessage
    {
        // Measurement of the Magnetic Field vector at a specific location.
        
        // If the covariance of the measurement is known, it should be filled in
        // (if all you know is the variance of each measurement, e.g. from the datasheet,
        //just put those along the diagonal)
        // A covariance matrix of all zeros will be interpreted as "covariance unknown",
        // and to use the data a covariance will have to be assumed or gotten from some
        // other source
        
        
        public std_msgs.Header header; // timestamp is the time the
        // field was measured
        // frame_id is the location and orientation
        // of the field measurement
        
        public geometry_msgs.Vector3 magnetic_field; // x, y, and z components of the
        // field vector in Tesla
        // If your sensor does not output 3 axes,
        // put NaNs in the components not reported.
        
        public double[/*9*/] magnetic_field_covariance; // Row major about x, y, z axes
        // 0 is interpreted as variance unknown
    
        /// <summary> Constructor for empty message. </summary>
        public MagneticField()
        {
            header = new std_msgs.Header();
            magnetic_field_covariance = new double[9];
        }
        
        public unsafe void Deserialize(ref byte* ptr, byte* end)
        {
            header.Deserialize(ref ptr, end);
            magnetic_field.Deserialize(ref ptr, end);
            BuiltIns.Deserialize(out magnetic_field_covariance, ref ptr, end, 9);
        }
    
        public unsafe void Serialize(ref byte* ptr, byte* end)
        {
            header.Serialize(ref ptr, end);
            magnetic_field.Serialize(ref ptr, end);
            BuiltIns.Serialize(magnetic_field_covariance, ref ptr, end, 9);
        }
    
        [IgnoreDataMember]
        public int RosMessageLength
        {
            get {
                int size = 96;
                size += header.RosMessageLength;
                return size;
            }
        }
    
        public IMessage Create() => new MagneticField();
    
        [IgnoreDataMember]
        public string RosType => RosMessageType;
    
        /// <summary> Full ROS name of this message. </summary>
        [Preserve]
        public const string RosMessageType = "sensor_msgs/MagneticField";
    
        /// <summary> MD5 hash of a compact representation of the message. </summary>
        [Preserve]
        public const string RosMd5Sum = "2f3b0b43eed0c9501de0fa3ff89a45aa";
    
        /// <summary> Base64 of the GZip'd compression of the concatenated dependencies file. </summary>
        [Preserve]
        public const string RosDependenciesBase64 =
                "H4sIAAAAAAAAE7VVXYvbRhR916+4RA/ZDVq3ZEugC30ohLT7sCE0S19KMdfSlTSJNKPOjGxrf33PHVle" +
                "sTTgQmsMtuV7z/2Yc85QTg/CYfTSi43kaoqt0AM3VqIp6YORrqK9lNF54khMYZDS1PircyVH4+wmyyin" +
                "+zmxdHv2hm0pC1S/QjeBvlp3sAWZSKF1I7B3QrXpOqnIWAW6MjVx19HkxhSsSYqzxhUu2zVwQbJpNlR7" +
                "16fYiiOHViQWQPwyhkjDGPGPCwJsZ5s5ynDjLHfXWvbndes9R2+OWkk7eRLvAh3QpDZrbBQ/eInomAO9" +
                "WqWNNk33SqsS24qioxEll5awvVV0Amx5LxoGYA5h7AGKRTcuRrHzPMH1ongOKB6/Rl9KhpX/KlzhQTt/" +
                "fOOVUzS9hMj9sOxRH+iX7Fs5LxHqRIEDZj1tvLo81XMvW1MttRfKpOU4b3B06ffFgCdOzS2tCIB9NIJF" +
                "RT9t+9CE735PjL3FSc5E3s4pOR0LmopU/wmH0Q/OIj2cgP/lTk6yMJYeJXR8cTa0AnbjMMUG5FdOAlkH" +
                "8Y1ReXpLfJRQXAynOR/5Y9BGZg2ex1JUL4PzYKvqtO4cx3c//PHjny82s10RM6ffILuev6jkd2jqtLSn" +
                "1NfFbX2vp/5CLS+lkv30H7+yh8+/3FGI1UyDWSRZTp8jjpy9kiZy0mKN6VrTQFQ3neylo6QStJn+jdMg" +
                "YYPEx1anCKCXFQ83mFTRSdlYcz9aA0bLs8qWfGTiNJgG9tjx2LFHvPOVsRqedKHoeAf5axRdyf37O8TY" +
                "IOUYDRqagFB6cNzAr+7fUzZimbdvNSHLHw/uRnfbQPvPEo8tJ5OVI1YeQtr5HWq8mYfbABvLEVSpAl2l" +
                "Z1v8DNeEImgBTIGxXqHzTxPc0j4b764TBS45OfVrTXp9vUK2CdqydQv8jPhc4xJYe8bVmW5anFmn04ex" +
                "wQIROHi3NxVCd9NM9U49hDqz8+ynLHlbKpnlH3THCEJWOhF8wmFdaVi5eDCxzQJcHuiLS2X/Exv/0ZkW" +
                "akGdOKokVl4ZSu0FkwxcykZJcp+O1VmQAq6HicG/cyYSK+ORqpcxUMULyC3pkl28BRg9f5VkOenC4WEA" +
                "GIju2cK7kivjMVKu9C4t6NDiBkpRuqPE6KQB3PzeNPD0lIlC/TmZ6TRcQbF+O19wqee5mBpsTt7Nnn+9" +
                "OfkgrhbMkAxxll66DZe+EkWic8Vyk6Y+1gv95CAErCUEbvR6DhGih9mdvI6O52/T+dtT9jfL5/4J+AgA" +
                "AA==";
                
    }
}