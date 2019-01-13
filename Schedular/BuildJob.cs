using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Quartz.Impl;
using Microsoft.Bot.Builder.Dialogs;

namespace LuisBot.Schedular
{
    public class BuildJob
    {
        public static object TriggerBuilderWithParameter { get; private set; }

        public static string TriggerImmediateBuildOnce(IDialogContext context, string jobName)
        {
            BuildTriggerJob.SetJobName(context, jobName);
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler sched = schedFact.GetScheduler().GetAwaiter().GetResult();
            sched.Start();

            IJobDetail job = JobBuilder.Create<BuildTriggerJob>()
                .WithIdentity("BuildOnceJob", "BuildGroup1")
                .Build();

            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
            .WithIdentity("BuildOnceJobTrigger", "BuildGroup1")
            .ForJob("BuildOnceJob", "BuildGroup")
            .Build();

            sched.ScheduleJob(job, trigger);

            return null;
        }

        public static string TriggerScheduleBuildWithParameterOnce(IDialogContext context, string jobName, string parameter, DateTime buildTime)
        {
            BuildTriggerJob.SetJobName(context, jobName);
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler sched = schedFact.GetScheduler().GetAwaiter().GetResult();
            sched.Start();

            IJobDetail job = JobBuilder.Create<BuildTriggerJobWithParameters>()
                .WithIdentity("BuildOnceJob", "BuildGroup2")
                .Build();

            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
            .WithIdentity("BuildOnceJobTrigger", "BuildGroup2")
            .StartAt(buildTime)
            .ForJob("BuildOnceJob", "BuildGroup")
            .Build();

            sched.ScheduleJob(job, trigger);

            return null;
        }

        // trigger builder creates simple trigger by default, actually an ITrigger is returned
        public static string TriggerScheduleBuildOnce(IDialogContext context, string jobName, DateTime buildTime)
        {
            BuildTriggerJob.SetJobName(context, jobName);
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler sched = schedFact.GetScheduler().GetAwaiter().GetResult();
            sched.Start();

            IJobDetail job = JobBuilder.Create<BuildTriggerJob>()
                .WithIdentity("BuildOnceJob", "BuildGroup3")
                .Build();

            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
            .WithIdentity("BuildOnceJobTrigger", "BuildGroup3")
            .StartAt(buildTime)
            .ForJob("BuildOnceJob", "BuildGroup")
            .Build();

            sched.ScheduleJob(job, trigger);

            return null;
        }

        public static string TriggerScheduleBuildMultipleTimes(IDialogContext context, string jobName, int count, int timerange)
        {
            BuildTriggerJob.SetJobName(context, jobName);
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler sched = schedFact.GetScheduler().GetAwaiter().GetResult();
            sched.Start();

            IJobDetail job = JobBuilder.Create<BuildTriggerJob>()
                .WithIdentity("BuildOnceJob", "BuildGroupMultiple")
                .Build();
            //cron expression 0 */15 * ? * * every 15 minutes

            ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("BuildRepeatJob", "BuildGroupMultiple")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(timerange).WithRepeatCount(count)) 
            .ForJob(job)                 
            .Build();
            return null;
        }
    }
}