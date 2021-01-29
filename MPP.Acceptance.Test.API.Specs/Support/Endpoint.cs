using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP.Acceptance.Test.API.Specs.Endpoints
{
    public sealed class Endpoint
    {
        private readonly string _name;
        private readonly int _value;

        public static readonly Endpoint Users = new Endpoint(1, "/api/users");
        public static readonly Endpoint Register = new Endpoint(2, "/api/register");
        public static readonly Endpoint Login = new Endpoint(2, "/api/login");
        public static readonly Endpoint Unknown = new Endpoint(2, "/api/unknown");

        private Endpoint(int value, string name)
        {
            _name = name;
            _value = value;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
