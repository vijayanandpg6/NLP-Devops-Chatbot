using LuisBot.BotActions;
using Microsoft.Bot.Builder.Dialogs;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LuisBot.Schedular
{
    public class BuildTriggerJob : IJob
    {
        static string _jobname = "";
        static IDialogContext _dialogContext;
        public static void SetJobName(IDialogContext context, string jobName)
        {
            _jobname = jobName;
            _dialogContext = context;
        }
        public Task Execute(IJobExecutionContext context)
        {
            //POST request to Jenkins.. to be implemented
            try
            {
                BuildActions.TriggerBuild(_jobname);
                _dialogContext.PostAsync("Interupting with a notification, Build trigger completed, Status : " + BuildActions.LastBuildStatus(_jobname) + " for job" + _jobname);
            }
            catch (Exception ex)
            {
                _dialogContext.PostAsync("Interupting with a notification, Build trigger could be completed properly, Status : " + BuildActions.LastBuildStatus(_jobname) + " for job" + _jobname);
            }
            
            return null;
        }
    }
}