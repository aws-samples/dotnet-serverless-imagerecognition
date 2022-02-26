FROM public.ecr.aws/lambda/dotnet:6.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim as build
WORKDIR /src
COPY ["store-image-metadata/store-image-metadata.csproj", "store-image-metadata/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Common/Common.csproj"
RUN dotnet restore "store-image-metadata/store-image-metadata.csproj"

WORKDIR "/src/store-image-metadata"
COPY "store-image-metadata/" .
COPY "Common/" .
RUN dotnet build "store-image-metadata.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "store-image-metadata.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .