using JPB.Communication.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.WinRT.Serilizer
{
    public class Serilizer
    {
        public Serilizer()
        {

        }

        public IEnumerable<Type> Get()
        {
            return Assembly
                .GetCallingAssembly()
                .GetTypes()
                .Where(s => typeof(IMessageSerializer).IsAssignableFrom(s));
        }

        public IEnumerable<string> GetNames()
        {
            return Get()
                .Select(s => s.Name);
        }
    }
}
