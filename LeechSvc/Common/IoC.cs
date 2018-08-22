using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class IoC
    {
        static IoC()
        {
        }

        public static IContainer Container { get; set; }

        public static T Resolve<T>() where T : class
        {
            if (Container == null) return null;
            return Container.Resolve<T>();
        }

        public static object Resolve(Type type)
        {
            if (Container == null) return null;
            return Container.Resolve(type);
        }
    }
}
