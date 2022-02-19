#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RabbitMQCommunication/RabbitMQCommunication.csproj", "RabbitMQCommunication/"]
RUN dotnet restore "RabbitMQCommunication/RabbitMQCommunication.csproj"
COPY . .
WORKDIR "/src/RabbitMQCommunication"
RUN dotnet build "RabbitMQCommunication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RabbitMQCommunication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMQCommunication.dll"]