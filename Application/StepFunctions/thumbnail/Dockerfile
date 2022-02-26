FROM public.ecr.aws/lambda/dotnet:6 AS base

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /src
COPY ["thumbnail/thumbnail.csproj", "thumbnail/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Common/Common.csproj"
RUN dotnet restore "thumbnail/thumbnail.csproj"

WORKDIR "/src/thumbnail"
COPY "thumbnail/" .
COPY "Common/" .
RUN dotnet build "thumbnail.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "thumbnail.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .