using Azure.Messaging.ServiceBus;
using CPReservationApi.Common;
using CPReservationApi.DAL;
using CPReservationApi.WebApi.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.Services
{
    public class ServiceBusQueueService
    {
        private readonly string queueName = "rsvpqueue";

        public ServiceBusSender CreateServiceBusSender(string queueName, string sbConnectionString)
        {
            // Retrieve storage account information from connection string.
            ServiceBusClient queueClient = new ServiceBusClient(sbConnectionString);

            ServiceBusSender queue = queueClient.CreateSender(queueName);

            return queue;
        }

        public async Task SendMessage(AppSettings appsettings, string data)
        {
            EmailServiceDAL emailDAL = new EmailServiceDAL(Common.Common.ConnectionString);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            var queueModel = new EmailQueue();
            queueModel = JsonConvert.DeserializeObject<EmailQueue>(data);
            logDAL.InsertLog("WebApi", "SendMessage Service Bus, data:" + data, "", 3, 0);
            var messageSender = CreateServiceBusSender(appsettings.ServiceBusQueueName, appsettings.ServiceBusConnectionString);

            ServiceBusMessage message = new ServiceBusMessage(data);

            await messageSender.SendMessageAsync(message);
        }
    }
}
