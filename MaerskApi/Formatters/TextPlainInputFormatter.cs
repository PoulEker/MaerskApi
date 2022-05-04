using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;
using MaerskApi.Models;



namespace MaerskApi.Formatters
{
    public class TextPlainInputFormatter : TextInputFormatter
    {
        private const string ContentType = "text/plain";


        public TextPlainInputFormatter()
        {
            SupportedMediaTypes.Add(ContentType);

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);

        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            using (var reader = new StreamReader(request.Body))
            {
                var content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        public override bool CanRead(InputFormatterContext context)
        {
            var contentType = context.HttpContext.Request.ContentType;
            if (!string.IsNullOrEmpty(contentType))
            {
                return contentType.StartsWith(ContentType);
            }
            return false;
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var httpContext = context.HttpContext;
            var serviceProvider = httpContext.RequestServices;

            var logger = serviceProvider.GetRequiredService<ILogger<InputFormatter>>();

            using var reader = new StreamReader(httpContext.Request.Body, encoding);
            string? nameLine = null;

            try
            {
                await ReadLineAsync("BEGIN:VCARD", reader, context, logger);
                await ReadLineAsync("VERSION:", reader, context, logger);

                nameLine = await ReadLineAsync("N:", reader, context, logger);

                var split = nameLine.Split(";".ToCharArray());
                //var contact = new Booking(FirstName: split[1], LastName: split[0].Substring(2));

                await ReadLineAsync("FN:", reader, context, logger);
                await ReadLineAsync("END:VCARD", reader, context, logger);

                logger.LogInformation("nameLine = {nameLine}", nameLine);

                return await InputFormatterResult.SuccessAsync(new Booking());
            }
            catch
            {
                logger.LogError("Read failed: nameLine = {nameLine}", nameLine);
                return await InputFormatterResult.FailureAsync();
            }


        }
        private static async Task<string> ReadLineAsync(
            string expectedText, StreamReader reader, InputFormatterContext context,
            ILogger logger)
        {
            var line = await reader.ReadLineAsync();

            if (line is null || !line.StartsWith(expectedText))
            {
                var errorMessage = $"Looked for '{expectedText}' and got '{line}'";

                context.ModelState.TryAddModelError(context.ModelName, errorMessage);
                logger.LogError(errorMessage);

                throw new Exception(errorMessage);
            }

            return line;
        }

    }
}
