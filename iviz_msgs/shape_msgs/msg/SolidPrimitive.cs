using System.Runtime.Serialization;

namespace Iviz.Msgs.shape_msgs
{
    public sealed class SolidPrimitive : IMessage
    {
        // Define box, sphere, cylinder, cone 
        // All shapes are defined to have their bounding boxes centered around 0,0,0.
        
        public const byte BOX = 1;
        public const byte SPHERE = 2;
        public const byte CYLINDER = 3;
        public const byte CONE = 4;
        
        // The type of the shape
        public byte type;
        
        
        // The dimensions of the shape
        public double[] dimensions;
        
        // The meaning of the shape dimensions: each constant defines the index in the 'dimensions' array
        
        // For the BOX type, the X, Y, and Z dimensions are the length of the corresponding
        // sides of the box.
        public const byte BOX_X = 0;
        public const byte BOX_Y = 1;
        public const byte BOX_Z = 2;
        
        
        // For the SPHERE type, only one component is used, and it gives the radius of
        // the sphere.
        public const byte SPHERE_RADIUS = 0;
        
        
        // For the CYLINDER and CONE types, the center line is oriented along
        // the Z axis.  Therefore the CYLINDER_HEIGHT (CONE_HEIGHT) component
        // of dimensions gives the height of the cylinder (cone).  The
        // CYLINDER_RADIUS (CONE_RADIUS) component of dimensions gives the
        // radius of the base of the cylinder (cone).  Cone and cylinder
        // primitives are defined to be circular. The tip of the cone is
        // pointing up, along +Z axis.
        
        public const byte CYLINDER_HEIGHT = 0;
        public const byte CYLINDER_RADIUS = 1;
        
        public const byte CONE_HEIGHT = 0;
        public const byte CONE_RADIUS = 1;
    
        /// <summary> Constructor for empty message. </summary>
        public SolidPrimitive()
        {
            dimensions = System.Array.Empty<double>();
        }
        
        public unsafe void Deserialize(ref byte* ptr, byte* end)
        {
            BuiltIns.Deserialize(out type, ref ptr, end);
            BuiltIns.Deserialize(out dimensions, ref ptr, end, 0);
        }
    
        public unsafe void Serialize(ref byte* ptr, byte* end)
        {
            BuiltIns.Serialize(type, ref ptr, end);
            BuiltIns.Serialize(dimensions, ref ptr, end, 0);
        }
    
        [IgnoreDataMember]
        public int RosMessageLength
        {
            get {
                int size = 5;
                size += 8 * dimensions.Length;
                return size;
            }
        }
    
        public IMessage Create() => new SolidPrimitive();
    
        [IgnoreDataMember]
        public string RosType => RosMessageType;
    
        /// <summary> Full ROS name of this message. </summary>
        [Preserve]
        public const string RosMessageType = "shape_msgs/SolidPrimitive";
    
        /// <summary> MD5 hash of a compact representation of the message. </summary>
        [Preserve]
        public const string RosMd5Sum = "d8f8cbc74c5ff283fca29569ccefb45d";
    
        /// <summary> Base64 of the GZip'd compression of the concatenated dependencies file. </summary>
        [Preserve]
        public const string RosDependenciesBase64 =
                "H4sIAAAAAAAAE3WTbWsbMQzH39+nEPRFW3Yc3VZGGeRF29yWwGhH2kGSUYJ7p+QMF/uwfSX59pNs30Mz" +
                "SiCcbEl/6Sf5DKa4lQrhVR9SsE2FBlMojrVUJRr60nSXnMFtXYOtRIMWhEEofVAJTkMl3hBchdJQjlaV" +
                "Uu04GTkWqBylKymCL+AqpV+WJK1U7gbuHpeTz/H76fcsX+STL9G8X/2aP0zzxeRrd/D4kE+uE6rjuSKx" +
                "Y4OgtywaaopefJ50TqXco7JSK/vedVtr4b5d/30ZeXQxexSKyx8HjNy+A4qiYiTWCeUiBOt9GdeB/r1x" +
                "PsScU/NGHFnhhzb+lhr3pabeWqawSkEQnvW4ZobM1zWqnau6igptDNpGe8qU0soS+/4Iejaw3SwnVyNr" +
                "1bNma02oxyUF/rEqreoj8NgLvScpGiJIC63FMtQpHezkW+zbiFK2XAJl88z8BmXv5rpZ3E7nf56onrFm" +
                "N2Sfkwfs5W2gElYHat5MEtdG8gFtUq194+yzBnGQNgMencGtjsS6vJtZPv85e4YLzh2Ny6EnSkLcRsSH" +
                "nmiXd5Xrmce3ABf8Fi6DHkX3OqG7qBOMkc5HKpShZxfGJyx+rHnPA2FS3RXFN0bupfMJT97kK6WQpmhr" +
                "YbLwZGQz7JBnyvGahsT73jZpIAufItTk5CVGfv1KnTRPyzV6qf85D2DY8R/CXFx5cwQAAA==";
                
    }
}