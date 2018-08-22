using Common;
using LeechSvc.Logger;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LeechSvc.Bots
{
    public interface IBotManager
    {
        void Initialize();
        void Close();
    }

    public class BotManager : IBotManager
    {
        private readonly IBotsConfiguration _botsConfig;
        private readonly ITickDispatcher _tickDispatcher = null;
        private readonly ILogger _logger = null;
        private Dictionary<string, IBot> _key_bots;
        private Dictionary<IBot, ILeechPlatform> _bot_platform;

        public BotManager(IBotsConfiguration botsConfig, ITickDispatcher tickDisp, ILogger logger)
        {
            _botsConfig = botsConfig ?? throw new ArgumentNullException("botsConfig");
            _tickDispatcher = tickDisp;
            _logger = logger;
            _key_bots = new Dictionary<string, IBot>();
            _bot_platform = new Dictionary<IBot, ILeechPlatform>();
        }

        public void Initialize()
        {
            _logger.AddInfo("BotManager", "Initialize ...");

            _key_bots.Clear();
            _bot_platform.Clear();
            var keys = _botsConfig.GetBotKeys();
            foreach (var key in keys)
            {
                var conf = _botsConfig.GetBotConfig(key);
                if (conf == null) continue;

                try
                {
                    var asm = Assembly.LoadFrom(conf.Assembly);
                    if (asm == null) continue;

                    var type = asm.GetType(conf.Class);
                    if (type == null) continue;

                    var platform = IoC.Resolve<ILeechPlatform>();
                    var bot = Activator.CreateInstance(type, platform) as IBot;
                    if (bot == null) continue;

                    bot.Initialize(conf.InitData);

                    // после успешной инициализации бота
                    _bot_platform.Add(bot, platform);
                    _key_bots.Add(key, bot);
                    _logger.AddInfo("BotManager", "Bot '" + key + "' initialized");
                }
                catch (Exception ex)
                {
                    _logger.AddException("BotManager", ex);
                }
            }
            _logger.AddInfo("BotManager", "Initialized");
        }

        public void Close()
        {
            _logger.AddInfo("BotManager", "Closing ...");
            foreach (var bot in _key_bots.Values)
            {
                bot.Close();
                _bot_platform[bot].Close();
            }
            _logger.AddInfo("BotManager", "Closed");
        }


    }
}
