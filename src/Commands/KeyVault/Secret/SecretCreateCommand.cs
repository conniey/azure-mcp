// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Security.KeyVault.Secrets;
using AzureMcp.Arguments.KeyVault.Secret;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace AzureMcp.Commands.KeyVault.Secret;

public sealed class SecretCreateCommand(ILogger<SecretCreateCommand> logger) : SubscriptionCommand<SecretCreateArguments>
{
    private readonly ILogger<SecretCreateCommand> _logger = logger;
    private readonly Option<string> _vaultOption = ArgumentDefinitions.KeyVault.VaultName.ToOption();
    private readonly Option<string> _secretNameOption = ArgumentDefinitions.KeyVault.SecretName.ToOption();
    private readonly Option<string> _secretValueOption = ArgumentDefinitions.KeyVault.SecretValue.ToOption();

    protected override string GetCommandName() => "create";

    protected override string GetCommandDescription() =>
        """
        Create a new secret in an Azure Key Vault. This command creates a secret with the specified name and value
        in the given vault.

        Required arguments:
        - subscription
        - vault-name
        - secret-name
        - secret-value
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_vaultOption);
        command.AddOption(_secretNameOption);
        command.AddOption(_secretValueOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateVaultNameArgument());
        AddArgument(CreateSecretNameArgument());
        AddArgument(CreateSecretValueArgument());
    }

    private static ArgumentBuilder<SecretCreateArguments> CreateVaultNameArgument() =>
        ArgumentBuilder<SecretCreateArguments>
            .Create(ArgumentDefinitions.KeyVault.VaultName.Name, ArgumentDefinitions.KeyVault.VaultName.Description)
            .WithValueAccessor(args => args.VaultName ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.KeyVault.VaultName.Required);

    private static ArgumentBuilder<SecretCreateArguments> CreateSecretNameArgument() =>
        ArgumentBuilder<SecretCreateArguments>
            .Create(ArgumentDefinitions.KeyVault.SecretName.Name, ArgumentDefinitions.KeyVault.SecretName.Description)
            .WithValueAccessor(args => args.SecretName ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.KeyVault.SecretName.Required);

    private static ArgumentBuilder<SecretCreateArguments> CreateSecretValueArgument() =>
        ArgumentBuilder<SecretCreateArguments>
            .Create(ArgumentDefinitions.KeyVault.SecretValue.Name, ArgumentDefinitions.KeyVault.SecretValue.Description)
            .WithValueAccessor(args => args.SecretValue ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.KeyVault.SecretValue.Required);

    protected override SecretCreateArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.VaultName = parseResult.GetValueForOption(_vaultOption);
        args.SecretName = parseResult.GetValueForOption(_secretNameOption);
        args.SecretValue = parseResult.GetValueForOption(_secretValueOption);
        return args;
    }

    [McpServerTool(Destructive = false, ReadOnly = false)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            if (!await ProcessArguments(context, args))
            {
                return context.Response;
            }

            var service = context.GetService<IKeyVaultService>();
            var secret = await service.CreateSecret(
                args.VaultName!,
                args.SecretName!,
                args.SecretValue!,
                args.Subscription!,
                args.Tenant);

            context.Response.Results = ResponseResult.Create(
                new SecretCreateCommandResult(secret.Name, secret.Value),
                KeyVaultJsonContext.Default.SecretCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating secret {SecretName} in vault {VaultName}", args.SecretName, args.VaultName);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    internal record SecretCreateCommandResult(string Name, string Value);
}
