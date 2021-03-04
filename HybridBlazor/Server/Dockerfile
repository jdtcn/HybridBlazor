FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HybridBlazor/Server/HybridBlazor.Server.csproj", "HybridBlazor/Server/"]
COPY ["HybridBlazor/Client/HybridBlazor.Client.csproj", "HybridBlazor/Client/"]
COPY ["HybridBlazor/Shared/HybridBlazor.Shared.csproj", "HybridBlazor/Shared/"]
RUN dotnet restore "HybridBlazor/Server/HybridBlazor.Server.csproj"
COPY . .
WORKDIR "/src/HybridBlazor/Server"
RUN dotnet build "HybridBlazor.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HybridBlazor.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HybridBlazor.Server.dll"]