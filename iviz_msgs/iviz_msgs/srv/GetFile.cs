using System.Runtime.Serialization;

namespace Iviz.Msgs.IvizMsgs
{
    [DataContract (Name = "iviz_msgs/GetFile")]
    public sealed class GetFile : IService
    {
        /// <summary> Request message. </summary>
        [DataMember] public GetFileRequest Request { get; set; }
        
        /// <summary> Response message. </summary>
        [DataMember] public GetFileResponse Response { get; set; }
        
        /// <summary> Empty constructor. </summary>
        public GetFile()
        {
            Request = new GetFileRequest();
            Response = new GetFileResponse();
        }
        
        /// <summary> Setter constructor. </summary>
        public GetFile(GetFileRequest request)
        {
            Request = request;
            Response = new GetFileResponse();
        }
        
        IService IService.Create() => new GetFile();
        
        IRequest IService.Request
        {
            get => Request;
            set => Request = (GetFileRequest)value;
        }
        
        IResponse IService.Response
        {
            get => Response;
            set => Response = (GetFileResponse)value;
        }
        
        /// <summary>
        /// An error message in case the call fails.
        /// If the provider sets this to non-null, the ok byte is set to false, and the error message is sent instead of the response.
        /// </summary>
        public string ErrorMessage { get; set; }
        
        string IService.RosType => RosServiceType;
        
        /// <summary> Full ROS name of this service. </summary>
        [Preserve] public const string RosServiceType = "iviz_msgs/GetFile";
        
        /// <summary> MD5 hash of a compact representation of the service. </summary>
        [Preserve] public const string RosMd5Sum = "08088c7144705ee9cf37b287c931476d";
    }

    public sealed class GetFileRequest : IRequest
    {
        // Retrieves a file
        [DataMember (Name = "uri")] public string Uri { get; set; } // Uri of the file. Example: package://some_package/file.dae
    
        /// <summary> Constructor for empty message. </summary>
        public GetFileRequest()
        {
            Uri = "";
        }
        
        /// <summary> Explicit constructor. </summary>
        public GetFileRequest(string Uri)
        {
            this.Uri = Uri;
        }
        
        /// <summary> Constructor with buffer. </summary>
        internal GetFileRequest(Buffer b)
        {
            Uri = b.DeserializeString();
        }
        
        public ISerializable RosDeserialize(Buffer b)
        {
            return new GetFileRequest(b ?? throw new System.ArgumentNullException(nameof(b)));
        }
    
        public void RosSerialize(Buffer b)
        {
            if (b is null) throw new System.ArgumentNullException(nameof(b));
            b.Serialize(Uri);
        }
        
        public void RosValidate()
        {
            if (Uri is null) throw new System.NullReferenceException(nameof(Uri));
        }
    
        public int RosMessageLength
        {
            get {
                int size = 4;
                size += BuiltIns.UTF8.GetByteCount(Uri);
                return size;
            }
        }
    }

    public sealed class GetFileResponse : IResponse
    {
        [DataMember (Name = "success")] public bool Success { get; set; } // Whether the retrieval succeeded
        [DataMember (Name = "bytes")] public byte[] Bytes { get; set; } // The content of the file as byte array
        [DataMember (Name = "message")] public string Message { get; set; } // An error message if success is false
    
        /// <summary> Constructor for empty message. </summary>
        public GetFileResponse()
        {
            Bytes = System.Array.Empty<byte>();
            Message = "";
        }
        
        /// <summary> Explicit constructor. </summary>
        public GetFileResponse(bool Success, byte[] Bytes, string Message)
        {
            this.Success = Success;
            this.Bytes = Bytes;
            this.Message = Message;
        }
        
        /// <summary> Constructor with buffer. </summary>
        internal GetFileResponse(Buffer b)
        {
            Success = b.Deserialize<bool>();
            Bytes = b.DeserializeStructArray<byte>();
            Message = b.DeserializeString();
        }
        
        public ISerializable RosDeserialize(Buffer b)
        {
            return new GetFileResponse(b ?? throw new System.ArgumentNullException(nameof(b)));
        }
    
        public void RosSerialize(Buffer b)
        {
            if (b is null) throw new System.ArgumentNullException(nameof(b));
            b.Serialize(Success);
            b.SerializeStructArray(Bytes, 0);
            b.Serialize(Message);
        }
        
        public void RosValidate()
        {
            if (Bytes is null) throw new System.NullReferenceException(nameof(Bytes));
            if (Message is null) throw new System.NullReferenceException(nameof(Message));
        }
    
        public int RosMessageLength
        {
            get {
                int size = 9;
                size += 1 * Bytes.Length;
                size += BuiltIns.UTF8.GetByteCount(Message);
                return size;
            }
        }
    }
}