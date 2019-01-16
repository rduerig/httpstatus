using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Web.Mvc;
using Teapot.Web.Models;

namespace Teapot.Web.Controllers
{
    [CorsAllowAnyOrigin]
    public class StatusController : Controller
    {
        static readonly StatusCodeResults StatusCodes = new StatusCodeResults();

        private const int SLEEP_MIN = 0;
        private const int SLEEP_MAX = 300000; // 5 mins in milliseconds
        private const string ETAG = "someTag";
        private const string LAST_MODIFIED = "Fri, 09 Aug 2013 23:54:35 GMT";
        private const string LAST_MODIFIED_VALID = "Fri, 09 Aug 2013 13:54:35 GMT";

        private DateTime LAST_MODIFIED_TIME = new DateTime(2013, 8, 9, 23, 54, 35);

        public ActionResult Index()
        {
            return View(StatusCodes);
        }

        public ActionResult StatusCode(int statusCode, string message = null, int? sleep = SLEEP_MIN)
        {
            var ifMatch = Request.Headers.Get("If-Match") ?? ETAG;
            var ifModifiedSince = Request.Headers.Get("If-Modified-Since") ?? LAST_MODIFIED_VALID;
            DateTime ifModifiedSinceTime = DateTime.ParseExact(ifModifiedSince, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat);
            var ifUnmodifiedSince = Request.Headers.Get("If-Unmodified-Since") ?? LAST_MODIFIED;

            StatusCodeResult statusData;

            if (!ifMatch.Equals(ETAG))
            {
                statusCode = 412;
            } else if(LAST_MODIFIED_TIME.CompareTo(ifModifiedSinceTime) <= 0)
            {
                statusCode = 304;
            }

            statusData = StatusCodes.ContainsKey(statusCode)
                    ? StatusCodes[statusCode]
                    : new StatusCodeResult { Description = $"{statusCode} {message ?? "Unknown Code" }" };

            DoSleep(sleep);

            // add etag header
            if (!statusData.IncludeHeaders.ContainsKey("ETag"))
            {
                statusData.IncludeHeaders.Add("ETag", ETAG);
            }
            if (!statusData.IncludeHeaders.ContainsKey("LastModified"))
            {
                statusData.IncludeHeaders.Add("LastModified", LAST_MODIFIED);
            }

            return new CustomHttpStatusCodeResult(statusCode, statusData);
        }

        public ActionResult Cors(int statusCode, int? sleep = SLEEP_MIN)
        {
            if (Request.HttpMethod != "OPTIONS")
            {
                return StatusCode(statusCode, null, sleep);
            }

            var allowedOrigin = Request.Headers.Get("Origin") ?? "*";
            var allowedMethod = Request.Headers.Get("Access-Control-Request-Method") ?? "GET";
            var allowedHeaders = Request.Headers.Get("Access-Control-Request-Headers") ?? "X-Anything";

            var responseHeaders = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", allowedOrigin },
                { "Access-Control-Allow-Headers", allowedHeaders },
                { "Access-Control-Allow-Methods", allowedMethod }
            };

            var statusData = new StatusCodeResult { IncludeHeaders = responseHeaders };

            DoSleep(sleep);

            return new CustomHttpStatusCodeResult((int)HttpStatusCode.OK, statusData);
        }

        private static void DoSleep(int? sleep)
        {
            int sleepData = SanitizeSleepParameter(sleep, SLEEP_MIN, SLEEP_MAX);

            if (sleepData > 0)
            {
                System.Threading.Thread.Sleep(sleepData);
            }
        }

        private static int SanitizeSleepParameter(int? sleep, int min, int max)
        {
            var sleepData = sleep ?? 0;

            // range check - minimum should be 0
            if (sleepData < min)
            {
                sleepData = min;
            }

            // range check- maximum should be 300000 (5 mins)
            if (sleepData > max)
            {
                sleepData = max;
            }

            return sleepData;
        }
    }
}
