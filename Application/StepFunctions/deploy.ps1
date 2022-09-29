docker build . -t net-7-native-builder
docker run --volume C:\source\repos\dotnet-serverless-imagerecognition\Application\StepFunctions\:/workingdir --name net7-native-build-container -i net-7-native-builder dotnet publish /workingdir/extract-image-metadata/extract-image-metadata.csproj -r linux-x64 -c Release --self-contained --output /workingdir/extract-image-metadata/publish
rm extract-image-metadata.zip
Compress-Archive /workingdir/extract-image-metadata/* extract-image-metadata.zip
docker rm net7-native-build-container