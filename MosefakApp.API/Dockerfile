FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY ["MosefakApp.Domains/MosefakApp.Domains.csproj", "MosefakApp.Domains/"]
COPY ["MosefakApp.Shared/MosefakApp.Shared.csproj", "MosefakApp.Shared/"]
COPY ["MosefakApp.Core/MosefakApp.Core.csproj", "MosefakApp.Core/"]
COPY ["MosefakApp.Infrastructure/MosefakApp.Infrastructure.csproj", "MosefakApp.Infrastructure/"]
COPY ["MosefakApp.API/MosefakApp.API.csproj", "MosefakApp.API/"]
COPY ["MosefakApi.DependencyInjection/MosefakApi.DependencyInjection.csproj", "MosefakApi.DependencyInjection/"]
COPY ["MosefakApi.Business/MosefakApi.Business.csproj", "MosefakApi.Business/"]
RUN dotnet restore "MosefakApp.API/MosefakApp.API.csproj" -a $TARGETARCH
COPY . .
WORKDIR "/src/MosefakApp.API"
RUN dotnet build "MosefakApp.API.csproj" -c Release -o /app/build -a $TARGETARCH

FROM build AS publish
RUN apt-get update -yq \
    && apt-get install -yq ca-certificates curl gnupg \
    && mkdir -p /etc/apt/keyrings \
    && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
    && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_18.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list \
    && apt-get update -yq \
    && apt-get install nodejs -yq
RUN dotnet publish "MosefakApp.API.csproj" -c Release -o /app/publish -a $TARGETARCH

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MosefakApp.API.dll"]
