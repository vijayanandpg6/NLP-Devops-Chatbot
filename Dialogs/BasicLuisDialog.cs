using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Net.Http;
using System.Collections.Generic;
using System.Text;
using LuisBot.Schedular;
using Quartz;
using Quartz.Impl;
using LuisBot.BotActions;
using LuisBot;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        private static readonly HttpClient client = new HttpClient();
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }
        // Store notes in a dictionary that uses the title as a key
        //private readonly Dictionary<string, Note> noteByTitle = new Dictionary<string, Note>();



        // CONSTANTS - Entities
        public const string DateTimeEntity = "datetimeV2";
        public const string JobNameEntity = "JobName";

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            string[] greetingText = { "Hello", "Hello there", "Yup Hi", "Hi", "Hi Dude", "Hello dude", "Hi what's up!!", "Hi, Good to see you!!", "Hey what's up!!", "Hey there!" };
            Random rand = new Random();
            int index = rand.Next(0, 10);
            await BotReply(context, greetingText[index]);
        }

        [LuisIntent("BuildJob")]
        public async Task BuildJobIntent(IDialogContext context, LuisResult result)
        {
            string replyString = "";
            EntityRecommendation data;
            DateTime dateTime = DateTime.Now;
            string jobName = "";
            if (result.TryFindEntity("JobName", out data))
            {
                jobName = data.Entity;
            }
            if (result.TryFindEntity("builtin.datetimeV2.datetime", out data))
            {
                //trigger build at specified date time
                var resolutionValues = (IList<object>)data.Resolution["values"];
                foreach (var value in resolutionValues)
                {
                    dateTime = Convert.ToDateTime(((IDictionary<string, object>)value)["value"]);
                }
                BuildJob.TriggerScheduleBuildOnce(context, jobName, dateTime);
                replyString = String.Format("Build trigger will be started for {0} at {1}.", jobName, dateTime);
            }
            else
            {
                if (result.TryFindEntity("builtin.datetimeV2.time", out data))
                {
                    //only time specified, trigger build at specified time today
                    var resolutionValues = (IList<object>)data.Resolution["values"];
                    foreach (var value in resolutionValues)
                    {
                        dateTime = Convert.ToDateTime(((IDictionary<string, object>)value)["value"]);
                    }
                    BuildJob.TriggerScheduleBuildOnce(context, jobName, dateTime);
                    replyString = String.Format("Build trigger will be started for {0} at {1}.", jobName, dateTime);
                }
                else
                {
                    //if no date time, trigger immediate build
                    BuildJob.TriggerImmediateBuildOnce(context, jobName);
                }

            }
            await BotReply(context, String.Format(replyString + "\nI will let you know with success or failure message.."));
        }

        [LuisIntent("BuildJobWithParameter")]
        public async Task BuildJobWithParameterIntent(IDialogContext context, LuisResult result)
        {
            string replyString = "";
            EntityRecommendation data;
            DateTime dateTime = DateTime.Now;
            string jobName = "", parameter = "";
            if (result.TryFindEntity("JobName", out data))
            {
                jobName = data.Entity;
            }
            if (result.TryFindEntity("Parameter", out data))
            {
                parameter = data.Entity;
            }
            if (result.TryFindEntity("builtin.datetimeV2.datetime", out data))
            {
                var resolutionValues = (IList<object>)data.Resolution["values"];
                foreach (var value in resolutionValues)
                {
                    dateTime = Convert.ToDateTime(((IDictionary<string, object>)value)["value"]);
                }
                BuildJob.TriggerScheduleBuildOnce(context, jobName, dateTime);
                replyString = String.Format("Build trigg will be started for {0} at {1}.", jobName, dateTime);
            }
            else
            {
                if (result.TryFindEntity("builtin.datetimeV2.time", out data))
                {
                    var resolutionValues = (IList<object>)data.Resolution["values"];
                    foreach (var value in resolutionValues)
                    {
                        dateTime = Convert.ToDateTime(((IDictionary<string, object>)value)["value"]);
                    }
                    BuildJob.TriggerScheduleBuildOnce(context, jobName, dateTime);
                    replyString = String.Format("Build trigger will started for {0} at {1}.", jobName, dateTime);
                }

            }
            await BotReply(context, String.Format(replyString + "\nI will let you know with success or failure message.."));
        }

        [LuisIntent("AllBuildInfo")]
        public async Task AllBuildInfoIntent(IDialogContext context, LuisResult result)
        {
            var buildData = await BuildActions.AllBuildInfo();
            await BotReply(context, buildData);
        }

        [LuisIntent("BuildInfo")]
        public async Task BuildInfoIntent(IDialogContext context, LuisResult result)
        {
            string jobName = "";
            EntityRecommendation data;
            if (result.TryFindEntity("JobName", out data))
            {
                jobName = data.Entity;
            }
            var buildData = await BuildActions.LastBuildInfo(jobName);
            await BotReply(context, buildData);
        }

        [LuisIntent("BuildStatus")]
        public async Task BuildStatusIntent(IDialogContext context, LuisResult result)
        {
            string jobName = "";
            EntityRecommendation data;
            if (result.TryFindEntity("JobName", out data))
            {
                jobName = data.Entity;
            }
            var buildData = await BuildActions.LastBuildStatus(jobName);
            await BotReply(context, buildData);
        }

        [LuisIntent("CreateJob")]
        public async Task CreateJobIntent(IDialogContext context, LuisResult result)
        {
            string jobName = "";
            EntityRecommendation data;
            if (result.TryFindEntity("JobName", out data))
            {
                jobName = data.Entity;
            }
            var responseMessage = await BuildActions.CreateJob(jobName);
            await BotReply(context, responseMessage);
        }

        [LuisIntent("DeleteJob")]
        public async Task DeleteJobIntent(IDialogContext context, LuisResult result)
        {
            string jobName = "";
            EntityRecommendation data;
            if (result.TryFindEntity("JobName", out data))
            {
                jobName = data.Entity;
            }
            var responseMessage = await BuildActions.DeleteJob(jobName);
            await BotReply(context, responseMessage);
        }

        [LuisIntent("BuildNumber")]
        public async Task BuildNumberIntent(IDialogContext context, LuisResult result)
        {
            string jobName = "";
            EntityRecommendation data;
            if (result.TryFindEntity("JobName", out data))
            {
                jobName = data.Entity;
            }
            var responseMessage = await BuildActions.GetBuildNumber(jobName);
            await BotReply(context, responseMessage);
        }

        [LuisIntent("BuildTimeStamp")]
        public async Task BuildTimeStampIntent(IDialogContext context, LuisResult result)
        {
            string jobName = "";
            EntityRecommendation data;
            if (result.TryFindEntity("JobName", out data))
            {
                jobName = data.Entity;
            }
            var responseMessage = await BuildActions.BuildTimeStamp(jobName);
            await BotReply(context, responseMessage);
        }

        [LuisIntent("ForceRestart")]
        public async Task ForceRestartIntent(IDialogContext context, LuisResult result)
        {
            var responseMessage = await BuildActions.ForceRestart();
            await BotReply(context, responseMessage);
        }

        [LuisIntent("SafeRestart")]
        public async Task SafeRestartIntent(IDialogContext context, LuisResult result)
        {
            var responseMessage = await BuildActions.SafeRestart();
            await BotReply(context, responseMessage);
        }

        [LuisIntent("GetAvailablePlugin")]
        public async Task GetAvailablePluginIntent(IDialogContext context, LuisResult result)
        {
            var responseMessage = await BuildActions.GetAvailablePlugin();
            await BotReply(context, responseMessage);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            var responseMessage = "I am you Devops dude! I can assist you with these.\n1) Create Job\n2) Delete Job\n3) Build Jobs\n4) Build informations\n5) Job build status\n6) Available Plugins\n7) Safe / Force restart\n8) Other tasks like timestamp, build number, etc..";
            await BotReply(context, responseMessage);
            //await this.ShowLuisResult(context, response);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            //context.Wait(MessageReceived);
        }

        private async Task BotReply(IDialogContext context, string replyString)
        {
            ReplyToUser.PostReplyToUser(replyString);
            //await context.PostAsync(replyString);
        }
    }


}