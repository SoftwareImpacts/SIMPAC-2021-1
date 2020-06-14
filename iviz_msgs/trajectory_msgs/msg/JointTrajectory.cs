using System.Runtime.Serialization;

namespace Iviz.Msgs.TrajectoryMsgs
{
    [DataContract (Name = "trajectory_msgs/JointTrajectory")]
    public sealed class JointTrajectory : IMessage
    {
        [DataMember (Name = "header")] public StdMsgs.Header Header { get; set; }
        [DataMember (Name = "joint_names")] public string[] JointNames { get; set; }
        [DataMember (Name = "points")] public JointTrajectoryPoint[] Points { get; set; }
    
        /// <summary> Constructor for empty message. </summary>
        public JointTrajectory()
        {
            JointNames = System.Array.Empty<string>();
            Points = System.Array.Empty<JointTrajectoryPoint>();
        }
        
        /// <summary> Explicit constructor. </summary>
        public JointTrajectory(in StdMsgs.Header Header, string[] JointNames, JointTrajectoryPoint[] Points)
        {
            this.Header = Header;
            this.JointNames = JointNames;
            this.Points = Points;
        }
        
        /// <summary> Constructor with buffer. </summary>
        internal JointTrajectory(Buffer b)
        {
            Header = new StdMsgs.Header(b);
            JointNames = b.DeserializeStringArray();
            Points = b.DeserializeArray<JointTrajectoryPoint>();
            for (int i = 0; i < this.Points.Length; i++)
            {
                Points[i] = new JointTrajectoryPoint(b);
            }
        }
        
        public ISerializable RosDeserialize(Buffer b)
        {
            return new JointTrajectory(b ?? throw new System.ArgumentNullException(nameof(b)));
        }
    
        public void RosSerialize(Buffer b)
        {
            if (b is null) throw new System.ArgumentNullException(nameof(b));
            Header.RosSerialize(b);
            b.SerializeArray(JointNames, 0);
            b.SerializeArray(Points, 0);
        }
        
        public void RosValidate()
        {
            Header.RosValidate();
            if (JointNames is null) throw new System.NullReferenceException();
            for (int i = 0; i < JointNames.Length; i++)
            {
                if (JointNames[i] is null) throw new System.NullReferenceException();
            }
            if (Points is null) throw new System.NullReferenceException();
            for (int i = 0; i < Points.Length; i++)
            {
                if (Points[i] is null) throw new System.NullReferenceException();
                Points[i].RosValidate();
            }
        }
    
        public int RosMessageLength
        {
            get {
                int size = 8;
                size += Header.RosMessageLength;
                size += 4 * JointNames.Length;
                for (int i = 0; i < JointNames.Length; i++)
                {
                    size += BuiltIns.UTF8.GetByteCount(JointNames[i]);
                }
                for (int i = 0; i < Points.Length; i++)
                {
                    size += Points[i].RosMessageLength;
                }
                return size;
            }
        }
    
        public string RosType => RosMessageType;
    
        /// <summary> Full ROS name of this message. </summary>
        [Preserve] public const string RosMessageType = "trajectory_msgs/JointTrajectory";
    
        /// <summary> MD5 hash of a compact representation of the message. </summary>
        [Preserve] public const string RosMd5Sum = "65b4f94a94d1ed67169da35a02f33d3f";
    
        /// <summary> Base64 of the GZip'd compression of the concatenated dependencies file. </summary>
        [Preserve] public const string RosDependenciesBase64 =
                "H4sIAAAAAAAAE7VUTYvbMBC961cM5LC7pUmhLT0Eeij0Y1soLGxuSzATaRJrkSVXkrPrf98neZt4Sw89" +
                "tMYgS/PmzdeTr4WNRGrrolKO1h/utnQfrM+N506S+la+N5HvRecQx5uyBaQva1Lq/T9+1PfbL2tK2TRd" +
                "OqRX11NmC7rN7A1HQ51kNpyZ9gGJ20MrcenkKA5O3PViqFrz2EtawXHT2kR4D+IlsnMjDQmgHEiHrhu8" +
                "1ZyFskWtc394Wk9MPcds9eA4Ah+isb7A9xG9Kex4k/wYxGuhrx/XwPgkesgWCY1g0FE4oakwkhrQsjev" +
                "i4NabB7CEls5oP2n4JRbziVZeeyjpJInpzVivJiKW4EbzRFEMYku61mDbboiBEEK0gfd0iUyvxlzGzwI" +
                "hY4cLe+cFGKNDoD1ojhdXM2YfaX27MMv+onxHONvaP2Jt9S0bDEzV6pPwwENBLCP4WgNoLuxkmhnxWdy" +
                "dhc5jqp4TSHV4nPpMUDwqhPByikFbTEAQw82t0+CnabRWPO/1JhP6p9E+acrgZI/MXp/xk5XhFIv2u6t" +
                "YKhIGdPuQ7LZQid3LwkqQUEZVmxYa3HQaDVut2AMz9Gyh+bztiq/NG8WC3LeYfyPRXtiijI/OHeKbTAs" +
                "NyAFjlJkXZxT6S4EjYw41ZN666ne+gL6rcoVSldq7wLnd2/rD+ApsdnZuZzZ4bOyZudTNcoMk6kqptnH" +
                "0DUQAAzqJ1kk7fqdBAAA";
                
    }
}
