using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using System.Collections.Generic;

namespace WasteManagement
{
    public static class WasteAlert
    {
        static BrokeredMessage bm = new BrokeredMessage();
        static string sbCS = "Endpoint=sb://glovebox-01.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=foVgle9ymABV8+IDved6uAoNdO/KRhRNNH1pAXXCbJ4=";
        static string queueName = "waste-management";

        [FunctionName("WasteAlert")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var qc = QueueClient.CreateFromConnectionString(sbCS, queueName);
            List<Guid> lockTokens = new List<Guid>();

            bm = qc.Receive(new TimeSpan(0, 0, 2));

            while (bm != null)
            {
                var msg = bm.GetBody<String>();
                log.Info(msg);
                lockTokens.Add(bm.LockToken);
                bm = qc.Receive(new TimeSpan(0, 0, 2));
            }

            if (lockTokens.Count != 0)
            {
                qc.CompleteBatch(lockTokens);
            }
        }
    }
}