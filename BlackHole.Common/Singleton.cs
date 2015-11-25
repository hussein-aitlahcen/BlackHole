using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Common
{
    public class Singleton<T>
        where T : class, new()
    {
        private static T m_instance = new T();
        public static T Instance => m_instance;
    }
}
