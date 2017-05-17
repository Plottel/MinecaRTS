using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public struct Message
    {
        public object sender;
        public IHandleMessages receiver;
        public MessageType type;
        public ulong delay;
        public dynamic extraInfo;
        public ulong dispatchTime;

        public Message(object s, IHandleMessages r, MessageType msgType, dynamic info = null, ulong d = 0)
        {
            sender = s;
            receiver = r;
            type = msgType;
            delay = d;
            dispatchTime = World.GameTime + d;
            extraInfo = info;
        }
    }

    public static class MsgBoard
    {
        private static object sender_irrelevant = new object();

        public static object SENDER_IRRELEVANT
        {
            get { return sender_irrelevant; }
        }

        private static List<Message> messages = new List<Message>();

        private static void SendMessage(IHandleMessages receiver, Message message)
        {
            receiver.HandleMessage(message);
        }

        public static void AddMessage(object sender, ulong receiverID, MessageType msg, dynamic info = null, uint delay = 0)
        {
            IHandleMessages receiver = MsgHandlerRegistry.GetMsgHandlerFromID(receiverID);
            Message message = new Message(sender, receiver, msg, info, delay);

            messages.Add(message);
        }

        public static void SendMessages()
        {
            messages.OrderByDescending(message => message.dispatchTime);

            for (int i = messages.Count - 1; i >= 0; --i)
            {
                if (messages[i].dispatchTime <= World.GameTime)
                {
                    SendMessage(messages[i].receiver, messages[i]);
                    messages.RemoveAt(i);
                }
                else
                    return; // Since messages are sorted by dispatch time, we can stop as soon as we find one not ready to send.
            }
        }
    }
}
