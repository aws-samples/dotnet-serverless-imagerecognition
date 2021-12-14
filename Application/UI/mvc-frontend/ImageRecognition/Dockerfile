#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ImageRecognition.Frontend.csproj", "."]
RUN dotnet restore "./ImageRecognition.Frontend.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ImageRecognition.Frontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ImageRecognition.Frontend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageRecognition.Frontend.dll"]