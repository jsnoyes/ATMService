using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ATMService.Services.Shared.DataAccess
{
    public interface IJSonDao<T>
    {
        IEnumerable<T> Values { get; }

        void Add(T value);
        void Update(List<T> value);
    }

    public class JSonDao<T> : IJSonDao<T>
    {
        private readonly string _fileLocation;

        public JSonDao(IConfiguration config)
        {
            var curDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _fileLocation = curDirectory + config["FileLocations:Value"];
        }

        public IEnumerable<T> Values => Parse<IEnumerable<T>>(_fileLocation);

        public void Update(List<T> values)
        {
            Update(values, _fileLocation);
        }

        public void Add(T newValue)
        {
            var values = Values.ToList();
            values.Add(newValue);
            Update(values, _fileLocation);
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
