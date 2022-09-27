docker build . -t net-7-native-builder
docker run --volume C:\source\repos\dotnet-serverless-imagerecognition\Application\s3Trigger\:/workingdir --name net7-native-build-container -i net-7-native-builder dotnet publish /workingdir/s3Trigger.csproj -r linux-x64 -c Release
rm s3Trigger.zip
Compress-Archive .\bin\Release\net7.0\linux-x64\publish\* s3Trigger.zip
docker rm net7-native-build-container