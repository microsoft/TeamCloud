targetScope = 'subscription'

@minLength(36)
@maxLength(36)
param projectId string = '00000000-0000-0000-0000-000000000000'
param projectName string = ''
param projectTags object = {}
param deploymentScopes array = []

@minLength(36)
@maxLength(36)
param organizationId string
param organizationName string
param organizationTags object = {}

var deployProject = !empty(projectName) && projectId != '00000000-0000-0000-0000-000000000000'
var projectResourceGroupName = 'TCP-${projectName}-${uniqueString(projectId)}'
var projectDeploymentName = take('${deployment().name}-${uniqueString(projectId, 'resources')}', 64)

var deployOrganization = !deployProject
var organizationResourceGroupName = 'TCO-${organizationName}-${uniqueString(organizationId)}'
var organizationDeploymentName = take('${deployment().name}-${uniqueString(organizationId, 'resources')}', 64)

resource organizationResourceGroup 'Microsoft.Resources/resourceGroups@2019-10-01' = if (deployOrganization) {
  name: organizationResourceGroupName
  location: deployment().location
  tags: organizationTags
}

module organizationResources './organizationResources.bicep' = if (deployOrganization) {
  name: organizationDeploymentName
  scope: organizationResourceGroup
  params: {
    tags: organizationTags
  }
}

resource projectResourceGroup 'Microsoft.Resources/resourceGroups@2019-10-01' = if (deployProject) {
  name: projectResourceGroupName
  location: deployment().location
  tags: projectTags
}

module projectResources './projectResources.bicep' = if (deployProject) {
  name: projectDeploymentName
  scope: projectResourceGroup
  params: {
    tags: projectTags
    deploymentScopes: deploymentScopes
  }
}

output organizationData object = deployOrganization ? organizationResources.outputs.organizationData : {}
output projectData object = deployProject ? projectResources.outputs.projectData : {}
