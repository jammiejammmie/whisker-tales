using System;
using System.Collections.Generic;
using WhiskerTales.Core;

namespace WhiskerTales.Runtime
{
    /// <summary>
    /// Typed service map — V2 시스템 간 의존성 주입의 표준 채널.
    /// 기존 v1 SaveService/AudioService 등은 LegacyServiceBridge로 어댑팅 후 등록.
    /// </summary>
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                DebugLogger.Warning(LogCategory.UI, "ServiceRegistry.Register: null for " + typeof(T).Name);
                return;
            }

            services[typeof(T)] = service;
            DebugLogger.Info(LogCategory.UI, "Service registered: " + typeof(T).Name);
        }

        public T Get<T>() where T : class
        {
            if (services.TryGetValue(typeof(T), out object svc) == true)
            {
                return svc as T;
            }

            return null;
        }

        public bool TryGet<T>(out T service) where T : class
        {
            service = Get<T>();
            return service != null;
        }

        public bool Has<T>() where T : class
        {
            return services.ContainsKey(typeof(T));
        }
    }
}
