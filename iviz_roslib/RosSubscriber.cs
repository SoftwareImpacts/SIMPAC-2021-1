﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iviz.Msgs;
using Iviz.XmlRpc;

namespace Iviz.Roslib
{
    /// <summary>
    /// Interface for all ROS subscribers.
    /// </summary>
    public interface IRosSubscriber : IDisposable
    {
        /// <summary>
        /// Timeout in milliseconds to wait for a publisher handshake.
        /// </summary>
        public int TimeoutInMs { get; set; }

        /// <summary>
        /// The name of the topic.
        /// </summary>        
        public string Topic { get; }

        /// <summary>
        /// The ROS message type of the topic.
        /// </summary>        
        public string TopicType { get; }

        /// <summary>
        /// The number of publishers in the topic.
        /// </summary>
        public int NumPublishers { get; }

        /// <summary>
        /// Returns a structure that represents the internal state of the subscriber. 
        /// </summary>           
        public SubscriberTopicState GetState();

        /// <summary>
        /// Checks whether this subscriber has provided the given id from a Subscribe() call.
        /// </summary>
        /// <param name="id">Identifier to check.</param>
        /// <returns>Whether the id was provided by this subscriber.</returns>        
        public bool ContainsId(string id);

        /// <summary>
        /// Checks whether the class of the subscriber message type corresponds to the given type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>Whether the class type matches.</returns>
        public bool MessageTypeMatches(Type type);

        /// <summary>
        /// Generates a new subscriber id with the given callback function.
        /// </summary>
        /// <param name="callback">The function to call when a message arrives.</param>
        /// <returns>The subscribed id.</returns>
        /// <exception cref="ArgumentNullException">The callback is null.</exception>
        public string Subscribe(Action<IMessage> callback);

        /// <summary>
        /// Unregisters the given id from the subscriber. If the subscriber has no ids left, the topic will be unsubscribed from the master.
        /// </summary>
        /// <param name="id">The id to be unregistered.</param>
        /// <returns>Whether the id belonged to the subscriber.</returns>        
        public bool Unsubscribe(string id);

        /// <summary>
        /// Unregisters the given id from the subscriber. If the subscriber has no ids left, the topic will be unsubscribed from the master.
        /// </summary>
        /// <param name="id">The id to be unregistered.</param>
        /// <returns>Whether the id belonged to the subscriber.</returns>        
        public Task<bool> UnsubscribeAsync(string id);

        internal Task PublisherUpdateRcpAsync(IEnumerable<Uri> publisherUris);
    }

    /// <summary>
    /// Manager for a subscription to a ROS topic.
    /// </summary>
    public class RosSubscriber<T> : IRosSubscriber where T : IMessage
    {
        readonly Dictionary<string, Action<T>> callbacksById = new Dictionary<string, Action<T>>();
        readonly CancellationTokenSource aliveTokenSource = new CancellationTokenSource();
        readonly RosClient client;
        Action<T>[] callbacks = Array.Empty<Action<T>>(); // cache to iterate through callbacks quickly
        int totalSubscribers;
        bool disposed;

        internal TcpReceiverManager<T> Manager { get; }

        /// <summary>
        /// A cancellation token that gets canceled when the subscriber is disposed.
        /// Used for external wrappers like <see cref="RosSubscriberChannelReader{T}"/>. 
        /// </summary>
        public CancellationToken CancellationToken => aliveTokenSource.Token; 

        /// <summary>
        /// Whether this subscriber is valid.
        /// </summary>
        public bool IsAlive => !CancellationToken.IsCancellationRequested;
        
        public string Topic => Manager.Topic;
        public string TopicType => Manager.TopicType;
        public int NumPublishers => Manager.NumConnections;

        /// <summary>
        /// The number of ids generated by this subscriber.
        /// </summary>
        public int NumIds => callbacksById.Count;

        /// <summary>
        /// Whether the TCP_NODELAY flag was requested.
        /// </summary>
        public bool RequestNoDelay => Manager.RequestNoDelay;

        /// <summary>
        /// Event triggered when a new publisher appears.
        /// </summary>
        public event Action<RosSubscriber<T>>? NumPublishersChanged;

        public int TimeoutInMs
        {
            get => Manager.TimeoutInMs;
            set => Manager.TimeoutInMs = value;
        }

        internal RosSubscriber(RosClient client, TopicInfo<T> topicInfo, bool requestNoDelay, int timeoutInMs)
        {
            this.client = client;
            Manager = new TcpReceiverManager<T>(this, client, topicInfo, requestNoDelay)
                {TimeoutInMs = timeoutInMs};
        }

        internal void MessageCallback(in T msg)
        {
            foreach (Action<T> callback in callbacks)
            {
                try
                {
                    callback(msg);
                }
                catch (Exception e)
                {
                    Logger.LogError($"{this}: Exception from callback : {e}");
                }
            }
        }

        internal void RaiseNumPublishersChanged()
        {
            try
            {
                NumPublishersChanged?.Invoke(this);
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        string GenerateId()
        {
            string newId = totalSubscribers == 0 ? Topic : $"{Topic}-{totalSubscribers}";
            totalSubscribers++;
            return newId;
        }

        void AssertIsAlive()
        {
            if (!IsAlive)
            {
                throw new ObjectDisposedException("This is not a valid subscriber");
            }
        }

        public SubscriberTopicState GetState()
        {
            AssertIsAlive();
            return new SubscriberTopicState(Topic, TopicType, callbacksById.Keys.ToArray(), Manager.GetStates());
        }

        async Task IRosSubscriber.PublisherUpdateRcpAsync(IEnumerable<Uri> publisherUris)
        {
            await Manager.PublisherUpdateRpcAsync(publisherUris).Caf();
        }

        /// <summary>
        /// Disposes this subscriber. This should only be called by RosClient.
        /// </summary>
        void IDisposable.Dispose()
        {
            Dispose();
        }

        void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            callbacksById.Clear();
            Manager.Stop();
            callbacks = Array.Empty<Action<T>>();
            NumPublishersChanged = null;
            aliveTokenSource.Cancel();
        }

        public bool MessageTypeMatches(Type type)
        {
            return type == typeof(T);
        }

        string IRosSubscriber.Subscribe(Action<IMessage> callback)
        {
            if (callback is null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            AssertIsAlive();

#if DEBUG__
            Logger.LogDebug($"{this}: Subscribing to '{Topic}' with type '{TopicType}'");
#endif

            string id = GenerateId();
            callbacksById.Add(id, t => callback(t));
            callbacks = callbacksById.Values.ToArray();
            return id;
        }

        /// <summary>
        /// Generates a new subscriber id with the given callback function.
        /// </summary>
        /// <param name="callback">The function to call when a message arrives.</param>
        /// <typeparam name="T">The message type</typeparam>
        /// <returns>The subscribed id.</returns>
        /// <exception cref="ArgumentNullException">The callback is null.</exception>
        public string Subscribe(Action<T> callback)
        {
            if (callback is null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            
            AssertIsAlive();

            string id = GenerateId();
            callbacksById.Add(id, callback);
            callbacks = callbacksById.Values.ToArray();
            return id;
        }

        public bool ContainsId(string id)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return callbacksById.ContainsKey(id);
        }

        public bool Unsubscribe(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!IsAlive)
            {
                return true;
            }

            bool removed = callbacksById.Remove(id);
            callbacks = callbacksById.Values.ToArray();

            if (callbacksById.Count == 0)
            {
                Dispose();
                client.RemoveSubscriber(this);
            }

            return removed;
        }

        public async Task<bool> UnsubscribeAsync(string id)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!IsAlive)
            {
                return true;
            }
            
            bool removed = callbacksById.Remove(id);
            callbacks = callbacksById.Values.ToArray();

            if (callbacksById.Count == 0)
            {
                Dispose();
                await client.RemoveSubscriberAsync(this).Caf();
            }

            return removed;
        }

        public override string ToString()
        {
            return $"[Subscriber {Topic} [{TopicType}] ]";
        }
    }
}