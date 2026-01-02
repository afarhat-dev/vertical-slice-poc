using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using WebClientApi.Models;
using WebClientApi.Services;

namespace WebClientApi.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class EncryptedApiAttribute : Attribute, IAsyncActionFilter, IAsyncResultFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var encryptionService = context.HttpContext.RequestServices.GetRequiredService<IEncryptionService>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<EncryptedApiAttribute>>();

        // Only decrypt if the request body contains encrypted data
        if (context.HttpContext.Request.Method != "GET" &&
            context.HttpContext.Request.ContentLength > 0)
        {
            try
            {
                // Read the encrypted request body
                context.HttpContext.Request.EnableBuffering();
                using var reader = new StreamReader(context.HttpContext.Request.Body, leaveOpen: true);
                var encryptedBody = await reader.ReadToEndAsync();
                context.HttpContext.Request.Body.Position = 0;

                if (!string.IsNullOrEmpty(encryptedBody))
                {
                    var encryptedRequest = JsonSerializer.Deserialize<EncryptedRequest>(encryptedBody);

                    if (encryptedRequest?.EncryptedData != null)
                    {
                        // Decrypt the data
                        var decryptedJson = encryptionService.Decrypt(encryptedRequest.EncryptedData);
                        logger.LogInformation("Decrypted request data");

                        // Find the parameter that should receive the decrypted data
                        foreach (var parameter in context.ActionDescriptor.Parameters)
                        {
                            if (parameter.BindingInfo?.BindingSource == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Body)
                            {
                                var parameterType = (parameter as Microsoft.AspNetCore.Mvc.Controllers.ControllerParameterDescriptor)?.ParameterInfo.ParameterType;
                                if (parameterType != null)
                                {
                                    var deserializedObject = JsonSerializer.Deserialize(decryptedJson, parameterType, new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    });
                                    context.ActionArguments[parameter.Name] = deserializedObject;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to decrypt request");
                context.Result = new BadRequestObjectResult(new { error = "Invalid encrypted request" });
                return;
            }
        }

        await next();
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var encryptionService = context.HttpContext.RequestServices.GetRequiredService<IEncryptionService>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<EncryptedApiAttribute>>();

        if (context.Result is ObjectResult objectResult && objectResult.Value != null)
        {
            try
            {
                // Serialize the response
                var jsonResponse = JsonSerializer.Serialize(objectResult.Value);

                // Encrypt the response
                var encryptedData = encryptionService.Encrypt(jsonResponse);
                logger.LogInformation("Encrypted response data");

                // Replace the result with encrypted response
                context.Result = new ObjectResult(new EncryptedResponse
                {
                    EncryptedData = encryptedData
                })
                {
                    StatusCode = objectResult.StatusCode
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to encrypt response");
                context.Result = new ObjectResult(new { error = "Failed to encrypt response" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        await next();
    }
}
