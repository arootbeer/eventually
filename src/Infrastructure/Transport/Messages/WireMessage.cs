using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Eventually.Interfaces.Common.Messages;
using Eventually.Interfaces.DomainCommands;
using Eventually.Utilities.Serialization;
using NetMQ;

namespace Eventually.Infrastructure.Transport.Messages
{
    public class WireMessage<TMessage> : WireMessage, IWireMessage<TMessage>
        where TMessage : IMessage
    {
        internal WireMessage(Guid senderId, byte[] content)
        {
            Message = Deserialize(content);
            SenderId = senderId;
        }

        private static TMessage Deserialize(byte[] rawContent)
        {
            var content = GetString(rawContent);
            return Serializer.Deserialize<TMessage>(content);
        }
        
        internal WireMessage(TMessage message, Guid senderId)
        {
            Message = message;
            SenderId = senderId;
            var messageTypeName = MessageType.FullName;

            MessageBytes = PreambleBytes
                .Concat(BitConverter.GetBytes(messageTypeName.Length))
                .Concat(Encoding.ASCII.GetBytes(messageTypeName))
                .Concat(Serialize(message))
                .ToArray();
        }

        private static byte[] Serialize(TMessage message)
        {
            var json = Serializer.Serialize(message);
            return Encoding.UTF8.GetBytes(json);
        }

        public TMessage Message { get; }

        IMessage IWireMessage.Message => Message;

        public Guid SenderId { get; }
        
        public byte[] MessageBytes { get; }

        public Type MessageType => typeof(TMessage);

        public Guid CommandOrQueryId => Message switch
        {
            DomainCommandResponse commandResponse => commandResponse.CommandId,
            // ManagedApplicationCommandResponse commandResponse => commandResponse.CommandId,
            // ManagedApplicationQueryResponse queryResponse => queryResponse.QueryId,
            _ => Guid.Empty
        };
    }

    public class WireMessage
    {
        protected static readonly byte[] PreambleBytes = { 3, 4 };
        private static readonly int HeaderLength = PreambleBytes.Length + 4;
        private static readonly ConcurrentQueue<Assembly> MessageAssemblies = new ConcurrentQueue<Assembly>();

        protected static ImmutableTypesJsonSerializer Serializer = new ImmutableTypesJsonSerializer();

        static WireMessage()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (o, e) => LoadMessageTypes(e.LoadedAssembly);
            AppDomain.CurrentDomain
                .GetAssemblies()
                .ToList()
                .ForEach(LoadMessageTypes);

            void LoadMessageTypes(Assembly assembly)
            {
                if (assembly.GetTypes().Any(type => type.GetInterfaces().Any(iface => iface == typeof(IMessage))))
                {
                    MessageAssemblies.Enqueue(assembly);
                }
            }
        }
    
        internal static WireMessageParameters GetParameters(NetMQMessage message)
        {
            if (message.FrameCount < 3 || message.First.BufferSize != 16)
            {
                throw new Exception(
                    $"Received a message in an unknown format. Message Bytes:{Environment.NewLine}" +
                    String.Join(Environment.NewLine, message.Select(frame => $"`{BitConverter.ToString(frame.Buffer)}`"))
                );
            }
            var senderId = new Guid(message.First.Buffer);

            var rawMessage = message
                .SkipWhile(frame =>
                        frame.BufferSize == 16         //sender/recipient identities
                        || frame == NetMQFrame.Empty   //message separator
                )
                .SelectMany(frame => frame.Buffer)
                .ToArray();

            var messageBytes = new byte[rawMessage.Length];
            Array.Copy(rawMessage, messageBytes, rawMessage.Length);
            if (rawMessage.Count() < 6 || !rawMessage.Take(2).SequenceEqual(PreambleBytes))
            {
                throw new Exception($"A malformed message was encountered: `{Encoding.UTF8.GetString(rawMessage)}`");
            }

            var typeNameLength = BitConverter.ToInt32(rawMessage, PreambleBytes.Length);
            var typeName = GetString(rawMessage, 6, typeNameLength);
            var messageType = MessageAssemblies.Select(assembly => assembly.GetType(typeName)).FirstOrDefault(type => type != null);
            if (messageType == null)
            {
                throw new Exception($"A message with unknown type `{typeName}` was encountered");
            }
            return new WireMessageParameters(messageType, senderId, rawMessage.Skip(HeaderLength + typeNameLength).ToArray());
        }

        protected static string GetString(IEnumerable<byte> rawMessage, int offset = 0, int length = -1)
        {
            rawMessage = rawMessage.ToArray();
            if (length == -1)
            {
                length = rawMessage.Count();
            }

            return Encoding.ASCII.GetString(rawMessage.Skip(offset).Take(length).ToArray());
        }
    }
}