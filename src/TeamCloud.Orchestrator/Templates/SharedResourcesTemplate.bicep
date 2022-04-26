targetScope = 'subscription'

@minLength(36)
@maxLength(36)
param projectId string = '00000000-0000-0000-0000-000000000000'
param projectName string = ''
param projectTags object = {}

@minLength(36)
@maxLength(36)
param organizationId string
param organizationSlug string
param organizationTags object = {}

param deploymentScopes array = []
param location string = deployment().location

var deployProject = !empty(projectName) && projectId != '00000000-0000-0000-0000-000000000000'
var projectResourceGroupName = 'TCP-${projectName}-${uniqueString(projectId)}'
var projectDeploymentName = take('${deployment().name}-project', 64)

var deployOrganization = !deployProject
var organizationResourceGroupName = 'TCO-${organizationSlug}-${uniqueString(organizationId)}'
var organizationDeploymentName = take('${deployment().name}-organization', 64)

resource organizationResourceGroup 'Microsoft.Resources/resourceGroups@2019-10-01' = if (deployOrganization) {
  name: organizationResourceGroupName
  location: location
  tags: organizationTags
}

module organizationResources './organizationResources.bicep' = if (deployOrganization) {
  name: organizationDeploymentName
  scope: organizationResourceGroup
  params: {
    organizationTags: organizationTags
    location: location
  }
}

resource projectResourceGroup 'Microsoft.Resources/resourceGroups@2019-10-01' = if (deployProject) {
  name: projectResourceGroupName
  location: location
  tags: projectTags
}

module projectResources './projectResources.bicep' = if (deployProject) {
  name: projectDeploymentName
  scope: projectResourceGroup
  params: {
    tags: projectTags
    deploymentScopes: deploymentScopes
    location: location
  }
}

output organizationData object = deployOrganization ? organizationResources.outputs.organizationData : {}
output projectData object = deployProject ? projectResources.outputs.projectData : {}
