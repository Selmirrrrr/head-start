FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/BFF/HeadStart.BFF.csproj", "src/BFF/"]
COPY ["src/Client/HeadStart.Client.csproj", "src/Client/"]
COPY ["src/SharedKernel.Models/HeadStart.SharedKernel.Models.csproj", "src/SharedKernel.Models/"]
COPY ["src/Aspire/ServiceDefaults/HeadStart.Aspire.ServiceDefaults.csproj", "src/Aspire/ServiceDefaults/"]
COPY ["src/SharedKernel/HeadStart.SharedKernel.csproj", "src/SharedKernel/"]
RUN dotnet restore "src/BFF/HeadStart.BFF.csproj"
COPY . .
WORKDIR "/src/src/BFF"
RUN dotnet build "./HeadStart.BFF.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HeadStart.BFF.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HeadStart.BFF.dll"]
