using Discord;
using Discord.Rest;
using Stonks.Class;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Stonks.Module
{
    internal class ReactMessageModule
    {
        public static async void CreateReactMessage(RestUserMessage msg, List<IEmote> emoji, List<Action> action, TimeSpan timeSpan, ulong userId)
        {
            if (emoji.Count != action.Count)
                throw new ArgumentException("Emoji list and Action list must have the same length.");

            MemoryCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = timeSpan };

            foreach (var item in emoji)
            {
                await msg.AddReactionAsync(item);
            }

            cache.Add(msg.Id.ToString(), new ReactMessage { messageId = msg.Id, messageEmote = emoji, messageAction = action, messageUserId = userId }, policy);

            Console.WriteLine("{0} Cache {1,24} Cached", DateTime.Now.ToString("HH:mm:ss"), msg.Id);
        }
    }
}