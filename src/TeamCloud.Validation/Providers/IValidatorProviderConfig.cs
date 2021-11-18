using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TeamCloud.Validation.Providers
{
    public interface IValidatorProviderConfig
    {
        public IValidatorProviderConfig Register(Assembly assembly);
    }
}
