using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRice.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<string> GetAccessTokenFromRefreshTokenAsync(CancellationToken cancellationToken);
    }
}