using Discord;
using System;
using System.Collections.Generic;

namespace Stonks.Class
{
    internal class ReactMessage
    {
        public ulong messageId { get; set; }
        public List<IEmote> messageEmote { get; set; }
        public List<Action> messageAction { get; set; }
        public ulong messageUserId { get; set; }
    }
}