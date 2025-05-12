using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Arguments.KeyVault.Secret;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;

namespace AzureMcp.Commands.KeyVault.Secret;

public sealed class SecretGetCommand : SubscriptionCommand<SecretGetArguments>
{
    private readonly Option<string> _secretNameOption = ArgumentDefinitions.KeyVault.SecretName.ToOption();

    protected override string GetCommandName() => "get";

    protected override string GetCommandDescription() =>
        """
        Get a secret from an Azure Key Vault. This command retrieves and displays details
        about a specific key in the specified vault.

        Required arguments:
        - subscription
        - vault
        - key
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_secretNameOption);
    }
    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateVaultNameArgument());
        AddArgument(CreateSecretNameArgument());
    }

    private static ArgumentBuilder<SecretGetArguments> CreateVaultNameArgument() =>
        ArgumentBuilder<SecretGetArguments>
            .Create(ArgumentDefinitions.KeyVault.VaultName.Name, ArgumentDefinitions.KeyVault.VaultName.Description)
            .WithValueAccessor(args => args.VaultName ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.KeyVault.VaultName.Required);

    private static ArgumentBuilder<SecretGetArguments> CreateSecretNameArgument() =>
        ArgumentBuilder<SecretGetArguments>
            .Create(ArgumentDefinitions.KeyVault.SecretName.Name, ArgumentDefinitions.KeyVault.SecretName.Description)
            .WithValueAccessor(args => args.SecretName ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.KeyVault.SecretName.Required);

    protected override SecretGetArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.SecretName = parseResult.GetValueForOption(_secretNameOption);
        return args;
    }

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
            var secretValue = await service.GetSecret(
                args.SecretName!,
                args.Subscription!,
                args.Tenant!);

            context.Response.Results = ResponseResult.Create(
                new SecretGetCommandResult(secretValue.Name, secretValue.Value),
                KeyVaultJsonContext.Default.SecretGetCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    internal record SecretGetCommandResult(string SecretName, string SecretValue);
}
