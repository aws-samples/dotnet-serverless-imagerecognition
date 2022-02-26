FROM public.ecr.aws/lambda/dotnet:6.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim as build
WORKDIR /src
COPY ["rekognition/rekognition.csproj", "rekognition/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Common/Common.csproj"
RUN dotnet restore "rekognition/rekognition.csproj"

WORKDIR "/src/rekognition"
COPY "rekognition/" .
COPY "Common/" .
RUN dotnet build "rekognition.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "rekognition.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .