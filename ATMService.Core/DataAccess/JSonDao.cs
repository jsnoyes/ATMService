using ATMService.Core.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ATMService.Core.DataAccess
{
    public interface IJSonDao
    {
        IEnumerable<HistoryModel> Histories { get; }
        IEnumerable<DenominationModel> Denominations { get; }

        void Add(HistoryModel history);
        void Update(List<DenominationModel> denominations);
    }

    public class JSonDao : IJSonDao
    {
        private readonly string _denomFileLocation;
        private readonly string _historyFileLocation;

        public JSonDao(IConfiguration config)
        {
            var curDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _denomFileLocation = curDirectory + config["FileLocations:Denominations"];
            _historyFileLocation = curDirectory + config["FileLocations:Histories"];
        }

        public IEnumerable<HistoryModel> Histories => Parse<IEnumerable<HistoryModel>>(_historyFileLocation);

        public IEnumerable<DenominationModel> Denominations => Parse<IEnumerable<DenominationModel>>(_denomFileLocation);

        public void Update(List<DenominationModel> denominations)
        {
            Update(denominations, _denomFileLocation);
        }

        public void Add(HistoryModel history)
        {
            var histories = Histories.ToList();
            histories.Add(history);
            Update(histories, _historyFileLocation);
        }

        private void Update<T>(T obj, string fileLocation)
        {
            var serializedData = JsonConvert.SerializeObject(obj);
            File.WriteAllText(fileLocation, serializedData);
        }

        private T Parse<T>(string fileLocation)
        {
            var serializedData = File.ReadAllText(fileLocation);
            var deserializedData = JsonConvert.DeserializeObject<T>(serializedData);
            return deserializedData;
        }

    }
}
