# ===================== build =====================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy sln and csprojs (efficient caching)
COPY Mtu.Rentals.sln .
COPY src/Mtu.Rentals.Api/Mtu.Rentals.Api.csproj src/Mtu.Rentals.Api/
COPY src/Mtu.Rentals.Application/Mtu.Rentals.Application.csproj src/Mtu.Rentals.Application/
COPY src/Mtu.Rentals.Domain/Mtu.Rentals.Domain.csproj src/Mtu.Rentals.Domain/
COPY src/Mtu.Rentals.Infrastructure/Mtu.Rentals.Infrastructure.csproj src/Mtu.Rentals.Infrastructure/
COPY src/Mtu.Rentals.Contracts/Mtu.Rentals.Contracts.csproj src/Mtu.Rentals.Contracts/

RUN dotnet restore src/Mtu.Rentals.Api/Mtu.Rentals.Api.csproj

# Copy all the code
COPY src/ src/

# Public
RUN dotnet publish src/Mtu.Rentals.Api/Mtu.Rentals.Api.csproj -c Release -o /app/publish

# ===================== runtime =====================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

# CNH image storage folder
RUN mkdir -p /app/storage
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Mtu.Rentals.Api.dll"]
