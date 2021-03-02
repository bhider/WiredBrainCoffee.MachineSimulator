using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiredBrainCoffee.EventHub.Sender.Model;

namespace WiredBrainCoffee.EventHub.Sender
{
   public interface ICoffeeMachineDataSender
    {
        Task SendDataAsync(CoffeeMachineData coffeeMachineData);
        Task SendDataAsync(IEnumerable<CoffeeMachineData> coffeeMachineDatas);
    }
    public class CoffeeMachineDataSender : ICoffeeMachineDataSender
    {
        private readonly EventHubClient  _eventHubClient;

        public CoffeeMachineDataSender(string eventHubConnectionString)
        {
            _eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString);
        }

        public async Task SendDataAsync(CoffeeMachineData coffeeMachineData)
        {
            EventData eventData = CreateEventData(coffeeMachineData);
            await _eventHubClient.SendAsync(eventData);
        }

       

        public async Task SendDataAsync(IEnumerable<CoffeeMachineData> coffeeMachineDatas)
        {
            var eventDatas = coffeeMachineDatas.Select(coffeeMachineData => CreateEventData(coffeeMachineData));
            
            var eventDataBatch = _eventHubClient.CreateBatch();

            foreach(var eventData in eventDatas)
            {
                if (!eventDataBatch.TryAdd(eventData))
                {
                    //if batch is full send what is currently batched and then create a new batch and add
                    //the eventData object that could not be added to the batch that was sent
                    await _eventHubClient.SendAsync(eventDataBatch.ToEnumerable());
                    eventDataBatch = _eventHubClient.CreateBatch();
                    eventDataBatch.TryAdd(eventData);
                }
            }
            ole
            if (eventDataBatch.Count > 0)
            {
                await _eventHubClient.SendAsync(eventDataBatch.ToEnumerable());
            }
        }

        private static EventData CreateEventData(CoffeeMachineData coffeeMachineData)
        {
            var dataAsJson = JsonConvert.SerializeObject(coffeeMachineData);
            var eventData = new EventData(Encoding.UTF8.GetBytes(dataAsJson));
            return eventData;
        }
    }
}
