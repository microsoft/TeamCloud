#!/bin/sh
set -e

rgx='^[0-9]+([.][0-9]+)*$'

cdir=$(cd -P -- "$(dirname -- "$0")" && pwd -P)

teamCloudDir=${cdir%/*}

azureDeploy="$cdir/azuredeploy.json"

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

  -d  TeamCloud Deploy YAML: The location of the teamcloud.deploy.yaml configuraiton to use.
        The values for Azure Region and Azure Subscription ID will be used during deployment.

  -c  TeamCloud Config YAML: The location of the teamcloud.config.yaml configuraiton to use.
        If this argument is ommited, the teamcloud.config.yaml file must be sent via a POST
        request to 'api/config' before the TeamCloud service can be used.

Examples:

    $ deploy.sh -n teamcloudapp -g TeamCloud -d teamcloud.deploy.yaml -c teamcloud.config.yaml

endHelp
)

# show help text if called with no args
if (($# == 0)); then
    echo "$helpText" >&2; exit 0
fi

# get arg values
while getopts ":n:g:d:c:h:" opt; do
    case $opt in
        n)  appName=$OPTARG;;
        g)  resourceGroupName=$OPTARG;;
        d)  teamCloudDeployYaml=$OPTARG;;
        c)  teamCloudConfigYaml=$OPTARG;;
        h)  echo "$helpText" >&2; exit 0;;
        \?) echo "    Invalid option -$OPTARG $helpText" >&2; exit 1;;
        :)  echo "    Option -$OPTARG requires an argument $helpText." >&2; exit 1;;
    esac
done

if [ ! -f "$teamCloudDeployYaml" ]; then
	echo "$teamCloudDeployYaml not found.  Please check the path is correct and try again."
	exit 1
fi

if [ ! -f "$teamCloudConfigYaml" ]; then
	echo "$teamCloudConfigYaml not found.  Please check the path is correct and try again."
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
echo "  parsing teamcloud deploy yaml file..."

eval $(parse_yaml "$teamCloudDeployYaml" "deploy_")

# set azure variables
azureRegion="$deploy_azure_region"
azureSubscriptionId="$deploy_azure_subscriptionId"

azureActiveDirectoryClientId="$deploy_azure_activeDirectory_clientId"
azureActiveDirectoryClientSecret="$deploy_azure_activeDirectory_clientSecret"

azureResourceManagerClientId="$deploy_azure_resourceManager_clientId"
azureResourceManagerClientSecret="$deploy_azure_resourceManager_clientSecret"

# name the deployment to get outputs later
teamCloudDeploymentName="teamCloudDeployment"

# echo ""
# echo "  Deploying with the following paramaters:"
# echo "    App Name: $appName"
# echo "    Resource Group Name: $resourceGroupName"
# echo "    Azure Region (location): $azureRegion"
# echo "    Azure Subscription ID: $azureSubscriptionId"
# echo "    Deployment Name: $teamCloudDeploymentName"
# echo "    azureActiveDirectoryClientId: $azureActiveDirectoryClientId"
# echo "    azureActiveDirectoryClientSecret: $azureActiveDirectoryClientSecret"
# echo "    azureResourceManagerClientId: $azureResourceManagerClientId"
# echo "    azureResourceManagerClientSecret: $azureResourceManagerClientSecret"

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

echo "  checking for an existing resource group named '$resourceGroupName'..."

# check for an existing resource group
az group show -g $resourceGroupName --subscription $azureSubscriptionId 1> /dev/null


if [ $? != 0 ]; then
	echo "  ...creating new resource group named '$resourceGroupName'\n"
    set -e
    (
	    az group create -n $resourceGroupName -l $azureRegion --subscription $azureSubscriptionId 1> /dev/null
    )
else
	echo "  ...found an existing resource group named '$resourceGroupName'\n"
fi


# deploy

echo "  deploying resources to azure (grab a coffee, this will take several minutes)..."

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
	echo "  successfully deployed azure resources\n"
fi

tempConfigServiceImport="$cdir/tempConfigServiceImport.json"


echo "  getting configuration details from deployment...\n"

# get the url for the teamcloud api
apiUrl=$(az group deployment show -n $teamCloudDeploymentName -g $resourceGroupName --query properties.outputs.apiUrl.value | tr -d \")
configUrl="$appUrl/api/config"

# get the name of the new app configuration service
configServiceName=$(az group deployment show -n $teamCloudDeploymentName -g $resourceGroupName --query properties.outputs.configServiceName.value | tr -d \")
# configServiceName="$appName-config"

# get the configuration output and pipe it to a temporary json file
az group deployment show -n $teamCloudDeploymentName -g $resourceGroupName --query properties.outputs.configServiceImport.value > $tempConfigServiceImport

echo "  adding configuration details to azure configuration service..."

# import contents of the json file to the new configuration service
az appconfig kv import --name $configServiceName \
                       --subscription $azureSubscriptionId \
                       --source file \
                       --format json \
                       --separator ':' \
                       --path $tempConfigServiceImport \
                       --yes # don't prompt for confirmaiton

rm -f $tempConfigServiceImport

echo "  successfully added configuration details to azure configuration service\n"

echo "  posting teamcloud config yaml to new teamcloud instance..."

# store the whole response with the status at the and
postConfigResponse=$(curl -s -w "HTTPSTATUS:%{http_code}" -X "POST" -H 'Content-Type: application/x-yaml' -d @$teamCloudConfigYaml $configUrl)

# extract the body and status
postConfigResponseBody=$(echo $postConfigResponse | sed -e 's/HTTPSTATUS\:.*//g')
postConfigResponseStatusCode=$(echo $postConfigResponse | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')

if [ $postConfigResponseStatusCode -eq 202 ]; then

    echo "  ...teamcloud yonfig yaml accepted.\n"
    echo "  polling teamcloud instance status api..."

    postConfigStatusUrl=$(echo $postConfigResponseBody | grep -o '"status": *"[^"]*"' | grep -o '"[^"]*"$' | tr -d \")
    postConfigRuntimeStatus=$(echo $postConfigResponseBody | grep -o '"runtimeStatus": *"[^"]*"' | grep -o '"[^"]*"$' | tr -d \")
    postConfigCustomStatus=$(echo $postConfigResponseBody | grep -o '"customStatus": *"[^"]*"' | grep -o '"[^"]*"$' | tr -d \")

    while true
    do
        statusResponseBody=$(curl -s -w "HTTPSTATUS:%{http_code}" $postConfigStatusUrl)
        statusResponseStatusCode=$(echo $statusResponseBody | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
        statusResponseRuntimeStatus=$(echo $statusResponseBody | grep -o '"runtimeStatus": *"[^"]*"' | grep -o '"[^"]*"$' | tr -d \")
        statusResponseCustomStatus=$(echo $statusResponseBody | grep -o '"customStatus": *"[^"]*"' | grep -o '"[^"]*"$' | tr -d \")

        if [ $statusResponseStatusCode -eq 200 ]; then
            if [ "$statusResponseRuntimeStatus" = "Canceled" ] || [ "$statusResponseRuntimeStatus" = "Completed" ] || [ "$statusResponseRuntimeStatus" = "Terminated" ]; then
                echo "  teamcloud config setup completed with status: $statusResponseRuntimeStatus\n"
                break
            else
                echo "  ...$statusResponseRuntimeStatus : $statusResponseCustomStatus"
            fi
        else
            echo "/nError checking teamcloud config setup state.."
            exit 1;
        fi

        sleep 10
    done

else

    echo "/nError posting teamcloud config yaml to new teamcloud instance:"
    echo $postConfigResponseBody
    exit 1;

fi

echo "\nTeamCloud deployment successful!\n"
