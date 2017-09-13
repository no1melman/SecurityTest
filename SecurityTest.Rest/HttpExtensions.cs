using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Newtonsoft.Json;

namespace SecurityTest.Rest
{
    public static class HttpExtensions
    {
        public static async Task<T> DeserialiseRequestBodyAsync<T>(this HttpContext ctx)
        {
            var streamReader = new StreamReader(ctx.Request.Body);
            var requestBody = await streamReader.ReadToEndAsync();
            streamReader.Dispose();
            var type = JsonConvert.DeserializeObject<T>(requestBody);
            return type;
        }

        public static Task WriteStringToResponse(this HttpContext ctx, string data)
        {
            var unauthorisedBytes = Encoding.UTF8.GetBytes(data);
            return ctx.Response.Body.WriteAsync(unauthorisedBytes, 0, unauthorisedBytes.Length);
        }

        public static Task WriteObjectToResponse(this HttpContext ctx, object data)
        {
            return WriteStringToResponse(ctx, JsonConvert.SerializeObject(data));
        }

        public static void WriteCookieToResponse(this HttpContext ctx, CookieBuilder builder, string data)
        {
            ICookieManager cm = new ChunkingCookieManager();
            var cookieOptions = builder.Build(ctx);
            cm.AppendResponseCookie(ctx, builder.Name,
                data,
                cookieOptions);
        }
    }
}