using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot
{
    public class ReplyToUser
    {
        private static Activity _activity;
        public static void SetActivity(Activity activity)
        {
            _activity = activity;
        }
        public async static void PostReplyToUser(string responseText)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(_activity.ServiceUrl));
            Activity reply = _activity.CreateReply(responseText);
            await connector.Conversations.ReplyToActivityAsync(reply);
        }
    }
}