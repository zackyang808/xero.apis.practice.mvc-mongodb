using MongoDB.Bson;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using xero.apis.practice.common.Contracts;
using xero.apis.practice.common.Models;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Token;

namespace xero.apis.practice.common.Services
{
    public class TokenService : ITokenService
    {
        private readonly IRepository<XeroToken> _tokenRepository;
        private readonly IXeroClient _xeroClient;

        public TokenService(IRepository<XeroToken> tokenRepository, IXeroClient xeroClient)
        {
            _tokenRepository = tokenRepository;
            _xeroClient = xeroClient;
        }

        public async Task<IXeroToken> GetAccessTokenAsync(string xeroUserId)
        {
            var tokens = await _tokenRepository.Get(t => t.XeroUserId == xeroUserId, 0, 1);
            var tokenSaved = tokens.FirstOrDefault();
            if (tokenSaved == null)
            {
                return null;
            }

            var token = await _xeroClient.GetCurrentValidTokenAsync((IXeroToken)tokenSaved);

            await SetToken(token);

            return token;
        }

        public async Task SetToken(IXeroToken xeroToken)
        {
            try
            {
                var currentToken = (XeroToken)xeroToken;
                var tokens = await _tokenRepository.Get(t => t.XeroUserId == currentToken.XeroUserId, 0, 1);
                var tokenSaved = tokens.FirstOrDefault();

                if (tokenSaved == null)
                {
                    currentToken.ObjectId = ObjectId.GenerateNewId().ToString();
                    await _tokenRepository.Add(currentToken);
                }
                else
                {
                    tokenSaved.AccessToken = currentToken.AccessToken;
                    tokenSaved.RefreshToken = currentToken.RefreshToken;
                    tokenSaved.ExpiresAtUtc = currentToken.ExpiresAtUtc;

                    await _tokenRepository.Update(tokenSaved);
                }
            }
            catch (Exception ex)
            {
                throw ex; //or log here
            }
        }
    }
}
