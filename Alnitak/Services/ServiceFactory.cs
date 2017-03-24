using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alnitak.Services
{
    public class ServiceFactory : IServiceFactory
    {
        private static ServiceFactory defaultServiceFactory = null;
        public static ServiceFactory DefaultServiceFactory
        {
            get
            {
                if (defaultServiceFactory == null)
                {
                    defaultServiceFactory = new ServiceFactory();
                }
                return defaultServiceFactory;
            }
        }

        public T GetService<T>() where T : class
        {
            Type type = typeof(T);
            if (type == typeof(ISettings))
            {
                return Properties.Settings.Default as T;
            }            
            else
            {
                throw new NotImplementedException(typeof(T).Name);
            }
        }



    }
}
