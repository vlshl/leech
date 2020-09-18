using Common;
using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeechSvc
{
    public interface IStopOrderTable
    {
        bool AddStopOrder(StopOrder so, DateTime updateTime);
        bool UpdateStopOrder(long stopOrderNo, DateTime? endTime, StopOrderStatus status, DateTime updateTime);
        IEnumerable<StopOrder> GetStopOrders(int accountId, int idFrom);
        IEnumerable<StopOrder> GetStopOrdersByIds(int[] ids);
    }

    public class StopOrderTable : IStopOrderTable
    {
        private readonly IAccountDA _da = null;

        public StopOrderTable(IAccountDA da)
        {
            _da = da;
        }

        public bool AddStopOrder(StopOrder so, DateTime updateTime)
        {
            var db_stopOrder = _da.GetStopOrder(so.StopOrderNo);
            if (db_stopOrder == null)
            {
                if (so.Status != StopOrderStatus.Active)
                {
                    so.CompleteTime = updateTime;
                }
                _da.InsertStopOrder(so);
            }
            else
            {
                so.StopOrderID = db_stopOrder.StopOrderID;
                so.CompleteTime = db_stopOrder.CompleteTime;
                if (so.Status != StopOrderStatus.Active && so.CompleteTime == null)
                {
                    so.CompleteTime = updateTime;
                }
                _da.UpdateStopOrder(so);
            }

            return true;
        }

        public bool UpdateStopOrder(long stopOrderNo, DateTime? endTime, StopOrderStatus status, DateTime updateTime)
        {
            var db_stopOrder = _da.GetStopOrder(stopOrderNo);
            if (db_stopOrder != null)
            {
                db_stopOrder.Status = status;
                db_stopOrder.EndTime = endTime;
                if (status != StopOrderStatus.Active && db_stopOrder.CompleteTime == null)
                {
                    db_stopOrder.CompleteTime = updateTime;
                }
                _da.UpdateStopOrder(db_stopOrder);
            }

            return true;
        }

        public IEnumerable<StopOrder> GetStopOrders(int accountId, int idFrom)
        {
            return _da.GetStopOrders(accountId, false, idFrom).ToList();
        }

        public IEnumerable<StopOrder> GetStopOrdersByIds(int[] ids)
        {
            return _da.GetStopOrdersByIds(ids).ToList();
        }
    }
}
