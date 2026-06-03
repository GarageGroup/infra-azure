using System;
using System.Net;
using System.Text.RegularExpressions;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.Azure.Functions.Worker.Http;

namespace GarageGroup.Infra;

partial class SwaggerFunction
{
    private const string FunctionCodeQueryParameterName = "code";

    private const string FunctionKeySecuritySchemeName = "FunctionKey";

    private static readonly Regex WindowUiAssignmentRegex = new(
        @"(?m)^(?<indent>\s*)window\.ui\s*=\s*ui;?\s*$",
        RegexOptions.CultureInvariant);

    public static HttpResponseData GetSwaggerUI(
        this HttpRequestData request, string swaggerSection = DefaultSwaggerSection, string swaggerUrl = DefaultSwaggerUrl)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.InnerBuildSwaggerUiResponse(request.FunctionContext.GetSwaggerOption(DefaultSwaggerSection) ?? new(), swaggerUrl);
    }

    public static HttpResponseData GetSwaggerUI(
        this HttpRequestData request, SwaggerOption swaggerOption, string swaggerUrl = DefaultSwaggerUrl)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.InnerBuildSwaggerUiResponse(swaggerOption ?? new(), swaggerUrl);
    }

    private static HttpResponseData InnerBuildSwaggerUiResponse(
        this HttpRequestData request, SwaggerOption swaggerOption, string swaggerUrl)
    {
        var options = new SwaggerDocOptions
        {
            Title = swaggerOption.ApiName
        };

        var content = options.GetSwaggerUIContent(swaggerUrl);

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteString(content);

        _ = response.Headers.TryAddWithoutValidation("Content-Type", "text/html;charset=utf-8");
        return response;
    }

    private static string GetSwaggerUIContent(this SwaggerDocOptions swaggerOptions, string swaggerUrl)
        =>
        LazyHtmlTemplate.Value
            .WithFunctionKeyAuthorization()
            .Replace("{url}", swaggerUrl)
            .Replace("{title}", swaggerOptions.Title)
            .Replace("{oauth2RedirectUrl}", swaggerOptions.OAuth2RedirectPath)
            .Replace("{clientId}", swaggerOptions.ClientId)
            .Replace("{clientSecret}", string.Empty)
            .Replace("{useBasicAuthenticationWithAccessCodeGrant}", "false")
            .Replace("{usePkceWithAuthorizationCodeGrant}", "false");

    private static string WithFunctionKeyAuthorization(this string html)
        =>
        WindowUiAssignmentRegex.Replace(
            html,
            AddFunctionKeyAuthorization);

    private static string AddFunctionKeyAuthorization(Match match)
    {
        var indent = match.Groups["indent"].Value;

        return
            $$"""
            {{indent}}window.ui = ui

            {{indent}}const functionCode = new URLSearchParams(window.location.search).get('{{FunctionCodeQueryParameterName}}');
            {{indent}}let functionKeyAuthorizationAttempts = 0;
            {{indent}}const authorizeFunctionKey = function() {
            {{indent}}  if (!functionCode || !ui.preauthorizeApiKey) {
            {{indent}}    return;
            {{indent}}  }

            {{indent}}  const system = ui.getSystem && ui.getSystem();
            {{indent}}  const specSelectors = system && system.specSelectors;
            {{indent}}  const securityDefinitions = specSelectors && specSelectors.securityDefinitions && specSelectors.securityDefinitions();
            {{indent}}  const functionKeyScheme = securityDefinitions && (securityDefinitions.get ? securityDefinitions.get('{{FunctionKeySecuritySchemeName}}') : securityDefinitions['{{FunctionKeySecuritySchemeName}}']);

            {{indent}}  if (!functionKeyScheme) {
            {{indent}}    if (functionKeyAuthorizationAttempts++ < 50) {
            {{indent}}      window.setTimeout(authorizeFunctionKey, 100);
            {{indent}}    }

            {{indent}}    return;
            {{indent}}  }

            {{indent}}  ui.preauthorizeApiKey('{{FunctionKeySecuritySchemeName}}', functionCode);
            {{indent}}};

            {{indent}}authorizeFunctionKey();
            """;
    }
}