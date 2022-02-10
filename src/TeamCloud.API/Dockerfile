FROM mcr.microsoft.com/dotnet/aspnet:6.0

ARG image_version=unknown
ENV TEAMCLOUD_IMAGE_VERSION=$image_version

ENV PORT 8080
EXPOSE 8080

RUN mkdir -p /home/site/wwwroot
COPY publish /home/site/wwwroot

ENV ASPNETCORE_URLS "http://*:${PORT}"

WORKDIR /home/site/wwwroot

ENTRYPOINT [ "dotnet", "/home/site/wwwroot/TeamCloud.API.dll" ]