FROM mcr.microsoft.com/dotnet/sdk:6.0 AS installer-env

RUN mkdir -p /home/site/wwwroot
COPY publish /home/site/wwwroot

# To enable ssh & remote debugging on app service change use the -appservice
FROM mcr.microsoft.com/azure-functions/dotnet:4-appservice
# FROM mcr.microsoft.com/azure-functions/dotnet:4

ARG image_version=unknown
ENV TEAMCLOUD_IMAGE_VERSION=$image_version

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]