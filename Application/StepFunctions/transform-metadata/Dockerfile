FROM public.ecr.aws/lambda/dotnet:6 AS base

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim as build
WORKDIR /src
COPY ["transform-metadata/transform-metadata.csproj", "transform-metadata/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Common/Common.csproj"
RUN dotnet restore "transform-metadata/transform-metadata.csproj"

WORKDIR "/src/transform-metadata"
COPY "transform-metadata/" .
COPY "Common/" .
RUN dotnet build "transform-metadata.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "transform-metadata.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .