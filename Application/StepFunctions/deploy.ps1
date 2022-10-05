docker build . -t net-7-native-builder
docker run --volume C:\source\repos\dotnet-serverless-imagerecognition\Application\StepFunctions\:/workingdir --name net7-native-build-container -i net-7-native-builder dotnet publish /workingdir/extract-image-metadata/extract-image-metadata.csproj -r linux-x64 -c Release --self-contained --output /workingdir/extract-image-metadata/publish
remove-item .\transform-metadata.zip
Compress-Archive .\transform-metadata\publish\* transform-metadata.zip
docker rm net7-native-build-container
dotnet lambda package-ci --template serverless.template --output-template updated.template --s3-bucket img-imagerecognitionartifactstore-12acumvjm8twe --s3-prefix transform
aws cloudformation deploy --template-file updated.template --region us-east-2 --stack-name img2 --capabilities CAPABILITY_IAM