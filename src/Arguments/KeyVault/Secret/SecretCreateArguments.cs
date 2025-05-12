// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.KeyVault.Secret;

public class SecretCreateArguments : BaseKeyVaultArguments
{
    public string? SecretName { get; set; }
    public string? SecretValue { get; set; }
}
