using System.Threading;

namespace MPP.Acceptance.Test.API.Specs.Support.Session
{
    public class ObjectBag
    {
        private static readonly ThreadLocal<TestSessionVariables<object, object>> TestSessionThreadLocal = new();

        public static ISessionMap<object, object> GetCurrentSession()
        {
            return TestSessionThreadLocal.Value ??
                   (TestSessionThreadLocal.Value = new TestSessionVariables<object, object>());
        }

        public static T SessionVariableCalled<T>(object key)
        {
            GetCurrentSession().TryGetValue(key, out var o);
            return (T) o;
        }

        public static SessionVariableSetter SetSessionVariable(object key)
        {
            return new(key);
        }

        public static void ClearCurrentSession()
        {
            GetCurrentSession().Clear();
        }

        public static bool HasASessionVariableCalled(object key)
        {
            return GetCurrentSession().ContainsKey(key);
        }

        public class SessionVariableSetter
        {
            private readonly object _key;

            public SessionVariableSetter(object key)
            {
                _key = key;
            }

            public void To<T>(T value)
            {
                if (value != null)
                    GetCurrentSession().Add(_key, value);
                else
                    GetCurrentSession().Remove(_key);
            }
        }
    }
}