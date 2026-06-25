FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["OrgHub.Api/OrgHub.Api.csproj", "OrgHub.Api/"]
COPY ["OrgHub.Application/OrgHub.Application.csproj", "OrgHub.Application/"]
COPY ["OrgHub.Core/OrgHub.Core.csproj", "OrgHub.Core/"]
COPY ["OrgHub.Domain/OrgHub.Domain.csproj", "OrgHub.Domain/"]
COPY ["OrgHub.Infrastructure/OrgHub.Infrastructure.csproj", "OrgHub.Infrastructure/"]

RUN dotnet restore "OrgHub.Api/OrgHub.Api.csproj"

COPY . .
WORKDIR "/src/OrgHub.Api"
RUN dotnet publish "OrgHub.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrgHub.Api.dll"]
