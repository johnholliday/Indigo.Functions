﻿using Indigo.Functions.Unity;
using Unity;

namespace UnityFunctionSample
{
    public class DependencyInjectionConfig : IDependencyConfig
    {
        public void RegisterComponents(UnityContainer container)
        {
            container.RegisterType<ICache, CacheProvider>();
            container.RegisterType<IStorageAccess, StorageAccess>();
            container.RegisterType<ITableAccess, CloudTableAccess>();
        }
    }
}