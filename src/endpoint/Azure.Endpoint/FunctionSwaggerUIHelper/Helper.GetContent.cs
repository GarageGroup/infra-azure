using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;

namespace GarageGroup.Infra.Endpoint;

partial class FunctionSwaggerUIHelper
{
    internal static string GetSwaggerUIContent(this SwaggerDocOptions swaggerOptions, string swaggerUrl)
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

    private static string LoadAndUpdateHtml(this string documentHtml, ZipArchive archive, string entryName, string? replacement = null)
    {
        var entry = archive.GetEntryOrThrow(entryName);

        using var stream = entry.Open();
        using var reader = new StreamReader(stream);

        var value = reader.ReadToEnd();
        return string.IsNullOrEmpty(replacement) is false ? documentHtml.Replace(replacement, value) : value;
    }

    private static Stream GetZippedResources()
    {
        var assembly = Assembly.GetAssembly(typeof(SwashBuckleClient))
            ?? throw new InvalidOperationException($"Assembly for type {typeof(SwashBuckleClient)} was not found");

        var resourceName = $"{typeof(ISwashBuckleClient).Namespace}.EmbededResources.resources.zip";

        return assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"ManifestResource {resourceName} must be not null");
    }

    private static ZipArchiveEntry GetEntryOrThrow(this ZipArchive archive, string entryName)
        =>
        archive.GetEntry(entryName) ?? throw new InvalidOperationException("Entry '{entryName}' must be not null");

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