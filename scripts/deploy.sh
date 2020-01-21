#!/bin/sh
set -e

rgx='^[0-9]+([.][0-9]+)*$'

cdir=$(cd -P -- "$(dirname -- "$0")" && pwd -P)

teamCloudDir=${cdir%/*}

azureDeploy="$teamCloudDir/azuredeploy.json"

helpText=$(cat << endHelp

TeamCloud Deploy Utility

Options:
  -h  View this help output text again.

  -n  App Name: The name of the TeamCloud instance that you wish to create.
        This will also be used as the subdomain of your service endpoint (i.e. myteamcloud.azurewebsites.net).
        Valid characters are: 0-9, a-z, A-Z, and -

  -g  Resource Group: The Name for the new Azure Resource Group to create.
        The value must be a string.  Resource Group names are case insensitive.
        Alphanumeric, underscore, parentheses, hyphen, period (except at the end) are valid.

  -y  TeamCloud YAML: The location of the teamcloud.yaml configuraiton to use.
        The values for Azure Region and Azure Subscription ID will be used during deployment.


Examples:

  Increment the build number by one
    $ deploy.sh -n teamcloudapp -g TeamCloud -y teamcloud.yaml

endHelp
)

# show help text if called with no args
if (($# == 0)); then
    echo "$helpText" >&2; exit 0
fi

# get arg values
while getopts ":n:g:y:h:" opt; do
    case $opt in
        n)  appName=$OPTARG;;
        g)  resourceGroupName=$OPTARG;;
        y)  teamCloudYaml=$OPTARG;;
        h)  echo "$helpText" >&2; exit 0;;
        \?) echo "    Invalid option -$OPTARG $helpText" >&2; exit 1;;
        :)  echo "    Option -$OPTARG requires an argument $helpText." >&2; exit 1;;
    esac
done

if [ ! -f "$teamCloudYaml" ]; then
	echo "$teamCloudYaml not found.  Please check the path is correct and try again."
	exit 1
fi

# check for the azure cli
if ! [ -x "$(command -v az)" ]; then
  echo 'Error: az command is not installed.\nThe Azure CLI is required to run this deploy script.  Please install the Azure CLI, run az login, then try again.  Aborting.' >&2
  exit 1
fi

# simple utility to parse yaml
parse_yaml() {
   local prefix=$2
   local s='[[:space:]]*' w='[a-zA-Z0-9_]*' fs=$(echo @|tr @ '\034')
   sed -ne "s|^\($s\)\($w\)$s:$s\"\(.*\)\"$s\$|\1$fs\2$fs\3|p" \
        -e "s|^\($s\)\($w\)$s:$s\(.*\)$s\$|\1$fs\2$fs\3|p"  $1 |
   awk -F$fs '{
      indent = length($1)/2;
      vname[indent] = $2;
      for (i in vname) {if (i > indent) {delete vname[i]}}
      if (length($3) > 0) {
         vn=""; for (i=0; i<indent; i++) {vn=(vn)(vname[i])("_")}
         printf("%s%s%s=\"%s\"\n", "'$prefix'",vn, $2, $3);
      }
   }'
}

echo ""
echo "  Parsing teamcloud.yaml file..."

eval $(parse_yaml "$teamCloudYaml" "config_")

# set azure variables
azureRegion="$config_azure_region"
azureSubscriptionId="$config_azure_subscriptionId"

azureActiveDirectoryClientId="$config_azure_activeDirectory_clientId"
azureActiveDirectoryClientSecret="$config_azure_activeDirectory_clientSecret"

azureResourceManagerClientId="$config_azure_resourceManager_clientId"
azureResourceManagerClientSecret="$config_azure_resourceManager_clientSecret"

# name the deployment to get outputs later
teamCloudDeploymentName="teamCloudDeploymentNameOne"

echo ""
echo "  Deploying with the following paramaters:"
echo "    App Name: $appName"
echo "    Resource Group Name: $resourceGroupName"
echo "    Azure Region (location): $azureRegion"
echo "    Azure Subscription ID: $azureSubscriptionId"
echo "    Deployment Name: $teamCloudDeploymentName"
echo "    azureActiveDirectoryClientId: $azureActiveDirectoryClientId"
echo "    azureActiveDirectoryClientSecret: $azureActiveDirectoryClientSecret"
echo "    azureResourceManagerClientId: $azureResourceManagerClientId"
echo "    azureResourceManagerClientSecret: $azureResourceManagerClientSecret"

# check if logged in to azure cli
az account show -s $azureSubscriptionId 1> /dev/null

if [ $? != 0 ];
then
	az login
fi

# set the subscriptionId as active
#  az account set -s $azureSubscriptionId

# exit 0

# remove e so `az group show` won't exit if an existing group isn't found
set +e

echo ""
echo "  Checking for an existing resource group named '$resourceGroupName'..."

# check for an existing resource group
az group show -g $resourceGroupName --subscription $azureSubscriptionId 1> /dev/null


if [ $? != 0 ]; then
	echo "  Creating new resource group named '$resourceGroupName'..."
    set -e
    (
	    az group create -n $resourceGroupName -l $azureRegion --subscription $azureSubscriptionId 1> /dev/null
    )
else
	echo "  Found an existing resource group named '$resourceGroupName'."
fi


# deploy

echo ""
echo "  Deploying Azure resources (grab a coffee, this will take several minutes)..."

set -e
(
    # az group deployment validate -g $resourceGroupName
    az group deployment create -n $teamCloudDeploymentName -g $resourceGroupName \
        --subscription $azureSubscriptionId \
        --template-file $azureDeploy \
        --parameters webAppName=$appName \
                     activeDirectoryIdentityClientId=$azureActiveDirectoryClientId \
                     activeDirectoryIdentityClientSecret=$azureActiveDirectoryClientSecret \
                     resourceManagerIdentityClientId=$azureResourceManagerClientId \
                     resourceManagerIdentityClientSecret=$azureResourceManagerClientSecret \
                     deployTeamCloudSource=false
)

if [ $? == 0 ]; then
    echo ""
	echo "  Deployment suceeded."
fi

tempConfig="$cdir/tempConfig.json"


echo ""
echo "  Getting configuration details from deployment..."

# get the name of the new app configuration service
configName=$(az group deployment show -n $teamCloudDeploymentName -g $resourceGroupName --query properties.outputs.configName.value | tr -d \")
# configName="$appName-config"

# get the configuration output and pipe it to a temporary json file
az group deployment show -n $teamCloudDeploymentName -g $resourceGroupName --query properties.outputs.config.value > $tempConfig


echo ""
echo "  Adding configuration details to Azure Configuration Service..."

# import contents of the json file to the new configuration service
az appconfig kv import --name $configName \
                       --subscription $azureSubscriptionId \
                       --source file \
                       --format json \
                       --separator ':' \
                       --path $tempConfig \
                       --yes # don't prompt for confirmaiton

echo ""
# az rest -m post -u "https://$appName.azurewebsites.net/api/config" -b @$teamCloudYaml --headers Content-Type="application/x-yaml" --subscription $azureSubscriptionId