FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./

# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App

# Set environment
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development
ENV APP_VERSION=24.08.08

EXPOSE 5000

COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "CusomMapOSM_API.dll"]