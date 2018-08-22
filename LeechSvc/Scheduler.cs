﻿using LeechSvc.Logger;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeechSvc
{
    /// <summary>
    /// Планировщик заданий.
    /// При инициализации нужно добавить запланированные действия по времени.
    /// В одну секунду может быть выполнено не более одного действия.
    /// Действия будут выполняться в указанное для них время каждый день.
    /// </summary>
    public class Scheduler
    {
        private bool _isWorking = false;
        private readonly ILeechConfig _config = null;
        private readonly ILeechApp _app = null;
        private readonly ILogger _logger = null;
        private Dictionary<int, Action> _time_action = null;

        public Scheduler(ILeechConfig config, ILeechApp app, ILogger logger)
        {
            _config = config ?? throw new ArgumentNullException("config");
            _app = app ?? throw new ArgumentNullException("app");
            _logger = logger ?? throw new ArgumentNullException("logger");
            _time_action = new Dictionary<int, Action>();
        }

        /// <summary>
        /// Запуск планировщика
        /// </summary>
        public void Start()
        {
            _isWorking = true;
            Thread thread = new Thread(new ThreadStart(DoWork));
            thread.Start();
        }

        /// <summary>
        /// Остановка планировщика
        /// </summary>
        public void Stop()
        {
            _isWorking = false;
        }

        /// <summary>
        /// Добавить запланированное действие
        /// </summary>
        /// <param name="time">Время действия в формате hhmmss (hours * 10000 + minutes * 100 + seconds)</param>
        /// <param name="action">Действие</param>
        /// <returns>true - успешно добавлено, false - время занято (в одну секунду может быть не более одного действия)</returns>
        public bool AddItem(int time, Action action)
        {
            lock (_time_action)
            {
                if (_time_action.ContainsKey(time)) return false;
                _time_action.Add(time, action);
            }
            return true;
        }

        /// <summary>
        /// Удалить запланированное действие
        /// </summary>
        /// <param name="time">Время действия</param>
        public void RemoveItem(int time)
        {
            lock (_time_action)
            {
                if (!_time_action.ContainsKey(time)) return;
                _time_action.Remove(time);
            }
        }

        /// <summary>
        /// Очистить полностью список запланированных действий
        /// </summary>
        public void ClearAllItems()
        {
            lock (_time_action)
            {
                _time_action.Clear();
            }
        }

        private void DoWork()
        {
            int lasttime = -1;
            _logger.AddInfo("Scheduler", "Working");
            while (_isWorking)
            {
                var now = DateTime.Now;
                int curtime = now.Hour * 10000 + now.Minute * 100 + now.Second;
                if (lasttime == curtime)
                {
                    Thread.Sleep(200);
                    continue;
                }
                lasttime = curtime;

                lock (_time_action)
                {
                    foreach (var time in _time_action.Keys)
                    {
                        if (time != curtime) continue;
                        Task.Factory.StartNew(() => 
                        {
                            try
                            {
                                _time_action[time].Invoke();
                            }
                            catch (Exception ex)
                            {
                                _logger.AddException("Scheduler", ex);
                            }
                        });
                    }
                }
            }
            _logger.AddInfo("Scheduler", "Stop work");
        }
    }
}
