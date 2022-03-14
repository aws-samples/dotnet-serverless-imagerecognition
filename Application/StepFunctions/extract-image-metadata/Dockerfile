FROM public.ecr.aws/lambda/dotnet:6 AS base

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /src
COPY ["extract-image-metadata/extract-image-metadata.csproj", "extract-image-metadata/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Common/Common.csproj"
RUN dotnet restore "extract-image-metadata/extract-image-metadata.csproj"

WORKDIR "/src/extract-image-metadata"
COPY "extract-image-metadata/" .
COPY "Common/" .
RUN dotnet build "extract-image-metadata.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "extract-image-metadata.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .