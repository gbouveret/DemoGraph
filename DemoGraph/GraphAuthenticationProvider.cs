using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using DemoGraph.Models;
using System.Diagnostics;

namespace Helpers
{
    public class GraphAuthenticationProvider : IAuthenticationProvider
    {
        #region Variables
        public static string GraphResourceId = "https://graph.microsoft.com/";
        public static string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        public static string ClientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        public static string TenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        public static string AADInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        public static string RedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static ClientCredential credential = new ClientCredential(ConfigurationManager.AppSettings["ida:ClientId"], ConfigurationManager.AppSettings["ida:ClientSecret"]);
        private string refreshToken = null;
        #endregion

        public GraphAuthenticationProvider()
        {
        }
        public GraphAuthenticationProvider(string refreshToken)
        {
            this.refreshToken = refreshToken;
        }

        public static async Task<string> GetAccessToken()
        {
            string signInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            string authority = AADInstance + TenantId;

            AuthenticationContext authContext = new AuthenticationContext(authority, new ADALTokenCache(signInUserId));

            string accessToken = HttpContext.Current.Session["AccessToken"] as string;
            #region redirect
            // if session has expired, access token is null and we need to re-authenticate
            if (string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var authResult = await authContext.AcquireTokenSilentAsync(GraphResourceId, new ClientCredential(ClientId, ClientSecret), new UserIdentifier(userObjectId, UserIdentifierType.UniqueId));
                    accessToken = authResult.AccessToken;
                    HttpContext.Current.Session["AccessToken"] = accessToken;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);

                    // Generate the parameterized URL for Azure login.
                    Uri authUri = authContext.GetAuthorizationRequestURL(
                        GraphResourceId,
                        ClientId,
                        new Uri(RedirectUri),
                        UserIdentifier.AnyUser,
                        null);
                    // Signout and re-signing
                    HttpContext.Current.GetOwinContext().Authentication.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
                }
            }
            #endregion
            return accessToken;
        }

        public static async Task<string> GetAccessTokenFromRefreshTokenAsync(string refreshToken)
        {
            //string authority = string.Format(System.Globalization.CultureInfo.InvariantCulture, aadInstance, "common");
            string authority = AADInstance + TenantId;

            AuthenticationContext authContext = new AuthenticationContext(authority, false);
            AuthenticationResult authResult = await authContext.AcquireTokenByRefreshTokenAsync(
                refreshToken,
                credential,
                GraphResourceId);
            return authResult.AccessToken;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            string token = null;
            if (!string.IsNullOrEmpty(refreshToken))
                token = await GetAccessTokenFromRefreshTokenAsync(refreshToken);
            else
                token = await GetAccessToken();

            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            request.Headers.Add("Authorization", "Bearer " + token);
        }
    }
}