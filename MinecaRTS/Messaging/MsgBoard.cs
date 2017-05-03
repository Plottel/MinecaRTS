using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public struct Message
    {
        public IHandleMessages sender;
        public IHandleMessages receiver;
        public MessageType type;
        public uint delay;
        public dynamic extraInfo;

        public Message(IHandleMessages s, IHandleMessages r, MessageType msgType, dynamic info, uint d = 0)
        {
            sender = s;
            receiver = r;
            type = msgType;
            delay = d;
            extraInfo = info;
        }
    }

    public static class MsgBoard
    {
        private static HashSet<Message> messages = new HashSet<Message>();

        private static void SendMessage(IHandleMessages receiver, Message message)
        {
            receiver.HandleMessage(message);
        }

        public static void AddMessage(IHandleMessages sender, ulong receiverID, MessageType msg, dynamic info = null, uint delay = 0)
        {
            IHandleMessages receiver = MsgHandlerRegistry.GetMsgHandlerFromID(receiverID);
            Message message = new Message(sender, receiver, msg, info, delay);

            if (delay <= 0)
                SendMessage(receiver, message);
        }
    }
}
