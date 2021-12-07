using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder
    .Services
        .AddControllers(options => 
            options.InputFormatters.Insert(0, new TextPlainInputFormatter()))
        .AddDapr();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.UseCloudEvents();

app.MapControllers();

app.MapSubscribeHandler();

app.Run();

public class TextPlainInputFormatter : TextInputFormatter
{
    private const string ContentType = "text/plain";

    public TextPlainInputFormatter()
    {
        SupportedMediaTypes.Add(ContentType);
    }

    //public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    //{
    //    var request = context.HttpContext.Request;
    //    using (var reader = new StreamReader(request.Body))
    //    {
    //        var content = await reader.ReadToEndAsync();
    //        return await InputFormatterResult.SuccessAsync(content);
    //    }
    //}

    public override bool CanRead(InputFormatterContext context)
    {
        var contentType = context.HttpContext.Request.ContentType;
        return contentType?.StartsWith(ContentType) ?? false;
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(
        InputFormatterContext context, 
        Encoding encoding)
    {
        var request = context.HttpContext.Request;
        using (var reader = new StreamReader(request.Body, encoding))
        {
            var content = await reader.ReadToEndAsync();
            return await InputFormatterResult.SuccessAsync(content);
        }
    }
}