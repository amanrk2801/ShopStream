FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["ShopStream.Api/ShopStream.Api.csproj", "ShopStream.Api/"]
COPY ["ShopStream.Services/ShopStream.Services.csproj", "ShopStream.Services/"]
COPY ["ShopStream.Data/ShopStream.Data.csproj", "ShopStream.Data/"]
COPY ["ShopStream.Core/ShopStream.Core.csproj", "ShopStream.Core/"]

RUN dotnet restore "ShopStream.Api/ShopStream.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ShopStream.Api"
RUN dotnet build "ShopStream.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShopStream.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopStream.Api.dll"]
