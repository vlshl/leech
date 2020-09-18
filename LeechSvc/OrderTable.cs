using Common;
using Common.Data;
using Common.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace LeechSvc
{
    public interface IOrderTable
    {
        bool AddOrder(Order order);
        bool UpdateOrder(long orderNo, OrderStatus status);
        Order GetOrder(long orderNo);
        IEnumerable<Order> GetOrders(int accountId, int idFrom);
        IEnumerable<Order> GetOrdersByIds(int[] ids);
    }

    public class OrderTable : IOrderTable
    {
        private readonly IAccountDA _da = null;

        public OrderTable(IAccountDA da)
        {
            _da = da;
        }

        public bool AddOrder(Order order)
        {
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
            return _da.GetOrder(orderNo);
        }

        public bool UpdateOrder(long orderNo, OrderStatus status)
        {
            var db_order = _da.GetOrder(orderNo);
            if (db_order != null)
            {
                db_order.Status = status;
                _da.UpdateOrder(db_order);
            }

            return true;
        }

        public IEnumerable<Order> GetOrders(int accountId, int idFrom)
        {
            return _da.GetOrders(accountId, false, idFrom).ToList();
        }

        public IEnumerable<Order> GetOrdersByIds(int[] ids)
        {
            return _da.GetOrdersByIds(ids).ToList();
        }
    }
}
