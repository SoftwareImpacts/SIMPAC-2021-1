using System.Runtime.Serialization;

namespace Iviz.Msgs.IvizMsgs
{
    [DataContract (Name = "iviz_msgs/Material")]
    public sealed class Material : IMessage
    {
        [DataMember (Name = "name")] public string Name { get; set; }
        [DataMember (Name = "ambient")] public Color Ambient { get; set; }
        [DataMember (Name = "diffuse")] public Color Diffuse { get; set; }
        [DataMember (Name = "emissive")] public Color Emissive { get; set; }
        [DataMember (Name = "diffuseTexture")] public Texture DiffuseTexture { get; set; }
    
        /// <summary> Constructor for empty message. </summary>
        public Material()
        {
            Name = "";
            DiffuseTexture = new Texture();
        }
        
        /// <summary> Explicit constructor. </summary>
        public Material(string Name, in Color Ambient, in Color Diffuse, in Color Emissive, Texture DiffuseTexture)
        {
            this.Name = Name;
            this.Ambient = Ambient;
            this.Diffuse = Diffuse;
            this.Emissive = Emissive;
            this.DiffuseTexture = DiffuseTexture;
        }
        
        /// <summary> Constructor with buffer. </summary>
        internal Material(Buffer b)
        {
            Name = b.DeserializeString();
            Ambient = new Color(b);
            Diffuse = new Color(b);
            Emissive = new Color(b);
            DiffuseTexture = new Texture(b);
        }
        
        public ISerializable RosDeserialize(Buffer b)
        {
            return new Material(b ?? throw new System.ArgumentNullException(nameof(b)));
        }
    
        public void RosSerialize(Buffer b)
        {
            if (b is null) throw new System.ArgumentNullException(nameof(b));
            b.Serialize(Name);
            Ambient.RosSerialize(b);
            Diffuse.RosSerialize(b);
            Emissive.RosSerialize(b);
            DiffuseTexture.RosSerialize(b);
        }
        
        public void RosValidate()
        {
            if (Name is null) throw new System.NullReferenceException();
            if (DiffuseTexture is null) throw new System.NullReferenceException();
            DiffuseTexture.RosValidate();
        }
    
        public int RosMessageLength
        {
            get {
                int size = 16;
                size += BuiltIns.UTF8.GetByteCount(Name);
                size += DiffuseTexture.RosMessageLength;
                return size;
            }
        }
    
        public string RosType => RosMessageType;
    
        /// <summary> Full ROS name of this message. </summary>
        [Preserve] public const string RosMessageType = "iviz_msgs/Material";
    
        /// <summary> MD5 hash of a compact representation of the message. </summary>
        [Preserve] public const string RosMd5Sum = "85fdd2e67e7550e3daaf42d48f8ebd04";
    
        /// <summary> Base64 of the GZip'd compression of the concatenated dependencies file. </summary>
        [Preserve] public const string RosDependenciesBase64 =
                "H4sIAAAAAAAAEysuKcrMS1fIS8xN5XLOz8kvUkjMTcpMzSuB8lIy09JKi2FyqbmZxcWZZalcIakVJaVF" +
                "qTBpKJeLy5bKgMs32N1KIbMssyo+tzi9WB/sDK7SzLwSCwUYnQ6lk6B0Iu2dAfMvyEJjI4XyzJSSDBgn" +
                "IzUzPaMExksqKIA4KzpWISWxBOg2ACcIsstyAQAA";
                
    }
}