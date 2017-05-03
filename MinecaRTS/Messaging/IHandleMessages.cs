using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public interface IHandleMessages
    {
        ulong ID
        {
            get;
        }

        void HandleMessage(Message message);
    }
}
