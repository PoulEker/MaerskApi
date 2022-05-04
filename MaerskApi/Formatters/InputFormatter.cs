using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MaerskApi.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace MaerskApi.Formatters
{
// <snippet_Class>
public class InputFormatter : TextInputFormatter
{
    // <snippet_ctor>
    public InputFormatter()
    {
        SupportedMediaTypes.Add("text/plain");

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            string? data = null;
            using (var streamReader = context.ReaderFactory(context.HttpContext.Request.Body, encoding))
            {
                data = await streamReader.ReadToEndAsync();
            }
            return InputFormatterResult.Success(data);
        }

        // </snippet_ctor>

        //protected override bool CanReadType(Type type)
        //    => type == typeof(Contact);

        //public override async Task<InputFormatterResult> ReadRequestBodyAsync(
        //    InputFormatterContext context, Encoding effectiveEncoding)
        //{
        //    var httpContext = context.HttpContext;
        //    var serviceProvider = httpContext.RequestServices;

        //    var logger = serviceProvider.GetRequiredService<ILogger<InputFormatter>>();

        //    using var reader = new StreamReader(httpContext.Request.Body, effectiveEncoding);
        //    string? nameLine = null;

        //    try
        //    {
        //        await ReadLineAsync("BEGIN:VCARD", reader, context, logger);
        //        await ReadLineAsync("VERSION:", reader, context, logger);

        //        nameLine = await ReadLineAsync("N:", reader, context, logger);

        //        var split = nameLine.Split(";".ToCharArray());
        //        var contact = new Contact(FirstName: split[1], LastName: split[0].Substring(2));

        //        await ReadLineAsync("FN:", reader, context, logger);
        //        await ReadLineAsync("END:VCARD", reader, context, logger);

        //        logger.LogInformation("nameLine = {nameLine}", nameLine);

        //        return await InputFormatterResult.SuccessAsync(contact);
        //    }
        //    catch
        //    {
        //        logger.LogError("Read failed: nameLine = {nameLine}", nameLine);
        //        return await InputFormatterResult.FailureAsync();
        //    }
        //}

        //private static async Task<string> ReadLineAsync(
        //    string expectedText, StreamReader reader, InputFormatterContext context,
        //    ILogger logger)
        //{
        //    var line = await reader.ReadLineAsync();

        //    if (line is null || !line.StartsWith(expectedText))
        //    {
        //        var errorMessage = $"Looked for '{expectedText}' and got '{line}'";

        //        context.ModelState.TryAddModelError(context.ModelName, errorMessage);
        //        logger.LogError(errorMessage);

        //        throw new Exception(errorMessage);
        //    }

        //    return line;
        //}
    }
}
// </snippet_Class>
