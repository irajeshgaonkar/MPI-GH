using HCA.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Core.Services
{
    public interface IServiceAccountService
    {
        Task<ServiceAccount?> GetServiceAccount(string appId);
        Task<IEnumerable<ServiceAccount?>> GetServiceAccounts();
    }
}
