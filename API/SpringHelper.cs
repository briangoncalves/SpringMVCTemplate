using Spring.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API
{
    public class SpringHelper : IApplicationContextAware
    {
        private static IApplicationContext context;

        public IApplicationContext ApplicationContext
        {
            get
            {
                return context;
            }
            set
            {
                context = value as IApplicationContext;
            }
        }

        public static object GetObject(string name)
        {
            return context.GetObject(name);
        }

        public static T GetObjectOfType<T>(string name)
        {
            T springObject = (T)GetObject(name);
            if (springObject == null)
            {
                throw new Exception(String.Format("The spring object '{0}' was found but was not the expected type '{1}'.", name, typeof(T)));
            }
            return springObject;
        }
    }
}