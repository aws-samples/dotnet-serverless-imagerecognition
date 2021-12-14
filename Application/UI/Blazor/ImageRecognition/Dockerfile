FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["UI/Blazor/ImageRecognition/ImageRecognition.BlazorFrontend.csproj", "UI/Blazor/ImageRecognition/"]
COPY ["Clients/ImageRecognition.API.Client/ImageRecognition.API.Client.csproj", "Clients/ImageRecognition.API.Client/"]

RUN dotnet restore "UI/Blazor/ImageRecognition/ImageRecognition.BlazorFrontend.csproj"
COPY . .
WORKDIR "/src/UI/Blazor/ImageRecognition"
RUN dotnet build "ImageRecognition.BlazorFrontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ImageRecognition.BlazorFrontend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageRecognition.BlazorFrontend.dll"]