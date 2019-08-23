using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace AmiBroker.DataSources.IB
{
    [XmlRoot(Namespace = "AmiBroker.DataSources.IB", IsNullable = false)]
    public class IBConfiguration
    {
        public string Host;
        public int Port;
        public int ClientId;

        public bool AutoRefreshEnabled;
        public DateTime AutoRefreshTime;
        public int AutoRefreshDays;

        public bool VerboseLog;
        public bool BadTickFilter;
        public bool RthOnly;
        public bool SymbolUpdate;

        public static string GetConfigString(IBConfiguration configuration)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IBConfiguration));

            Stream stream = new MemoryStream();
            serializer.Serialize(XmlWriter.Create(stream), configuration);

            stream.Seek(0, SeekOrigin.Begin);
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        public static IBConfiguration GetConfigObject(string config)
        {
            // if no config string, set default values
            if (string.IsNullOrEmpty(config) || config.Trim().Length == 0)
                return GetDefaultConfigObject();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(IBConfiguration));
                Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(config));

                return (IBConfiguration)serializer.Deserialize(stream);
            }
            catch (Exception ex)
            {
                LogAndMessage.Log(MessageType.Error, "Configuration error:" + ex);
                return GetDefaultConfigObject();
            }
        }

        public static IBConfiguration GetDefaultConfigObject()
        {
            IBConfiguration defConfig = new IBConfiguration();

            defConfig.Host = "127.0.0.1";
            defConfig.Port = 7496;
            defConfig.ClientId = 0;

            defConfig.AutoRefreshEnabled = true;
            defConfig.AutoRefreshTime = DateTime.Now.Date.AddHours(1);
            defConfig.AutoRefreshDays = 1;

            defConfig.RthOnly = false;
            defConfig.BadTickFilter = true;
            defConfig.SymbolUpdate = true;
#if DEBUG
            defConfig.VerboseLog = true;
#else
            defConfig.VerboseLog = false;
#endif
            return defConfig;
        }
    }
}
