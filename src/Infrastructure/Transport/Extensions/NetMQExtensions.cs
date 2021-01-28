using System;
using Eventually.Infrastructure.Transport.Messages;
using NetMQ;
using NetMQ.Sockets;

namespace Eventually.Infrastructure.Transport.Extensions
{
    public static class NetMQExtensions
    {
        public static void SendToAddress(this RouterSocket socket, IWireMessage wireMessage, Guid recipientId)
        {
            if (recipientId == Guid.Empty)
            {
                throw new ArgumentException("SendToAddress requires a valid recipientId", nameof(recipientId));
            }

            var netMQMessage = new NetMQMessage();
            netMQMessage.Append(recipientId.ToByteArray());
            netMQMessage.Append(wireMessage.SenderId.ToByteArray());
            netMQMessage.AppendEmptyFrame();
            netMQMessage.Append(wireMessage.MessageBytes);

            socket.SendMessage(netMQMessage);
        }

        public static void SendResponse(this NetMQSocket socket, IWireMessage wireMessage, Guid replyToId)
        {
            if (replyToId == Guid.Empty)
            {
                throw new ArgumentException("SendResponse requires a valid replyToId", nameof(replyToId));
            }

            var netMQMessage = new NetMQMessage();
            netMQMessage.Append(replyToId.ToByteArray());
            netMQMessage.Append(wireMessage.SenderId.ToByteArray());
            netMQMessage.AppendEmptyFrame();
            netMQMessage.Append(wireMessage.MessageBytes);

            socket.SendMessage(netMQMessage);
        }
        
        public static void SendMessage(this DealerSocket socket, IWireMessage wireMessage)
        {
            // DealerSocket only sends to a single destination address, so there's no need to prepend the recipient id
            var netMQMessage = new NetMQMessage();
            netMQMessage.Append(wireMessage.SenderId.ToByteArray());
            netMQMessage.AppendEmptyFrame();
            netMQMessage.Append(wireMessage.MessageBytes);

            socket.SendMessage(netMQMessage);
        }

        public static void SendMessage(this NetMQSocket socket, NetMQMessage message)
        {
            socket.SendMultipartMessage(message);
        }
    }
}