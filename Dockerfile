# Stage 1: Build and publish ASP.NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["testingStuff.csproj", "."]
RUN dotnet restore "./testingStuff.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "testingStuff.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "testingStuff.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run both ASP.NET and PHP
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS aspnet
WORKDIR /app
COPY --from=publish /app/publish .

# Install PHP and configure Apache
FROM php:7.4-apache AS php
WORKDIR /var/www/html
COPY ./wwwroot /var/www/html

# Configuration for running both ASP.NET and PHP
COPY --from=aspnet /app /var/www/html/aspnet
RUN mv /var/www/html/aspnet/* /var/www/html/

# Expose ports
EXPOSE 80

# Start Apache
CMD ["apache2-foreground"]