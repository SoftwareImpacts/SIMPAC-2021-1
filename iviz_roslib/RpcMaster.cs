﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using TopicTuple = System.Tuple<string, string>;
using TopicTuples = System.Tuple<string, string[]>;

namespace Iviz.Roslib.XmlRpc
{
    public enum StatusCode
    {
        Error = -1,
        Failure = 0,
        Success = 1,
    }

    public sealed class Master
    {
        public Uri MasterUri { get; }
        public Uri CallerUri { get; }
        public string CallerId { get; }
        public int TimeoutInMs { get; set; } = 2000;

        internal Master(Uri masterUri, string callerId, Uri callerUri)
        {
            MasterUri = masterUri;
            CallerUri = callerUri;
            CallerId = callerId;
        }

        public GetUriResponse GetUri()
        {
            Arg[] args = { new Arg(CallerId) };
            object[] response = MethodCall("getUri", args);
            return new GetUriResponse(response);
        }

        public LookupNodeResponse LookupNode(string nodeId)
        {
            Arg[] args = {
                new Arg(CallerId),
                new Arg(nodeId),
            };
            object[] response = MethodCall("lookupNode", args);
            return new LookupNodeResponse(response);
        }

        public GetPublishedTopicsResponse GetPublishedTopics(string subgraph = "")
        {
            Arg[] args = {
                new Arg(CallerId),
                new Arg(subgraph),
            };
            object[] response = MethodCall("getPublishedTopics", args);
            return new GetPublishedTopicsResponse(response);
        }

        public RegisterSubscriberResponse RegisterSubscriber(string topic, string topicType)
        {
            Arg[] args = {
                new Arg(CallerId),
                new Arg(topic),
                new Arg(topicType),
                new Arg(CallerUri.ToString()),
            };
            object[] response = MethodCall("registerSubscriber", args);
            return new RegisterSubscriberResponse(response);
        }

        public UnregisterSubscriberResponse UnregisterSubscriber(string topic)
        {
            Arg[] args = {
                new Arg(CallerId),
                new Arg(topic),
                new Arg(CallerUri.ToString()),
            };
            object[] response = MethodCall("unregisterSubscriber", args);
            return new UnregisterSubscriberResponse(response);
        }

        public RegisterPublisherResponse RegisterPublisher(string topic, string topicType)
        {
            Arg[] args = {
                new Arg(CallerId),
                new Arg(topic),
                new Arg(topicType),
                new Arg(CallerUri),
            };
            object[] response = MethodCall("registerPublisher", args);
            return new RegisterPublisherResponse(response);
        }

        public UnregisterPublisherResponse UnregisterPublisher(string topic)
        {
            Arg[] args = {
                new Arg(CallerId),
                new Arg(topic),
                new Arg(CallerUri),
            };
            object[] response = MethodCall("unregisterPublisher", args);
            return new UnregisterPublisherResponse(response);
        }

        public GetSystemStateResponse GetSystemState()
        {
            Arg[] args = {
                new Arg(CallerId),
            };
            object[] response = MethodCall("getSystemState", args);
            return new GetSystemStateResponse(response);
        }

        public LookupServiceResponse LookupService(string service)
        {
            Arg[] args = {
                new Arg(CallerId),
                new Arg(service),
            };
            object[] response = MethodCall("lookupService", args);
            return new LookupServiceResponse(response);
        }

        public DefaultResponse RegisterService(string service, Uri rosRpcUri)
        {
            Arg[] args = {
                new Arg(CallerId),
                new Arg(service),
                new Arg(rosRpcUri),
                new Arg(CallerUri),
            };
            object[] response = MethodCall("registerService", args);
            return new DefaultResponse(response);
        }

        public UnregisterServiceResponse UnregisterService(string service, Uri rosRpcUri)
        {
            Arg[] args = {
                new Arg(CallerId),
                new Arg(service),
                new Arg(rosRpcUri),
            };
            object[] response = MethodCall("unregisterService", args);
            return new UnregisterServiceResponse(response);
        }

        object[] MethodCall(string function, Arg[] args)
        {
            object tmp = Service.MethodCall(MasterUri, CallerUri, function, args, TimeoutInMs);
            if (!(tmp is object[] result))
            {
                Logger.Log($"Rpc Response: Expected type object[], got {tmp.GetType().Name}");
                return null;
            }
            return result;
        }
    }

    public abstract class BaseResponse
    {
        public StatusCode Code { get; }
        public string StatusMessage { get; }
        private protected bool hasParseError;

        public bool IsValid => Code == StatusCode.Success && !hasParseError;

        private protected BaseResponse(object[] a)
        {
            if (!EnsureSize(a, 2))
            {
                hasParseError = true;
                Code = StatusCode.Error;
                StatusMessage = "Parse error for input.";
                return;
            }
            Code = (StatusCode)Cast<int>(a[0]);
            StatusMessage = Cast<string>(a[1]);
            if (!IsValid)
            {
                Logger.LogDebug($"Rpc Response: {GetType().Name} failed! " + StatusMessage);
            }
        }

        private protected T Cast<T>(object a)
        {
            if (!(a is T t))
            {
                Logger.LogDebug($"Rpc Response: Expected type '{typeof(T).Name}, got {a.GetType().Name}");
                hasParseError = true;
                return default;
            }
            return t;
        }

        protected bool EnsureSize<T>(IList<T> a, int size = 1)
        {
            if (a == null)
            {
                Logger.Log($"Rpc Response: Null input");
                hasParseError = true;
                return false;
            }
            if (a.Count < size)
            {
                Logger.Log($"Rpc Response: Expected size '{size}, got {a.Count}");
                hasParseError = true;
                return false;
            }
            return true;
        }

    }

    public class TopicTuple
    {
        public string Topic { get; }
        public ReadOnlyCollection<string> Members { get; }

        public TopicTuple(string topic, IList<string> members)
        {
            Topic = topic;
            Members = new ReadOnlyCollection<string>(members);
        }
    }

    public sealed class GetSystemStateResponse : BaseResponse
    {
        static readonly ReadOnlyCollection<TopicTuple> Empty =
            new ReadOnlyCollection<TopicTuple>(Array.Empty<TopicTuple>());

        public ReadOnlyCollection<TopicTuple> Publishers { get; } = Empty;
        public ReadOnlyCollection<TopicTuple> Subscribers { get; } = Empty;
        public ReadOnlyCollection<TopicTuple> Services { get; } = Empty;

        internal GetSystemStateResponse(object[] a) : base(a)
        {
            if (!IsValid || !EnsureSize(a, 3))
            {
                return;
            }
            object[] root = Cast<object[]>(a[2]);
            if (!EnsureSize(root, 3))
            {
                return;
            }
            Publishers = CreateTuple(root[0]);
            Subscribers = CreateTuple(root[1]);
            Services = CreateTuple(root[2]);
        }

        ReadOnlyCollection<TopicTuple> CreateTuple(object root)
        {
            object[] list = Cast<object[]>(root);
            if (list == null)
            {
                return Empty;
            }

            TopicTuple[] result = new TopicTuple[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                object[] tuple = Cast<object[]>(list[i]);
                if (!EnsureSize(tuple, 2))
                {
                    return Empty;
                }
                string topic = Cast<string>(tuple[0]);
                object[] tmp = Cast<object[]>(tuple[1]);
                if (tmp == null)
                {
                    return Empty;
                }
                string[] members = tmp.Cast<string>().ToArray();
                result[i] = new TopicTuple(topic, members);
            }
            return new ReadOnlyCollection<TopicTuple>(result);
        }
    }

    public sealed class DefaultResponse : BaseResponse
    {
        internal DefaultResponse(object[] a) : base(a) { }
    }

    public sealed class GetUriResponse : BaseResponse
    {
        public Uri Uri { get; }

        internal GetUriResponse(object[] a) : base(a)
        {
            if (!IsValid || !EnsureSize(a, 3))
            {
                return;
            }
            if (Uri.TryCreate(Cast<string>(a[2]) ?? "", UriKind.Absolute, out Uri uri))
            {
                Uri = uri;
            }
            else
            {
                Logger.Log($"Rpc Response: Failed to parse GetUriResponse uri: " + a[2]);
                hasParseError = true;
                Uri = null;
            }
        }
    }

    public sealed class LookupNodeResponse : BaseResponse
    {
        public Uri Uri { get; }

        internal LookupNodeResponse(object[] a) : base(a)
        {
            if (!IsValid || !EnsureSize(a, 3))
            {
                return;
            }
            if (Uri.TryCreate(Cast<string>(a[2]) ?? "", UriKind.Absolute, out Uri uri))
            {
                Uri = uri;
            }
            else
            {
                Logger.Log($"RcpMaster: Failed to parse LookupNodeResponse uri: " + a[2]);
                hasParseError = true;
                Uri = null;
            }
        }
    }

    public sealed class GetPublishedTopicsResponse : BaseResponse
    {
        static readonly ReadOnlyCollection<(string, string)> Empty =
            new ReadOnlyCollection<(string, string)>(Array.Empty<(string, string)>());

        public ReadOnlyCollection<(string, string)> Topics { get; } = Empty;

        internal GetPublishedTopicsResponse(object[] a) : base(a)
        {
            if (!IsValid || !EnsureSize(a, 3))
            {
                return;
            }
            object[] tmp = Cast<object[]>(a[2]);
            if (tmp == null)
            {
                return;
            }

            (string, string)[] topics = new (string, string)[tmp.Length];
            for (int i = 0; i < topics.Length; i++)
            {
                object[] topic = Cast<object[]>(tmp[i]);
                topics[i] = (Cast<string>(topic[0]), Cast<string>(topic[1]));
            }
            Topics = new ReadOnlyCollection<(string, string)>(topics);
        }
    }

    public sealed class RegisterSubscriberResponse : BaseResponse
    {
        static readonly ReadOnlyCollection<Uri> Empty =
            new ReadOnlyCollection<Uri>(Array.Empty<Uri>());

        public ReadOnlyCollection<Uri> Publishers { get; } = Empty;

        internal RegisterSubscriberResponse(object[] a) : base(a)
        {
            if (!IsValid || !EnsureSize(a, 3))
            {
                return;
            }
            object[] tmp = Cast<object[]>(a[2]);
            if (tmp == null)
            {
                hasParseError = true;
                return;
            }
            List<Uri> publishers = new List<Uri>();
            for (int i = 0; i < tmp.Length; i++)
            {
                if (!Uri.TryCreate(Cast<string>(tmp[i]) ?? "", UriKind.Absolute, out Uri publisher))
                {
                    Logger.Log($"RcpMaster: Invalid uri '{tmp[i]}'");
                }
                else
                {
                    publishers.Add(publisher);
                }
            }
            Publishers = new ReadOnlyCollection<Uri>(publishers);
        }
    }

    public sealed class UnregisterSubscriberResponse : BaseResponse
    {
        public int NumUnsubscribed { get; }

        internal UnregisterSubscriberResponse(object[] a) : base(a)
        {
            if (!IsValid || !EnsureSize(a, 3))
            {
                return;
            }
            NumUnsubscribed = Cast<int>(a[2]);
        }
    }

    public sealed class RegisterPublisherResponse : BaseResponse
    {
        static readonly ReadOnlyCollection<string> Empty =
            new ReadOnlyCollection<string>(Array.Empty<string>());

        public ReadOnlyCollection<string> Subscribers { get; } = Empty;

        internal RegisterPublisherResponse(object[] a) : base(a)
        {
            if (!IsValid || !EnsureSize(a, 3))
            {
                return;
            }
            object[] tmp = Cast<object[]>(a[2]);
            if (tmp == null)
            {
                return;
            }
            string[] subscribers = new string[tmp.Length];
            for (int i = 0; i < subscribers.Length; i++)
            {
                subscribers[i] = Cast<string>(tmp[i]);
            }
            Subscribers = new ReadOnlyCollection<string>(subscribers);
        }
    }

    public sealed class UnregisterPublisherResponse : BaseResponse
    {
        public int NumUnregistered { get; }

        internal UnregisterPublisherResponse(object[] a) : base(a)
        {
            if (!IsValid || !EnsureSize(a, 3))
            {
                return;
            }
            NumUnregistered = Cast<int>(a[2]);
        }
    }

    public sealed class LookupServiceResponse : BaseResponse
    {
        public Uri ServiceUrl { get; }

        internal LookupServiceResponse(object[] a) : base(a)
        {
            if (!IsValid || !EnsureSize(a, 3))
            {
                return;
            }
            ServiceUrl = new Uri(Cast<string>(a[2]));
        }
    }

    public sealed class UnregisterServiceResponse : BaseResponse
    {
        public int NumUnregistered { get; }

        internal UnregisterServiceResponse(object[] a) : base(a)
        {
            if (!IsValid || !EnsureSize(a, 3))
            {
                return;
            }
            NumUnregistered = Cast<int>(a[2]);
        }
    }


}
