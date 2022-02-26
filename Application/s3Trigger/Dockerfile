FROM public.ecr.aws/lambda/dotnet:6 AS base

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim as build
WORKDIR /src
COPY ["s3Trigger.csproj", "."]
RUN dotnet restore "./s3Trigger.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "s3Trigger.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "s3Trigger.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .