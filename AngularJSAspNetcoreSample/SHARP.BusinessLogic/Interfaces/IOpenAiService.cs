using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IOpenAIService
    {
        Task<Dictionary<string, List<Dictionary<string, object>>>> Search(Dictionary<string, List<Dictionary<string, string>>> wordIndex);
    }
}
