using Common;
using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeechSvc
{
    public interface IStopOrderTable
    {
        bool AddStopOrder(StopOrder so, DateTime updateTime);
        bool UpdateStopOrder(long stopOrderNo, DateTime? endTime, StopOrderStatus status, DateTime updateTime);
        void Load();
        IEnumerable<StopOrder> GetStopOrders(int accountId);
    }

    public class StopOrderTable : IStopOrderTable
    {
        private Dictionary<long, StopOrder> _id_stoporder = null;
        private readonly IAccountDA _da = null;

        public StopOrderTable(IAccountDA da)
        {
            _da = da;
            _id_stoporder = new Dictionary<long, StopOrder>();
        }

        public bool AddStopOrder(StopOrder so, DateTime updateTime)
        {
            if (_id_stoporder.ContainsKey(so.StopOrderNo)) return false;
            _id_stoporder.Add(so.StopOrderNo, so);

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
            if (!_id_stoporder.ContainsKey(stopOrderNo)) return false;
            var so = _id_stoporder[stopOrderNo];
            so.EndTime = endTime;
            so.Status = status;
            if (status != StopOrderStatus.Active)
            {
                so.CompleteTime = updateTime;
            }

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

        public void Load()
        {
            _id_stoporder.Clear();
            var accounts = _da.GetAccounts();
            foreach (var acc in accounts)
            {
                var stopOrders = _da.GetStopOrders(acc.AccountID, true);
                foreach (var so in stopOrders)
                {
                    _id_stoporder.Add(so.StopOrderNo, so);
                }
            }
        }

        public IEnumerable<StopOrder> GetStopOrders(int accountId)
        {
            return _id_stoporder.Values.Where(r => r.AccountID == accountId).ToList();
        }
    }
}
