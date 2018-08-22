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
    public interface IOrderTable
    {
        bool AddOrder(Order order);
        bool UpdateOrder(long orderNo, OrderStatus status);
        Order GetOrder(long orderNo);
        void Load();
    }

    public class OrderTable : IOrderTable
    {
        private Dictionary<long, Order> _id_order = null;
        private readonly IAccountDA _da = null;

        public OrderTable(IAccountDA da)
        {
            _da = da;
            _id_order = new Dictionary<long, Order>();
        }

        public bool AddOrder(Order order)
        {
            if (_id_order.ContainsKey(order.OrderNo)) return false;
            _id_order.Add(order.OrderNo, order);

            var db_order = _da.GetOrder(order.OrderNo);
            if (db_order == null)
            {
                _da.InsertOrder(order);
            }
            else
            {
                order.OrderID = db_order.OrderID;
                _da.UpdateOrder(order);
            }

            return true;
        }

        public Order GetOrder(long orderNo)
        {
            if (!_id_order.ContainsKey(orderNo)) return null;
            return _id_order[orderNo];
        }

        public bool UpdateOrder(long orderNo, OrderStatus status)
        {
            if (!_id_order.ContainsKey(orderNo)) return false;
            var so = _id_order[orderNo];
            so.Status = status;

            var db_order = _da.GetOrder(orderNo);
            if (db_order != null)
            {
                db_order.Status = status;
                _da.UpdateOrder(db_order);
            }

            return true;
        }

        public void Load()
        {
            _id_order.Clear();
            var accounts = _da.GetAccounts();
            foreach (var acc in accounts)
            {
                var orders = _da.GetOrders(acc.AccountID, true);
                foreach (var ord in orders)
                {
                    _id_order.Add(ord.OrderNo, ord);
                }
            }
        }
    }
}
