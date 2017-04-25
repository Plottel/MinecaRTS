using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public struct Message
    {
        public Entity sender;
        public Entity receiver;
        public MessageType msg;
        public uint delay;

        public Message(Entity s, Entity r, MessageType msgType, uint d = 0)
        {
            sender = s;
            receiver = r;
            msg = msgType;
            delay = d;
        }
    }

    public static class MsgBoard
    {
        private static HashSet<Message> messages = new HashSet<Message>();

        private static void SendMessage(Entity receiver, Message message)
        {
            receiver.HandleMessage(message);
        }

        public static void AddMessage(Entity sender, ulong receiverID, MessageType msg, uint delay = 0)
        {
            Entity receiver = EntityRegistry.GetEntityFromID(receiverID);
            Message message = new Message(sender, receiver, msg, delay);

            if (delay <= 0)
                SendMessage(receiver, message);
        }
    }
}
