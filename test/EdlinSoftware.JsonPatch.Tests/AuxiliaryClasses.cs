using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdlinSoftware.JsonPatch.Tests
{
    internal class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    internal class PersonData
    {
        private readonly string _name;
        private readonly int _age;

        internal class PersonDataConverter : JsonConverter<PersonData>
        {
            public override PersonData ReadJson(
                JsonReader reader,
                Type objectType,
                PersonData existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
            {
                var jObject = JObject.Load(reader);

                var name = jObject.Value<string>("name");
                var age = jObject.Value<int>("age");

                return new PersonData(name, age);
            }

            public override void WriteJson(
                JsonWriter writer,
                PersonData value,
                JsonSerializer serializer)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                writer.WriteValue(value._name);
                writer.WritePropertyName("age");
                writer.WriteValue(value._age);
                writer.WriteEndObject();
            }
        }

        public PersonData(string name, int age)
        {
            _name = name;
            _age = age;
        }

        public override string ToString()
        {
            return GetStringPresentation(_name, _age);
        }

        public static string GetStringPresentation(string name, int age)
        {
            return $"Person '{name}' has age {age}";
        }
    }
}