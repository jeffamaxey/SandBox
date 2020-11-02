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

using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BEAPSamples
{
    /// <summary>
    /// This class creates a new request and all associated resources from scratch.
    /// </summary>
    class SampleRequestFromScratch
    {
        private static readonly HttpClient BeapSession = new HttpClient(new BEAPHandler());

        private readonly string UniverseId;
        private readonly string FieldListId;
        private readonly string TriggerId;
        private readonly string RequestId;
        private const string OutputPath = "samples";

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        SampleRequestFromScratch()
        {
            // Generate unique resource indentifiers.
            var resourceIdPostfix = DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            UniverseId = $"myUniverse{resourceIdPostfix}";
            FieldListId = $"myFieldList{resourceIdPostfix}";
            TriggerId = $"myTrigger{resourceIdPostfix}";
            RequestId = $"myReq{resourceIdPostfix}";
        }

        /// <summary>
        /// Constructs a URI referencing the specified resource on the BEAP server.
        /// </summary>
        /// <param name="path">Path to the resource referenced by the URI.</param>
        /// <returns>Constructed URI.</returns>
        private static Uri MakeUri(string path)
        {
            var uriBuilder = new UriBuilder {
                Scheme = "https",
                Host = "api.bloomberg.com",
                Path = path,
            };
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Sends a POST request to BEAP server.
        /// </summary>
        /// <typeparam name="TPayload">The type of payload before serialization.</typeparam>
        /// <param name="uri">URI of the requested resource.</param>
        /// <param name="payload">Request body.</param>
        /// <returns>Location of a successfully created resource.</returns>
        private async Task<string> HttpPost<TPayload>(Uri uri, TPayload payload)
        {
            var body = JsonConvert.SerializeObject(payload, Formatting.Indented);

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await BeapSession.PostAsync(uri, content);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Unexpected response status: {response.ReasonPhrase}");
            }

            return response.Headers.Location.ToString();
        }

        /// <summary>
        /// Simplified data model for single catalog in HAPI.
        /// </summary>
        private class Catalog
        {
            [JsonProperty("identifier")]
            public string Identifier { get; set; }

            [JsonProperty("subscriptionType")]
            public string SubscriptionType { get; set; }
        }

        /// <summary>
        /// Simplified data model for 'catalogs' response (actual response contains more data).
        /// </summary>
        private class CatalogsResponse
        {
            [JsonProperty("contains")]
            public Catalog[] Contains { get; set; }
        }

        /// <summary>
        /// Requests a list of available catalogs.
        /// Retrieves the one having "scheduled" subscription type, 
        /// which corresponds to the Data License account number.
        /// </summary>
        /// <returns>Scheduled catalog identifier.</returns>
        private async Task<string> GetScheduledCatalog()
        {
            var uri = MakeUri("/eap/catalogs/");
            var response = await BeapSession.GetAsync(uri);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unexpected response status: {response.ReasonPhrase}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var catalogs = JsonConvert.DeserializeObject<CatalogsResponse>(content);
            var scheduledCatalog = Array.Find(catalogs.Contains, catalog => catalog.SubscriptionType == "scheduled");
            if (scheduledCatalog == null)
            {
                throw new Exception("Account number not found");
            }

            return scheduledCatalog.Identifier;
        }

        /// <summary>
        /// Simplified data model for single security in HAPI.
        /// </summary>
        private class Security
        {
            [JsonProperty("@type")]
            public string Type { get; set; }

            [JsonProperty("identifierType")]
            public string IdentifierType { get; set; }

            [JsonProperty("identifierValue")]
            public string IdentifierValue { get; set; }
        }

        /// Simplified data model for universe.
        private class Universe
        {
            [JsonProperty("@type")]
            public string Type { get; set; }

            [JsonProperty("identifier")]
            public string Identifier { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("contains")]
            public Security[] Contains { get; set; }
        };

        /// <summary>
        /// Creates a new universe resource.
        /// </summary>
        /// <param name="catalog">Catalog identifier (must be the DL account number).</param>
        /// <returns>Location of a successfully created resource.</returns>
        private async Task<string> CreateUniverse(string catalog)
        {
            var uri = MakeUri($"/eap/catalogs/{catalog}/universes/");
            var payload = new Universe {
                Type = "Universe",
                Identifier = UniverseId,
                Title = "My Universe",
                Description = "Some description",
                Contains = new [] {
                    new Security {
                        Type = "Identifier",
                        IdentifierType = "TICKER",
                        IdentifierValue = "SKSW10 Curncy"
                        //IdentifierValue = "AAPL US Equity"
                    },
                    new Security {
                        Type = "Identifier",
                        IdentifierType = "BB_GLOBAL",
                        IdentifierValue = "BBG009S3NB30"
                    }
                }
            };
            return await HttpPost(uri, payload);
        }

        /// <summary>
        /// Simplified data model for single field in HAPI.
        /// </summary>
        private class Field
        {
            [JsonProperty("cleanName")]
            public string CleanName { get; set; }
        }

        /// <summary>
        /// Simplified data model for field list.
        /// </summary>
        private class FieldList
        {
            [JsonProperty("@type")]
            public string Type { get; set; }

            [JsonProperty("identifier")]
            public string Identifier { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("contains")]
            public Field[] Contains { get; set; }
        }

        /// <summary>
        /// Creates a new field list resource.
        /// </summary>
        /// <param name="catalog">Catalog identifier (must be the DL account number).</param>
        /// <returns>Location of a successfully created resource.</returns>
        private async Task<string> CreateFieldList(string catalog)
        {
            var uri = MakeUri($"/eap/catalogs/{catalog}/fieldLists/");
            var payload = new FieldList {
                Type = "DataFieldList",
                Identifier = FieldListId,
                Title = "My FieldList",
                Description = "Some description",
                Contains = new [] {
                    new Field { CleanName = "pxBid" },
                    new Field { CleanName = "marketSectorDes" },
                    new Field { CleanName = "pxLast" }
                }
            };
            return await HttpPost(uri, payload);
        }

        /// Simplified data model for trigger.
        private class Trigger
        {
            [JsonProperty("@type")]
            public string Type { get; set; }

            [JsonProperty("identifier")]
            public string Identifier { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "frequency", NullValueHandling = NullValueHandling.Ignore)]
            public string Frequency { get; set; }

            [JsonProperty(PropertyName="startDate", NullValueHandling = NullValueHandling.Ignore)]
            public string StartDate { get; set; }

            [JsonProperty(PropertyName ="startTime", NullValueHandling = NullValueHandling.Ignore)]
            public string StartTime { get; set; }
        }

        /// <summary>
        /// Creates a new trigger resource.
        /// </summary>
        /// <param name="catalog">Catalog identifier (must be the DL account number).</param>
        /// <returns>Location of a successfully created resource.</returns>
        private async Task<string> CreateTrigger(string catalog)
        {
            var uri = MakeUri($"/eap/catalogs/{catalog}/triggers/");

            // NOTE: The "SubmitTrigger" value is used as the trigger's type.
            //       We choose an event-based trigger, in this case, specifying that the
            //       job should run as soon as possible after POSTing the request component
            var payload = new Trigger {
                Type = "SubmitTrigger",
                Identifier = TriggerId,
                Title = "My Trigger",
                Description = "Some description"
            };

            // An alternative value, "ScheduledTrigger", can be used to specify a
            // recurring job schedule.
            // Using a 5 minute margin of safety for the job:
            // If the current time is less than 5 minutes before midnight in the account timezone,
            // Ensure the job will be processed the following day.
            //var utcNowBiased = DateTime.UtcNow.AddMinutes(5);
            //var accountTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");  // America/New_York
            //var zonedStartDate = TimeZoneInfo.ConvertTime(utcNowBiased, accountTimeZone);
            //var payload = new Trigger {
            //    Type = "ScheduledTrigger",
            //    Identifier = TriggerId,
            //    Title = "My Trigger",
            //    Frequency = "daily",
            //    StartDate = zonedStartDate.ToString("yyyyMMdd"),
            //    StartTime = "12:00:00"
            //};

            return await HttpPost(uri, payload);
        }

        /// <summary>
        /// Data model for request formatting options in HAPI.
        /// </summary>
        private class RequestFormatting
        {
            [JsonProperty("@type")]
            public string Type { get; set; }

            [JsonProperty("columnHeader")]
            public bool ColumnHeader { get; set; }

            [JsonProperty("dateFormat")]
            public string DateFormat { get; set; }

            [JsonProperty("delimiter")]
            public string Delimiter { get; set; }

            [JsonProperty("fileType")]
            public string FileType { get; set; }

            [JsonProperty("outputFormat")]
            public string OutputFormat { get; set; }
        }

        /// <summary>
        /// Simplified data model for pricing source preference in HAPI.
        /// </summary>
        private class PricingSourcePreference
        {
            [JsonProperty("mnemonic")]
            public string Mnemonic { get; set; }
        }

        /// <summary>
        /// Simplified data model for pricing source options in HAPI.
        /// </summary>
        private class PricingSourceOptions
        {
            [JsonProperty("@type")]
            public string Type { get; set; }

            [JsonProperty("prefer")]
            public PricingSourcePreference Prefer { get; set; }
        }

        /// Simplified data model for request.
        private class Request
        {
            [JsonProperty("@type")]
            public string Type { get; set; }

            [JsonProperty("identifier")]
            public string Identifier { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("universe")]
            public Uri Universe { get; set; }

            [JsonProperty("fieldList")]
            public Uri FieldList { get; set; }

            [JsonProperty("trigger")]
            public Uri Trigger { get; set; }

            [JsonProperty("formatting")]
            public RequestFormatting Formatting { get; set; }

            [JsonProperty("pricingSourceOptions")]
            public PricingSourceOptions PricingSourceOptions { get; set; }
        }

        /// <summary>
        /// Creates a new request.
        /// Only one request definition is allowed.
        /// It cannot be POSTed twice after the request resource has been created.
        /// </summary>
        /// <param name="catalog">Catalog identifier (must be the DL account number).</param>
        /// <param name="universePath">Location of successfully created universe definition.</param>
        /// <param name="fieldListPath">Location of successfully created field list definition.</param>
        /// <param name="triggerPath">Location of successfully created trigger definition.</param>
        /// <returns>Location of a successfully created resource.</returns>
        private async Task<string> CreateRequest(string catalog, string universePath, string fieldListPath, string triggerPath)
        {
            var uri = MakeUri($"/eap/catalogs/{catalog}/requests/");
            var payload = new Request {
                Type = "DataRequest",
                Identifier = RequestId,
                Title = "My Request",
                Description = "Some description",
                Universe = MakeUri(universePath),
                FieldList = MakeUri(fieldListPath),
                Trigger = MakeUri(triggerPath),
                Formatting = new RequestFormatting {
                    Type = "DataFormat",
                    ColumnHeader = true,
                    DateFormat = "yyyymmdd",
                    Delimiter = "|",
                    FileType = "unixFileType",
                    OutputFormat = "variableOutputFormat"
                },
                PricingSourceOptions = new PricingSourceOptions {
                    Type = "DataPricingSourceOptions",
                    Prefer = new PricingSourcePreference {
                        Mnemonic = "BGN"
                    }
                }
            };
            return await HttpPost(uri, payload);
        }

        /// <summary>
        /// Downloads distribution file in GZIP format.
        /// </summary>
        /// <param name="distributionUrl">URL of the distribution to load from</param>
        /// <param name="outputFilePath">path to store the distribution file</param>
        /// <returns></returns>
        private async Task DownloadDistribution(Uri distributionUrl, string outputFilePath)
        {
            // Create a HTTP request.
            // Set up compression to reduce download time.
            // Note that some of dataset files can exceed 100MB in size,
            // so compression will speed up downloading significantly.
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = distributionUrl
            };
            request.Headers.Add("Accept-Encoding", "gzip");

            var response = await BeapSession.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var content = response.Content;
            // Read the response data into a file.
            // Use streams to optimize memory footprint.
            using (var contentStream = await content.ReadAsStreamAsync())
            using (var outputFile = File.Create(outputFilePath))
            {
                await contentStream.CopyToAsync(outputFile);
            }

            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Content-Encoding: {content.Headers.ContentEncoding}");
            Console.WriteLine($"Content-Length: {content.Headers.ContentLength} bytes");
            Console.WriteLine($"Downloaded {distributionUrl} to {outputFilePath}");
            Console.WriteLine();
        }


        /// <summary>
        /// Creates a new request along with its associated resources.
        /// </summary>
        async Task DefineRequest()
        {
            //  Create an SSE session to receive notification when reply is delivered
            using (var sseClient = new Sse.SseClient(MakeUri("/eap/notifications/sse"), BeapSession))
            {
                await sseClient.Connect();

                // Fetch our account number.
                var catalog = await GetScheduledCatalog();

                // Create a universe component.
                var universePath = await CreateUniverse(catalog);
                // Fetch the newly-created universe component.
                await BeapSession.GetAsync(MakeUri(universePath));

                // Create a field list component.
                var fieldListPath = await CreateFieldList(catalog);
                // Fetch the newly-created field list component.
                await BeapSession.GetAsync(MakeUri(fieldListPath));

                // Create a trigger component.
                var triggerPath = await CreateTrigger(catalog);
                // Fetch the newly-created trigger component.
                await BeapSession.GetAsync(MakeUri(triggerPath));

                // Create a request component.
                var requestPath = await CreateRequest(catalog, universePath, fieldListPath, triggerPath);
                // Fetch the newly-created request component.
                await BeapSession.GetAsync(MakeUri(requestPath));

                // Wait for notification that our output is ready for download. We allow a reasonable amount of time for
                // the request to be processed and avoid waiting forever for the purposes of the sample code -- a timeout
                // may not apply to your actual business workflow. For larger requests or requests made during periods of
                // high load, you may need to increase the timeout.
                var replyTimeout = TimeSpan.FromMinutes(15);
                var expirationTimestamp = DateTimeOffset.UtcNow.Add(replyTimeout);
                while (DateTimeOffset.UtcNow < expirationTimestamp)
                {
                    var sseEvent = await sseClient.ReadEvent();

                    if (sseEvent.IsHeartbeat)
                    {
                        Console.WriteLine("Received heartbeat event, keep waiting for events");
                        continue;
                    }

                    DeliveryNotification notification;
                    try
                    {
                        notification = JsonConvert.DeserializeObject<DeliveryNotification>(sseEvent.Data);
                    }
                    catch (JsonException)
                    {
                        Console.WriteLine("Received other event type, continue waiting");
                        continue;
                    }

                    Console.WriteLine($"Received reply delivery notification event: {sseEvent.Data}");

                    var deliveryDistributionId = notification.Generated.Identifier;
                    var replyUrl = notification.Generated.Id;
                    var deliveryCatalogId = notification.Generated.Snapshot.Dataset.Catalog.Identifier;

                    if (deliveryCatalogId != catalog || deliveryDistributionId != $"{RequestId}.bbg")
                    {
                        Console.WriteLine("Some other delivery occurred - continue waiting");
                        continue;
                    }

                    // Prepare for storing reply file.
                    Directory.CreateDirectory(OutputPath);
                    var outputFilePath = Path.Combine(OutputPath, $"{deliveryDistributionId}.gz");
                    // Download reply file from server.
                    await DownloadDistribution(replyUrl, outputFilePath);
                    Console.WriteLine("Reply was downloaded, exit now");
                    return;
                }
            }
            Console.WriteLine("Reply NOT delivered, try to increase waiter loop timeout");
        }
        
        /// <summary>
        /// Application entry point of this BEAP sample.
        /// </summary>
        static void Main()
        {
            var sample = new SampleRequestFromScratch();
            sample.DefineRequest().Wait();
        }
    }
}
