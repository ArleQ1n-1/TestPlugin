using System;
using System.Xml.Serialization;

namespace Resto.Front.Api.TestPlugin.Config
{
    [XmlRoot("config")]
    public sealed class Config
    {
        [XmlElement]
        public Guid ClientId { get; set; } = Guid.NewGuid();

        public static Config CreateDefault()
        {
            return new Config();
        }
    }
}