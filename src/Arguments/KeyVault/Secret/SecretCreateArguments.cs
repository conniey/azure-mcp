// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Arguments.KeyVault.Key;

public class SecretCreateArguments : BaseKeyVaultArguments
{
    public string? SecretName { get; set; }
    public string? SecretType { get; set; }
}
