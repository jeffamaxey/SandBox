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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;

namespace BEAPSamples
{
    /// <summary>
    /// This module tries to perform basic diagnstics for issues related to 
    /// HAPI server connectivity.
    /// </summary>
    class Diagnostics
    {
        private const string HapiHost = "api.bloomberg.com";
        private const int HapiPort = 443;
        private static Uri HapiUri = new UriBuilder("https", HapiHost).Uri;

        /// <summary>
        /// Extracty main and all nested exceptions into array and print them.
        /// </summary>
        /// <param name="topError">Top level exception</param>
        /// <returns></returns>
        public static IEnumerable<Exception> FetchExceptions(Exception topError)
        {
            Console.WriteLine($"\t{topError.Message}");
            int depth = 0;
            yield return topError;
            Exception innerError = topError;
            while (innerError.InnerException != null)
            {
                innerError = innerError.InnerException;
                Console.WriteLine($"\t{new String(' ', depth * 3)}-> {innerError.Message}");
                yield return innerError;
                depth += 1;
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Try to infer and describe the cause and solutions for different exception types.
        /// </summary>
        /// <param name="allErrors"></param>
        public static void DescribeException(Exception[] allErrors)
        {
            var innerError = allErrors[allErrors.Length - 1];
            var parentError = allErrors.ElementAtOrDefault(allErrors.Length - 2);


            if (innerError is SocketException socketError)
            {
                Console.WriteLine($"Socket error code: {socketError.SocketErrorCode}");
                // Using SoketErrorCode as it's more reliable for most OS.
                // https://blog.jetbrains.com/dotnet/2020/04/27/socket-error-codes-depend-runtime-operating-system/
                switch (socketError.SocketErrorCode)
                {

                    case SocketError.AccessDenied:
                        Console.WriteLine("\tAccess to create the TCP connection is denied.");
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Firewall is blocking connections to the HAPI host.");
                        Console.WriteLine("\tProbable solutions:");
                        Console.WriteLine("\t\t- Check that api.bloomberg.com is accessible from your network.");
                        Console.WriteLine("\t\t- Check local firewall settings.");
                        break;
                    case SocketError.ConnectionReset:
                        // typically the description of this error is: 
                        // "An existing connection was forcibly closed by the remote host" -
                        // may depend on locale settings(may be translated to local language)
                        Console.WriteLine("\tThe remote server closed the established connection.");
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- This client uses old version of TLS protocol (TLS1.0).");
                        Console.WriteLine("\t\t- Access to requested host is denied from your network.");
                        Console.WriteLine("\t\t- Too many pending connections made from this application.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Check that TLSv1.2 is enabled for this application.");
                        Console.WriteLine("\t\t- Try using single instance of HTTPClient per application.");
                        Console.WriteLine("\t\t- Try using diagnostics.py to detect networking issues.");
                        break;
                    case SocketError.TimedOut:
                        Console.WriteLine("\tConnection was not established within the provided timeout.");
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Firewall is blocking connections to the HAPI host.");
                        Console.WriteLine("\t\t- Connection not established due to network delays/losses.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Ensure that access to api.bloomberg.com is allowed.");
                        Console.WriteLine("\t\t- Try connecting to api.bloomberg.com using diagnostics.py.");
                        Console.WriteLine("\t\t- Try increasing connection timeout.");
                        break;
                    case SocketError.OperationAborted:
                        // socket was closed while the send or receive was in progress
                        Console.WriteLine("\tSocket was closed while the send or receive was in progress.");
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Operation was cancelled due to timeout.");
                        Console.WriteLine("\t\t- Excess of outbound http connections.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Try increasing clientHttp.Timeout value.");
                        Console.WriteLine("\t\t- Try using single instance of HTTPClient per application.");
                        Console.WriteLine("\t\t- If the issue is rare - retry approach may be suitable.");
                        break;
                    case SocketError.Shutdown:
                        // A request to send or receive data was disallowed because ... been closed.
                        // Firewall issue? typically all errors are due to xamarin issue with raising this 
                        // exception in case if server responded with "content too large" HTTP RC
                        break;
                    case SocketError.HostNotFound:
                        // The socket error code the "Device not configured" error is mapped to.
                        Console.WriteLine("\tUnable to resolve the specified hostname.");
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Hostname is incorrect.");
                        Console.WriteLine("\t\t- Hostname resolution failure.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Check the hostname.");
                        Console.WriteLine("\t\t- Ensure that DNS settings are correct.");
                        break;
                    case SocketError.NoBufferSpaceAvailable:
                        // should not be the case in common, happened to server apps
                        break;
                    case SocketError.ConnectionRefused:
                        Console.WriteLine("\tThe target host rejects connections.");
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Firewall is blocking connections to the HAPI host.");
                        Console.WriteLine("\t\t- Incorrect hostname is requested.");
                        Console.WriteLine("\t\t- Incorrect TCP port is used.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Check that api.bloomberg.com is not bloked by firewall.");
                        Console.WriteLine("\t\t- Try running diagnostics.py utility.");
                        Console.WriteLine("\t\t- Try to access api.bloomberg.com host your network.");
                        Console.WriteLine("\t\t- Check the hostname in the requested URI.");
                        Console.WriteLine("\t\t- Check the requested URI scheme(should be HTTPS).");
                        Console.WriteLine("\t\t- Ensure that DNS settings are correct.");
                        break;
                    case SocketError.AddressAlreadyInUse:
                        // should not be the case- server issue typically
                        break;

                    default:
                        Console.WriteLine("");
                        break;
                }
                if (socketError.SocketErrorCode == SocketError.TimedOut)
                {
                    Console.WriteLine("Connection timed out");
                }
            }
            else if (innerError is WebException webError)
            {
                Console.WriteLine(webError.Message);
                switch (webError.Status)
                {
                    case WebExceptionStatus.NameResolutionFailure:
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Could not reach DNS server.");
                        Console.WriteLine("\t\t- DNS settings are incorrect.");
                        Console.WriteLine("\t\t- DNS server could not find the hostname.");
                        Console.WriteLine("\t\t- The hostname is incorrrect.");
                        Console.WriteLine("\t\t- Temporarily networking failure.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Check DNS settings.");
                        Console.WriteLine("\t\t- Try resolving the hostname using nlsookup utility.");
                        Console.WriteLine("\t\t- Check that hostname is correct, not misspelled.");
                        break;
                    case WebExceptionStatus.SecureChannelFailure:
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Required TLS version is not enabled.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Try to enable TLSv1.2 using HttpClientHandler.");
                        Console.WriteLine("\t\t- Check TLS 1.2 registry settings (for windows).");
                        // https://docs.microsoft.com/en-us/windows-server/security/tls/tls-registry-settings
                        break;
                    case WebExceptionStatus.ProtocolError:
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Proxy server authentication is not accomplished.");
                        Console.WriteLine("\t\t- Sending HTTP body for url with redirection.");
                        Console.WriteLine("\t\t- Required HTTP header is not provided by server.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Check Proxy settings(if proxy server is used).");
                        Console.WriteLine("\t\t- Avoid using redirects for POST/PATCH requests.");
                        Console.WriteLine("\t\t- Check that the requested URI is the correct.");
                        Console.WriteLine("\t\t- Check that requested HTTP method is allowed.");
                        break;
                    case WebExceptionStatus.RequestProhibitedByProxy:
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Proxy server restricts accessing HAPI host.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Ensure the acess to api.bloomberg.com is allowed.");
                        break;
                    case WebExceptionStatus.RequestCanceled:
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Connection in the pool was closed by timeout.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Increase timeout(HTTPClient.Timeout property).");
                        break;
                }
            }
            else if (parentError is WebException webParentError)
            {
                // Using parent error for cases when inner error does not contain an error code.
                Console.WriteLine(innerError.Message);
                switch (webParentError.Status)
                {
                    case WebExceptionStatus.TrustFailure:
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- x509 certificate verification failed.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Check certificate validity of api.bloomberg.com in browser.");
                        Console.WriteLine("\t\t- Check/update the root certificate store.");
                        break;
                    case WebExceptionStatus.ReceiveFailure:
                        // The parent msg: The underlying connection was closed: An unexpected error ...
                        // The inner msg: An established connection was aborted by the software in ...
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Connection timeout error.");
                        Console.WriteLine("\t\t- Timed out current receive operation.");
                        Console.WriteLine("\t\t- Sytem time has just been adjusted.");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Increase connection/receive timeout.");
                        Console.WriteLine("\t\t- Retry if the error occurs rarely.");
                        break;
                    case WebExceptionStatus.SendFailure:
                        Console.WriteLine("\tPossible reasons:");
                        Console.WriteLine("\t\t- Wrong port is used.");
                        Console.WriteLine("\t\t- Server returned unexpected packet format.");
                        Console.WriteLine("\t\t- .");
                        Console.WriteLine("\tPossible solutions:");
                        Console.WriteLine("\t\t- Check the requested URI scheme(should be HTTPS).");
                        break;
                }
            }
            else if (innerError is AuthenticationException authError)
            {
                // this is raised for https://untrusted-root.badssl.com/
                // https://no-common-name.badssl.com/
                // https://no-subject.badssl.com/
                Console.WriteLine(authError.Message);
                Console.WriteLine("\tWarning: potential security risk,");
                Console.WriteLine("\t         do not workaround this error.");
                Console.WriteLine("\tPossible reasons:");
                Console.WriteLine("\t\t- The accessed server is not authentic.");
                Console.WriteLine("\t\t- Server's certificate has expired.");
                Console.WriteLine("\t\t- Invalid server certificate.");
                Console.WriteLine("\tPossible solutions:");
                Console.WriteLine("\t\t- Verify server's IP address.");
                Console.WriteLine("\t\t- Try verifing server's certificate in browser.");
                Console.WriteLine("\t\t- Try using diagnostics.py utility.");
            }
            else if (parentError is AuthenticationException)
            {
                Console.WriteLine(parentError.Message);
                Console.WriteLine("\tWarning: potential security risk,");
                Console.WriteLine("\t         do not workaround this error.");
                Console.WriteLine("\tPossible reasons:");
                // 1000-sans for instance
                Console.WriteLine("\t\t- Unsupported SSL server certificate.");
                // for "https://rc4-md5.badssl.com/"
                // and for "https://rc4.badssl.com/"
                // and for "https://null.badssl.com/"
                Console.WriteLine("\t\t- Insecure encryption used in certificate.");
                Console.WriteLine("\tPossible solutions:");
                Console.WriteLine("\t\t- Verify server's IP address.");
                Console.WriteLine("\t\t- Try verifing server's certificate in browser.");
                Console.WriteLine("\t\t- Try using diagnostics.py utility.");
            }
            // Not exception for:
            // https://revoked.badssl.com/
            // https://pinning-test.badssl.com/
        }

        /// <summary>
        /// Handle initial exception and check HAPI server availability.
        /// </summary>
        /// <param name="topError">original exception</param>
        public static void run(Exception topError) {
            Console.WriteLine($"Original exception descriptions:");
            var allErrors = FetchExceptions(topError).ToArray();
            Console.WriteLine($"=== Inferring the cause of original exception ===");
            DescribeException(allErrors);

            Console.Write("\n\n\n");
            Console.WriteLine($"=== Checking proxy settings. ===");
            try
            {
                var proxy = WebRequest.GetSystemWebProxy();
                Uri resourceProxy = proxy.GetProxy(HapiUri);
                if (resourceProxy == HapiUri)
                {
                    Console.WriteLine($"No proxy for {HapiUri} specified.");
                    Console.WriteLine($"Ensure whether you need to set Proxy settings.");
                    return;
                }
                else
                {
                    Console.WriteLine($"Proxy for {HapiUri} is {resourceProxy}");
                }
            }
            catch (PlatformNotSupportedException)
            {
                Console.WriteLine("Skipped proxy checking since it's not supported.");
                Console.WriteLine("Check whether usage of proxy server is required.");
                Console.WriteLine("Ensure proxy settings are correct(if required).");
            }

            try
            {
                Console.Write("\n\n\n");
                Console.WriteLine($"=== Testing TCP connection. ===");
                using (var tcpSession = new TcpClient(HapiHost, HapiPort))
                {
                    Console.WriteLine("Result: conection established.");
                    Console.WriteLine($"=== Testing SSL session. ===");
                    using (var sslStream = new SslStream(tcpSession.GetStream()))
                    {
                        sslStream.AuthenticateAsClient(HapiHost);
                        Console.WriteLine("SSL handshake passed.");
                        Console.WriteLine($"SSL connection to {HapiHost} established.");
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"Failed connecting to {HapiHost}:");
                var errors = FetchExceptions(error).ToArray();
                DescribeException(errors);
            }
        }
    }
}
