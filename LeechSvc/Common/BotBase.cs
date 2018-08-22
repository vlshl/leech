using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeechSvc
{
    public interface IBot
    {
        void Initialize(string data);
        void Close();
    }

    public class BotBase : IBot
    {
        public virtual void Initialize(string data) { }
        public virtual void Close() { }
    }
}
