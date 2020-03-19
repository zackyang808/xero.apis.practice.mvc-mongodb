using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Token;

namespace xero.apis.practice.common.Contracts
{
    public interface ITokenService
    {
        Task<IXeroToken> GetAccessTokenAsync(string xeroUserId);

        Task SetToken(IXeroToken xeroToken);
    }
}
