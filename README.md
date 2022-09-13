## Serverless Reference Architecture: Building End-to-End .NET Serverless Applications on AWS
The Image Recognition application illustrates modern, serverless, .NET application on AWS. It uses AWS SDK for .NET to work with AWS services from within application code, as well as tools from AWS to integrate with Visual Studio (on Windows) and the dotnet CLI (on Windows, macOS, and Linux) to make working with serverless application development on AWS easy and convenient.

Image recognition is an application that enables registered and signed-in users to upload and organize photos in Albums. After the  photo is submitted, the backend workflow processes photos and extracts metadata from the image such as geolocation, size/format, time, etc. It then uses image recognition to tag objects in the photo. In parallel, it also produces a thumbnail of the photo.

The Image Recognition sample application demonstrates:
* Building ASP.NET Web API project using JWT tokens vended by Cognito to handle authentication. The project is hosted in AWS Lambda using Lambda Container support and the AWS .NET tooling that simplifies using .NET5 Container with Lambda.
* Building frontend using ASP.NET new server-side Blazor web framework hosted in ECS using AWS Fargate.
* Using API Gateway's WebSocket support to build realtime two-way communication application. This project connect backend components to the frontend allowing backend systems to easily communicate their status to the user in real time.
* Using AWS Step Functions to orchestrate a serverless processing workflow built in .NET. This project orchestrate backend using AWS Lambda, Amazon S3, Amazon DynamoDB and Amazon Rekognition.

This repository contains sample code for all the Lambda functions depicted in the diagram below as well as an AWS CloudFormation template for creating the functions and related resources. There is also a test web app that you can run to interact with the backend.

![High level architecture diagram](/Images/ImageRecognitionArchitecture.png)

Following the instructions in this sample you will gain experience in deploying to and using the following AWS services, with the tools you are already familiar with - namely Visual Studio and AWS extensions for the dotnet CLI:

* Amazon Cognito
* Amazon DynamoDB
* Amazon ECS
* AWS Fargate
* AWS Lambda
* AWS Step Functions
* AWS Systems Manager

# Walkthrough of the architecture
1. An image is uploaded to the PhotoRepo S3 bucket under the private/{userid}/uploads prefix
1. The S3 upload event triggers the S3Trigger Lambda function, which kicks off an execution of the ImageProcStateMachine in AWS Step Functions, passing in the S3 bucket and object key as input parameters.
1. The ImageProcStateMachine has the following sub-steps:
1. Read the file from S3 and extract image metadata (format, EXIF data, size, etc.)
1. Based on output from previous step, validate if the file uploaded is a supported file format (png or jpg). If not, throw NotSupportedImageType error and end execution.
1. Store the extracted metadata in the ImageMetadata DynamoDB table
1. In parallel, kick off two processes simultaneously:
1. Call Amazon Rekognition to detect objects in the image file. If detected, store the tags in the ImageMetadata DynamoDB table
1. Generate a thumbnail and store it under the private/{userid}/resized prefix in the PhotoRepo S3 bucket

# How to deploy
Follow these instructions to deploy the application (both backend and frontend):
1. Fork this repository in your GitHub account.
2. Click Launch Stack to launch the template in your account and then click Next.
   
    [![Launch image recognition with CloudFormation](http://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/images/cloudformation-launch-stack-button.png)](https://console.aws.amazon.com/cloudformation/home?region=us-east-2#/stacks/new?stackName=image-recognition&templateURL=https://windows-dev-env-ec2.s3.amazonaws.com/dotnet-serverless-dev-cfn/imagerecognition-pipeline.template)

1. In the parameters section 
   1. Add Stack name as "ImageRecognition".
   2. Change the GitHubOwner to your account.
   3. Grant AWS CodePipeline access to your GitHub repository. 
      1. Create [personal access token](https://docs.github.com/en/github/authenticating-to-github/keeping-your-account-and-data-secure/creating-a-personal-access-token) to use in place of a password.
      2. Add GitHub repository OAuth token parameter to GitHubToken

    ![Cloudformation stack parameters](Images/cloudformation-parameters.png)

2. Click Next and Next.
    > In the last page of the wizard, make sure to click the checkboxes to accept:
    > 
    > * I acknowledge that AWS CloudFormation might create IAM resources.
    > * I acknowledge that AWS CloudFormation might create IAM resources with custom names.
    > * I acknowledge that AWS CloudFormation might require the following capability: `CAPABILITY_AUTO_EXPAND`
    > 
    > before clicking **"Create stack"**

# Using the test web app
### Sign up and log in
1. Go to the URL of the deployed app.

1. In the login page, click on "Create account"
   
2. Register an account by following the sign up instructions
   
   ![Register new user](/Images/app-signup.PNG)

3. After confirming the account, sign in

### Album List
1. create albums using the "Add a new album"
   ![create album](/Images/app-create-album.PNG)

### Photo gallery
1. Click into an album you created
1. Upload a photo
2. You can follow the Step Functions execution link to review the details of the workflow execution Below is the diagram of the state machine being executed every time a new image is uploaded (you can explore this in the Step Functions Console):
   
   ![step function execution](/Images/step-function-execution.PNG)
3. When the processing finishes, the photo and extracted information is added to the display
   
   ![create album](/Images/photo-analyzed.PNG)

# Cleaning Up the Application Resources
To remove all resources created by this example, do the following:

1. Go to AWS CloudFormation console
1. Expand the AWS CloudFormation node in the AWS Explorer. The sample consists of seven stacks:
   1. *StackName*-ECSFrontend
   1. *StackName*-API
   1. *StackName*-S3TriggerLambdaFunction
   1. *StackName*-StepFunctions
   1. *StackName*-Communications
   1. *StackName*-Infrastructure
   1. *StackName*
   \
    (*StackName* is the name of the stack you used in the **How to deploy** section.)
   \

   For each stack in order, right click and select Delete and respond to the confirmation prompt.
> DANGER! Be sure to select the correct stacks!

## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This library is licensed under the MIT-0 License. See the LICENSE file.
