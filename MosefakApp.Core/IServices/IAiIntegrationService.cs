using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MosefakApp.Core.IServices;
public interface IAiIntegrationService
{
    Task<string> AskAiAsync(string userQuestion);
}
