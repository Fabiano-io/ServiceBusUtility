using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceBusUtility.Servicebus
{
    public class ServiceBus
    {        
        public string endPoint { get; set; }

        public string topic { get; set; }

        public string labelFilter { get; set; }

        public void SendMessages(dynamic message)
        {
            try
            {
                ITopicClient _topicClient = new TopicClient(endPoint, topic);

                var msg = new Message(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message)))
                {
                    Label = labelFilter,
                    MessageId = Guid.NewGuid().ToString(),
                };

                _topicClient.SendAsync(msg);

                _topicClient.CloseAsync();
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
    }
}
