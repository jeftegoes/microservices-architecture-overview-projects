#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CustomerNotification.Service/Notification.Service/Notification.Service.csproj", "CustomerNotification.Service/Notification.Service/"]
COPY ["CustomerNotification.Service/Email.Service/Email.Service.csproj", "CustomerNotification.Service/Email.Service/"]
COPY ["Messaging.Interfaces.SharedLib/Messaging.Interfaces.SharedLib.csproj", "Messaging.Interfaces.SharedLib/"]
RUN dotnet restore "CustomerNotification.Service/Notification.Service/Notification.Service.csproj"
COPY . .
WORKDIR "/src/CustomerNotification.Service/Notification.Service"
RUN dotnet build "Notification.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.Service.dll"]