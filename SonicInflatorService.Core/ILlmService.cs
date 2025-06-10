using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicInflatorService.Core
{
    public interface ILlmService
    {
        Task<string> GenerateResponseAsync(string prompt);
    }
}
