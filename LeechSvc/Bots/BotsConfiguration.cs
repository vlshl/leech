using LeechSvc.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LeechSvc.Bots
{
    public interface IBotsConfiguration
    {
        void Load();
        BotConfig GetBotConfig(string key);
        string[] GetBotKeys();
    }

    public class BotsConfiguration : IBotsConfiguration
    {
        private readonly ILeechConfig _config = null;
        private readonly ILogger _logger = null;
        private Dictionary<string, BotConfig> _key_botconf;

        public BotsConfiguration(ILeechConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
            _key_botconf = new Dictionary<string, BotConfig>();
        }

        public void Load()
        {
            _logger.AddInfo("BotsConfiguration", "Loading bots configuration ...");
            _key_botconf.Clear();

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(_config.GetBotsConfigPath());

                foreach (XmlNode xn in xDoc.DocumentElement.ChildNodes)
                {
                    if (xn.Name != "Bot")
                    {
                        _logger.AddInfo("BotsConfiguration", "Skip node: " + xn.Name);
                        continue;
                    }

                    var xa_key = xn.Attributes["Key"];
                    if (xa_key == null)
                    {
                        _logger.AddError("BotsConfiguration", "Key attribute not found.");
                        continue;
                    }
                    string key = xa_key.Value;

                    var xa_assembly = xn.Attributes["Assembly"];
                    if (xa_assembly == null)
                    {
                        _logger.AddError("BotsConfiguration", "Assembly attribute not found.");
                        continue;
                    }
                    string assembly = xa_assembly.Value;
                    if (!Path.IsPathRooted(assembly))
                    {
                        assembly = Path.Combine(_config.GetBotsPath(), assembly);
                    }

                    var xa_class = xn.Attributes["Class"];
                    if (xa_class == null)
                    {
                        _logger.AddError("BotsConfiguration", "Class attribute not found.");
                        continue;
                    }
                    string cls = xa_class.Value;

                    var xa_initdata = xn.Attributes["InitData"];
                    string initdata = "";
                    if (xa_initdata != null)
                    {
                        initdata = xa_initdata.Value;
                    }
                    else
                    {
                        _logger.AddInfo("BotsConfiguration", "InitData attribute not found. Empty string used.");
                    }

                    if (_key_botconf.ContainsKey(key))
                    {
                        _logger.AddError("BotsConfiguration", "Duplicate key: " + key);
                        continue;
                    }

                    _key_botconf.Add(key, new BotConfig()
                    {
                        Assembly = assembly,
                        Class = cls,
                        InitData = initdata,
                        Key = key
                    });
                    _logger.AddInfo("BotsConfiguration", 
                        string.Format("Bot config load: Key={0}, Assembly={1}, Class={2}", key, assembly, cls));
                }
            }
            catch (FileNotFoundException ex)
            {
                _logger.AddError("BotsConfiguration", "Bots config file not found");
            }
            catch (Exception ex)
            {
                _logger.AddException("BotsConfiguration", ex);
            }
        }

        public BotConfig GetBotConfig(string key)
        {
            if (!_key_botconf.ContainsKey(key)) return null;
            return _key_botconf[key];
        }

        public string[] GetBotKeys()
        {
            return _key_botconf.Keys.ToArray();
        }
    }
}
