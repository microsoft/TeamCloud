/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Secrets;

public interface ISecretsStoreProvider
{
    Task<ISecretsStore> GetSecretsStoreAsync(Organization organization);

    Task<ISecretsStore> GetSecretsStoreAsync(Project project);
}
