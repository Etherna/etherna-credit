﻿using Etherna.EthernaCredit.Areas.Api.DtoModels;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Api.Services
{
    public interface IUserControllerService
    {
        Task<double> GetCreditAsync(ClaimsPrincipal user);
        Task<IEnumerable<LogDto>> GetLogsAsync(ClaimsPrincipal user, int page, int take);
    }
}