FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /App

# Copy project files first (for better layer caching)
COPY CusomMapOSM_Domain/CusomMapOSM_Domain.csproj ./CusomMapOSM_Domain/
COPY CusomMapOSM_Commons/CusomMapOSM_Commons.csproj ./CusomMapOSM_Commons/
COPY CusomMapOSM_Application/CusomMapOSM_Application.csproj ./CusomMapOSM_Application/
COPY CusomMapOSM_Infrastructure/CusomMapOSM_Infrastructure.csproj ./CusomMapOSM_Infrastructure/
COPY CusomMapOSM_API/CusomMapOSM_API.csproj ./CusomMapOSM_API/

# Restore dependencies for each project (in dependency order)
RUN dotnet restore CusomMapOSM_Domain/CusomMapOSM_Domain.csproj
RUN dotnet restore CusomMapOSM_Commons/CusomMapOSM_Commons.csproj
RUN dotnet restore CusomMapOSM_Application/CusomMapOSM_Application.csproj
RUN dotnet restore CusomMapOSM_Infrastructure/CusomMapOSM_Infrastructure.csproj
RUN dotnet restore CusomMapOSM_API/CusomMapOSM_API.csproj

# Copy everything else
COPY . ./

# Build and publish the API project
RUN dotnet publish CusomMapOSM_API/CusomMapOSM_API.csproj -c Release -o /App/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV APP_VERSION=24.08.08

EXPOSE 5000

# Copy published files
COPY --from=build-env /App/out .

ENTRYPOINT ["dotnet", "CusomMapOSM_API.dll"]