using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Synapse_Chat_Server.Server
{
    public static class Filter
    {
        public static Dictionary<string, string> LastMessage = new Dictionary<string, string>();
        public static Dictionary<string, long> LastMessageTimes = new Dictionary<string, long>();

        public static bool Process(Chat Parent, string Message)
        {
            if (!LastMessageTimes.ContainsKey(Parent.Username))
            {
                LastMessageTimes.Add(Parent.Username, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
            else
            {
                if (LastMessageTimes[Parent.Username] >= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    return true;

                LastMessageTimes[Parent.Username] = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1;
            }

            if (!LastMessage.ContainsKey(Parent.Username))
            {
                LastMessage.Add(Parent.Username, Message);
            }
            else
            {
                if (LastMessage[Parent.Username] == Message)
                {
                    return true;
                }

                LastMessage[Parent.Username] = Message;
            }

            return false;
        }
    }
}
