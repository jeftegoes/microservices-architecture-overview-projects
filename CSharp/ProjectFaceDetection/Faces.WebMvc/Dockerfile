#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Faces.WebMvc/Faces.WebMvc.csproj", "Faces.WebMvc/"]
COPY ["Messaging.Interfaces.SharedLib/Messaging.Interfaces.SharedLib.csproj", "Messaging.Interfaces.SharedLib/"]
RUN dotnet restore "Faces.WebMvc/Faces.WebMvc.csproj"
COPY . .
WORKDIR "/src/Faces.WebMvc"
RUN dotnet build "Faces.WebMvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Faces.WebMvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Faces.WebMvc.dll"]