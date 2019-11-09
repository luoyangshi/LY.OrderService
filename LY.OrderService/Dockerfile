FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["LY.OrderService/LY.OrderService.csproj", "LY.OrderService/"]
RUN dotnet restore "LY.OrderService/LY.OrderService.csproj"
COPY . .
WORKDIR "/src/LY.OrderService"
RUN dotnet build "LY.OrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LY.OrderService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LY.OrderService.dll"]