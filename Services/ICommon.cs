using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apidemo.Services
{
    public interface ICommon
    {
        float Verify(string id);
        void Initialize();
        float CreateEncounter();
        object CreateCompareWorkflow();
    }
}
