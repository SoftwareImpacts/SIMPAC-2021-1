using System.Runtime.Serialization;

namespace Iviz.Msgs.tf
{
    [DataContract]
    public sealed class FrameGraph : IService
    {
        /// <summary> Request message. </summary>
        [DataMember] public FrameGraphRequest Request { get; set; }
        
        /// <summary> Response message. </summary>
        [DataMember] public FrameGraphResponse Response { get; set; }
        
        /// <summary> Empty constructor. </summary>
        public FrameGraph()
        {
            Request = new FrameGraphRequest();
            Response = new FrameGraphResponse();
        }
        
        /// <summary> Setter constructor. </summary>
        public FrameGraph(FrameGraphRequest request)
        {
            Request = request;
            Response = new FrameGraphResponse();
        }
        
        IService IService.Create() => new FrameGraph();
        
        IRequest IService.Request
        {
            get => Request;
            set => Request = (FrameGraphRequest)value;
        }
        
        IResponse IService.Response
        {
            get => Response;
            set => Response = (FrameGraphResponse)value;
        }
        
        public string ErrorMessage { get; set; }
        
        string IService.RosType => RosServiceType;
        
        /// <summary> Full ROS name of this service. </summary>
        [Preserve] public const string RosServiceType = "tf/FrameGraph";
        
        /// <summary> MD5 hash of a compact representation of the service. </summary>
        [Preserve] public const string RosMd5Sum = "c4af9ac907e58e906eb0b6e3c58478c0";
    }

    public sealed class FrameGraphRequest : Internal.EmptyRequest
    {
    }

    public sealed class FrameGraphResponse : IResponse
    {
        [DataMember] public string dot_graph { get; set; }
    
        /// <summary> Constructor for empty message. </summary>
        public FrameGraphResponse()
        {
            dot_graph = "";
        }
        
        /// <summary> Explicit constructor. </summary>
        public FrameGraphResponse(string dot_graph)
        {
            this.dot_graph = dot_graph ?? throw new System.ArgumentNullException(nameof(dot_graph));
        }
        
        /// <summary> Constructor with buffer. </summary>
        internal FrameGraphResponse(Buffer b)
        {
            this.dot_graph = b.DeserializeString();
        }
        
        ISerializable ISerializable.Deserialize(Buffer b)
        {
            return new FrameGraphResponse(b ?? throw new System.ArgumentNullException(nameof(b)));
        }
    
        void ISerializable.Serialize(Buffer b)
        {
            if (b is null) throw new System.ArgumentNullException(nameof(b));
            b.Serialize(this.dot_graph);
        }
        
        public void Validate()
        {
            if (dot_graph is null) throw new System.NullReferenceException();
        }
    
        public int RosMessageLength
        {
            get {
                int size = 4;
                size += BuiltIns.UTF8.GetByteCount(dot_graph);
                return size;
            }
        }
    }
}
