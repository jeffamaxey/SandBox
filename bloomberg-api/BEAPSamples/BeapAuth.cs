/// <summary>
/// Copyright 2020. Bloomberg Finance L.P. Permission is hereby granted, free of
/// charge, to any person obtaining a copy of this software and associated
/// documentation files (the "Software"), to deal in the Software without
/// restriction, including without limitation the rights to use, copy, modify,
/// merge, publish, distribute, sublicense, and/or sell copies of the Software,
/// and to permit persons to whom the Software is furnished to do so, subject to
/// the following conditions: The above copyright notice and this permission
/// notice shall be included in all copies or substantial portions of the
/// Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// </summary>

using System;
using System.IO;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using Serilog;

namespace BEAPSamples
{
    /// <summary>
    /// The following class represents BEAP credentials and provides method for
    /// loading BEAP credentials from file (typically called credential.txt).
    /// credential.txt is generated for each account and needed to authorize on
    /// BEAP HTTP service.
    /// Note: Please obtain credential.txt file first before launching any sample.
    /// </summary>
    class Credential
    {
        // 1970-Jan-01 00:00:00 UTC
        private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        // accout region, use 'default' value unless being instructed otherwise
        private const string d_region = "default";

        // Once created each token will be valid only for the following specified period(in seconds).
        // This value is ajustable on client side, but note that increasing this value too much 
        // might not suffice for security concerns, as well as too little value could lead to 
        // JWT token invalidation before it being received by the beap server due to network delays.
        // You can adjust this value for your need or use default value unless you definitely know 
        // that you need to change this value.
        private const int d_lifetime = 25;

        /// <summary>
        /// Provides access to client id parsed from credential.txt file.
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        /// <summary>
        /// Accepts client secret form json deserializer and decodes it to bytes.
        /// </summary>
        [JsonProperty("client_secret")]
        public string ClientSecret { set { DecodedSecret = FromHexString(value); } }

        /// <summary>
        /// Provides access to decoded client secret.
        /// </summary>
        public byte[] DecodedSecret { get; private set; }

        /// <summary>
        /// Accepts the expiration date from a json deserializer and converts it to a convenient representation.
        /// </summary>
        /// <value>The expiration date as a timestamp in milliseconds.</value>
        [JsonProperty("expiration_date")]
        public long ExpirationDate { set { ExpiresAt = UnixEpoch.AddMilliseconds(value); } }

        /// <summary>
        /// Provides access to the expiration time.
        /// </summary>
        /// <value>The expiration date.</value>
        public DateTimeOffset ExpiresAt { get; private set; }

        /// <summary>
        /// Converts hexadecimal string to bytes.
        /// </summary>
        /// <param name="s">Input hexadecimal string.</param>
        /// <returns></returns>
        static private byte[] FromHexString(string input)
        {
            return Enumerable.Range(0, input.Length)
                     .Where(charIdx => charIdx % 2 == 0)
                     .Select(charIdx => Convert.ToByte(input.Substring(charIdx, 2), 16))
                     .ToArray();
        }

        /// <summary>
        /// Loads credentials from credential.txt file.
        /// </summary>
        /// <param name="logger"> An object to be used to log messages. </param>
        /// <param name="credentialPath">Path to credential.txt file.</param>
        /// <returns></returns>
        public static Credential LoadCredential(ILogger logger, string credentialPath = "credentials/credential.txt")
        {
            try
            {
                using (var fileInput = new StreamReader(credentialPath))
                using (var jsonInput = new JsonTextReader(fileInput))
                {
                    var clientCredential = new JsonSerializer().Deserialize<Credential>(jsonInput);
                    string clientSecret = Convert.ToBase64String(clientCredential.DecodedSecret);

                    logger.Information("client id: {clientId}", clientCredential.ClientId);

                    var now = DateTimeOffset.UtcNow;
                    var expiresIn = clientCredential.ExpiresAt - now;
                    if (clientCredential.ExpiresAt < now)
                    {
                        logger.Warning("Credentials expired {expiresIn} days ago", -expiresIn.Days);
                    }
                    else if (clientCredential.ExpiresAt < now.AddMonths(1))
                    {
                        logger.Warning("Credentials expiring in {expiresIn} days", expiresIn.Days);
                    }

                    return clientCredential;
                }
            }
            catch (JsonReaderException error)
            {
                logger.Fatal("Cannot read credential file, probably not in JSON format: {error}", error.Message);
            }
            catch (UnauthorizedAccessException)
            {
                logger.Fatal("Cannot access credential file, check file permissions.");
            }
            catch (ArgumentException error)
            {
                logger.Fatal("Cannot load credentials: {error}", error.Message);
            }
            catch (IOException error)
            {
                logger.Fatal("Cannot open credential file: {error}", error.Message);
            }
            Environment.Exit(-1);
            return null;
        }

        /// <summary>
        /// Creates new JWT token for the given input request parameters.
        /// </summary>
        /// <param name="host">the beap host being accessed.</param>
        /// <param name="path">URI path of the accessed endpoint.</param>
        /// <param name="method">HTTP method used to access the endpoint.</param>
        /// <returns></returns>
        internal string CreateToken(string host, string path, string method)
        {
            string guid = Guid.NewGuid().ToString();
            // Get unix timestamp
            var issueTime = (long)(DateTimeOffset.UtcNow - UnixEpoch).TotalSeconds;
            // Create key for JWT signature
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(DecodedSecret);
            // Define JWT signing key and algorythm
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            // Create JWT header container object
            var header = new JwtHeader(signingCredentials);
            // Create JWT payload container object
            var payload = new JwtPayload
            {
                { JwtRegisteredClaimNames.Iss, ClientId },
                { JwtRegisteredClaimNames.Iat, issueTime },
                { JwtRegisteredClaimNames.Nbf, issueTime },
                { JwtRegisteredClaimNames.Exp, issueTime + d_lifetime },
                { "host", host },
                { "path", path },
                { "region", d_region },
                { "jti", guid },
                { "method", method },
                { "client_id", ClientId }
            };
            // Create JWT token object
            JwtSecurityToken jwtToken = new JwtSecurityToken(header, payload);
            // Serialize JWT token object to base64 encoded string
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }

    /// <summary>
    /// This class serves to inject JWT tokens in each outgoing HTTP request.
    /// It uses standard "HttpClient Message Handlers" engine provided by 
    /// HTTPClient component to embed into HTTPClient HTTP handling subsystem. 
    /// (https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/httpclient-message-handlers)
    /// </summary>
    class BEAPHandler : DelegatingHandler
    {
        const HttpStatusCode PermanentRedirect = (HttpStatusCode)308;
        private readonly string d_apiVersion;
        private readonly Credential d_tokenMaker;
        private const uint d_maxRedirects = 3;
        private const string d_MediaTypeSseEvents = "text/event-stream";
        private readonly ILogger d_logger;

        /// <summary>
        /// Constructs new JWT signing handler and initializes default HttpClientHandler inner HTTP handler 
        /// which is used to delegate the rest of standard HTTP messaging behaviour to the regular
        /// component. 
        /// Note: The default "HttpClientHandler" redirecitons handling is disabled because in case of
        /// HTTP redirection default "HttpClientHandler" behaviour is to follow redirection without
        /// generating new JWT token for each redirected request while actually each request should have
        /// its own unique JWT token.
        ///
        /// This constructor will enable all logging of this unit to console. The following code shows how to
        /// customize this behaviour:
        /// var logger = new LoggerConfiguration()
        ///     .MinimumLevel.Information()
        ///     .WriteTo.Console()
        ///     .CreateLogger();
        /// var httpHandler = new BEAPHandler(logger: logger);
        /// </summary>
        /// <param name="apiVersion"> The Hypermedia API version. </param>
        /// <param name="tokenMaker"> JWT token maker instance responsible for JWT generation. </param>
        /// <param name="logger"> An object to be used to log messages. </param>
        public BEAPHandler(string apiVersion = "2", Credential tokenMaker = null, ILogger logger = null)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var defaultHandler = new HttpClientHandler();
            // Important: the following line disables default redirection 
            // hadler because the default handler will not update JWT on 
            // each redirection, JWTSigningHandler will do it in 
            // 'SendAsync' method.
            defaultHandler.AllowAutoRedirect = false;
            InnerHandler = defaultHandler;
            d_apiVersion = apiVersion;
            d_logger = logger ?? DefaultLogger();
            d_tokenMaker = tokenMaker ?? Credential.LoadCredential(d_logger);
        }

        /// <summary>
        /// Return default logger in case if it is not provided by application.
        /// </summary>
        /// <returns> Default logger with enabled logging all messages to console. </returns>
        static ILogger DefaultLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }

        /// <summary>
        /// Determine whether provided response has one of the redirection status code.
        /// </summary>
        /// <param name="response"> HTTP response to determine redirection status for. </param>
        /// <returns> True if this is redirect response, false otherwise </returns>
        private static bool IsRedirect(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.MultipleChoices:
                case HttpStatusCode.Moved:
                case HttpStatusCode.Found:
                case HttpStatusCode.SeeOther:
                case HttpStatusCode.TemporaryRedirect:
                case PermanentRedirect:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Convert HTTP method of redirected response in accord with HTTP standard.
        /// </summary>
        /// <param name="response"> Response containing redirection status. </param>
        /// <param name="request"> Original HTTP request. </param>
        private static void ConvertForRedirect(HttpResponseMessage response, HttpRequestMessage request)
        {
            if (request.Method == HttpMethod.Post)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.MultipleChoices:
                    case HttpStatusCode.Moved:
                    case HttpStatusCode.Found:
                    case HttpStatusCode.SeeOther:
                        request.Content = null;
                        request.Headers.TransferEncodingChunked = false;
                        request.Method = HttpMethod.Get;
                        break;
                }
            }
        }
        /// <summary>
        /// Set new JWT token to outgoing request.
        /// </summary>
        /// <param name="request"> The HTTP request message to send to the server. </param>
        private void AssignBeapToken(HttpRequestMessage request)
        {
            var accessToken = d_tokenMaker.CreateToken(
                request.RequestUri.Host,
                request.RequestUri.LocalPath,
                request.Method.ToString()
                );

            request.Headers.Remove("JWT");
            request.Headers.Add("JWT", accessToken);
        }
        /// <summary>
        /// Add JWT token to request and reflect HTTP redirection responses, if any.
        /// </summary>
        /// <param name="request"> The HTTP request message to send to the server. </param>
        /// <param name="cancellationToken"> A cancellation token to cancel operation. </param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            System.Threading.CancellationToken cancellationToken)
        {
            AssignBeapToken(request);
            request.Headers.Add("api-version", d_apiVersion);

            d_logger.Debug("Request being sent to HTTP server:\n{request}\n", request);

            var response = await base.SendAsync(request, cancellationToken);
            uint redirectCount = 0;
            try
            {
                while (IsRedirect(response) && redirectCount++ < d_maxRedirects)
                {
                    Uri redirectUri;
                    redirectUri = response.Headers.Location;
                    if (!redirectUri.IsAbsoluteUri)
                    {
                        redirectUri = new Uri(request.RequestUri, response.Headers.Location);
                    }

                    d_logger.Information("Redirecting to {redirectUri}.", redirectUri);

                    request.RequestUri = redirectUri;
                    ConvertForRedirect(response, request);
                    response.Dispose();

                    AssignBeapToken(request);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }
            catch (Exception)
            {
                response.Dispose();
                throw;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.Unauthorized:
                    d_logger.Fatal("Either supplied credentials are invalid or expired, " +
                    	"or the requesting IP is address is not on the allowlist");
                    break;
            }

            d_logger.Debug(
                "{method} {requestUri} [{statusCode}]",
                request.Method,
                request.RequestUri,
                response.StatusCode);
            // If it's not a file download request - print the response contents.
            if (response.Content.Headers.ContentDisposition == null &&
                response.Content.Headers.ContentType.MediaType != d_MediaTypeSseEvents)
            {
                var content = await response.Content.ReadAsStringAsync();
                d_logger.Debug("Response content: {content}", content);
            }
            return response;
        }
    }
}
