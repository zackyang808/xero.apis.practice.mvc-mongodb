using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using xero.apis.practice.common.Contracts;
using Xero.NetStandard.OAuth2.Models;
using Xero.NetStandard.OAuth2.Token;

namespace xero.apis.practice.common.Models
{
    public class XeroToken : IXeroToken, IEntity
    {
        [BsonId]
        public string ObjectId { get; set; }
        public string XeroUserId { get; set; }
        public List<Tenant> Tenants { get ; set ; }
        public string AccessToken { get ; set ; }
        public string RefreshToken { get ; set ; }
        public string IdToken { get ; set ; }
        public DateTime ExpiresAtUtc { get ; set ; }
    }
}
