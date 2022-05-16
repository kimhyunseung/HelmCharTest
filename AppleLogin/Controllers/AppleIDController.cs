using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AppleLogin.Controllers
{
    public class AppleIDController : Controller
    {
        // GET: AppleID
        public ActionResult Callback(string code, string state, string user, string error)
        {
            //Apple Response Data
            var oauthCode = code;
            var scopeData = user;
            var webzenData = state;

            StringBuilder sb = new StringBuilder();
            foreach(var item in Request.ServerVariables.AllKeys)
            {
                sb.AppendLine(Request.ServerVariables[item]);
            }
            string sbb = sb.ToString();

            StringBuilder sb2 = new StringBuilder();
            foreach(var item in Request.QueryString.AllKeys)
            {
                sb2.AppendLine(Request.QueryString[item]);
            }
            string sbb2 = sb2.ToString();

            //Required
            var clientId = "com.webzen.mocaa.nativesample.signin";      //Native
            //var clientId = "com.webzen.mocaa.unitysample";              //Unity
            var clientSecret = "";
            var grantType = "authorization_code";       //Generate&Validate : authorization_code, Refresh : refresh_token
            var redirectUrl = "https://alpha-kr-mapi.webzen.com/v2/account/appleid/callback";

            string res = string.Empty;
            string idToken = string.Empty,
                accessToken = string.Empty,
                refreshToken = "ra593e4d0508a483e9856d407252a5378.0.mvy._ndWMiQTdefjr1eweyUlIg",
                tokenType = string.Empty;
            long expiresIn = 0;

            #region Genrate token string with JWT

            var key = "MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQg7hRw0ZIL3+gsafgt"
                + "S98r3k/BJSmwX2ouIFKnJabpVlygCgYIKoZIzj0DAQehRANCAATQ8MySnoEucEVB"
                + "3kZ7jQmEwTQxCPr929u/+gawIFSbsziLp2eoWlPNkPEGvUUdHGmVaKjjBVRyomLJ"
                + "0fOfakGY";     //Native
            //var key = "MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQgdP03wzoMm5PeXlCB"
            //    + "aqyDSoshW2QcHytk6+fK/EjY/nOgCgYIKoZIzj0DAQehRANCAATbMQC2DQNupDK8"
            //    + "diqmRPlHqEC3Hhno2SlTtuE+qNyYpMCoYXKC15ICAqDljqLOrRN6/1U2swxLCimX"
            //    + "y+G5p2rN";       //Unity
            var keyByte = Convert.FromBase64String(key);

            CngKey secKey = CngKey.Import(keyByte, CngKeyBlobFormat.Pkcs8PrivateBlob);
            var ecdsaKey = new ECDsaSecurityKey(new ECDsaCng(secKey));
            ecdsaKey.KeyId = "FN572QCZ55";

            IdentityModelEventSource.ShowPII = true;

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new Claim("sub", clientId),
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = "U473ZJ736G",
                IssuedAt = DateTime.UtcNow,
                Audience = "https://appleid.apple.com",
                SigningCredentials = new SigningCredentials(ecdsaKey, SecurityAlgorithms.EcdsaSha256),
                CompressionAlgorithm = SecurityAlgorithms.EcdsaSha256,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.CreateJwtSecurityToken(descriptor);
            clientSecret = tokenHandler.WriteToken(jwtToken);

            #endregion

            #region Get Tokens

            using (HttpClient client = new HttpClient())
            {
                var data = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("code", oauthCode),
                    new KeyValuePair<string, string>("grant_type", grantType),
                    new KeyValuePair<string, string>("redirect_uri", redirectUrl)
                });

                var result = client.PostAsync("https://appleid.apple.com/auth/token", data).Result;
                res = result.Content.ReadAsStringAsync().Result;

                var resData = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                idToken = resData["id_token"];
                accessToken = resData["access_token"];
                refreshToken = resData["refresh_token"];
                tokenType = resData["token_type"];
                expiresIn = long.Parse(resData["expires_in"]);
            }

            #endregion

            #region Validation Token

            Validate(idToken);

            #endregion

            #region RefreshToken

            grantType = "refresh_token";
            using (HttpClient client = new HttpClient())
            {
                var data = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("code", null),
                    new KeyValuePair<string, string>("grant_type", grantType),
                    new KeyValuePair<string, string>("redirect_uri", redirectUrl),
                    new KeyValuePair<string, string>("refresh_token", refreshToken)
                });

                var result = client.PostAsync("https://appleid.apple.com/auth/token", data).Result;
                res = result.Content.ReadAsStringAsync().Result;

                var resData = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                accessToken = resData["access_token"];
                expiresIn = long.Parse(resData["expires_in"]);
            }

            #endregion

            return Json(new { idToken = idToken, accessToken = accessToken, refreshToken = refreshToken, tokenType= tokenType, expiresIn = expiresIn }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Validate(string id_token)
        {
            string alg = string.Empty,
                kid = string.Empty,
                iss = string.Empty,
                aud = string.Empty,
                nonce = string.Empty,
                at_hash = string.Empty;
            DateTime exp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                iat = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            string result = string.Empty;

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtInfo = tokenHandler.ReadJwtToken(id_token);
            alg = jwtInfo.SignatureAlgorithm;
            kid = jwtInfo.Header.Kid;
            iss = jwtInfo.Issuer;
            aud = jwtInfo.Audiences.FirstOrDefault();
            nonce = jwtInfo.Claims.Where(x => x.Type.Equals("nonce")).FirstOrDefault().Value;
            exp = exp.AddSeconds(long.Parse(jwtInfo.Claims.Where(x => x.Type.Equals("exp")).FirstOrDefault().Value));
            iat = iat.AddSeconds(long.Parse(jwtInfo.Claims.Where(x => x.Type.Equals("iat")).FirstOrDefault().Value));
            at_hash = jwtInfo.Claims.Where(x => x.Type.Equals("at_hash")).FirstOrDefault().Value;

            using(var client = new HttpClient())
            {
                var res = client.GetStringAsync("https://appleid.apple.com/auth/keys").Result;
                var appleInfo = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(res)["keys"];

                if(!appleInfo.Where(x => x["alg"].Equals(alg) && x["kid"].Equals(kid)).Any())
                {
                    result += "APPLE 공개키와 동일한 암호알고리즘/키ID가 아님";
                }
            }

            if(DateTime.UtcNow >= exp)
            {
                result += "토큰 만료";
            }

            if (!aud.Equals("com.webzen.mocaa.nativesample.signin"))
            {
                result += "발급자 다름";
            }

            return Json(new { result = result, alg = alg, kid = kid, iss = iss, aud = aud, nonce = nonce, exp = exp, iat = iat }, JsonRequestBehavior.AllowGet);
        }
    }
}