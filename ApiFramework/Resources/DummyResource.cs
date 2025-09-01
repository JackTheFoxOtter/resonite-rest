using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFramework.Resources
{
    internal class DummyResource : ApiResource
    {
        protected override ApiPropertyInfo[] GetPropertyInfos()
        {
            throw new NotImplementedException();
        }
    }
}
