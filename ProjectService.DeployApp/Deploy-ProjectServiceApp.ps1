#
# Script to upload packaged Service Fabric application to cluster
# 
# Note: - package the service fabric application type and copy to $PackagePath location below
#       - update $ServiceFabricConnectionEndpoint below to point to your Service Fabric cluster
#
# Parameter help description
Param(
	[string] $PackagePath = "C:\Users\nickward\Source\Repos\ProjectService\ProjectService\pkg\Release",
	[string] $ServiceFabricConnectionEndpoint = "cadclustermmpnwoqqgktfk.australiasoutheast.cloudapp.azure.com:19000"
)

Write-Host "Importing Service Fabric SDK module..." -NoNewline
Import-Module "$ENV:ProgramFiles\Microsoft SDKs\Service Fabric\Tools\PSModule\ServiceFabricSDK\ServiceFabricSDK.psm1"
Write-Host "done!"

# Test if package is ready for uploading to Azure Service Fabric
Write-Host "Testing to see if package is ready for uploading to Azure Service Fabric..." -NoNewline
$result = Test-ServiceFabricApplicationPackage $PackagePath
if($result -ne "True")
{
	Write-Host " Package failed test. Please verify package is ready for uploading to Azure Service Fabric." -ForegroundColor Red
	break
} else {
	Write-Host "done!"
}

# Connect to the Azure Service Fabric Cluster
Write-Host "Connecting to the Azure Service Fabric Cluster at " -NoNewline; Write-Host $ServiceFabricConnectionEndpoint -NoNewline; Write-Host "..." -NoNewline
$serviceFabricClusterConnection = Connect-ServiceFabricCluster -ConnectionEndpoint $ServiceFabricConnectionEndpoint
Write-Host "done!"

# Upload application package to cluster Image Store
Write-Host "Uploading application package to the cluster image store..." -NoNewline
$result = Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $PackagePath -ImageStoreConnectionString (Get-ImageStoreConnectionStringFromClusterManifest(Get-ServiceFabricClusterManifest)) -Verbose
Write-Host $result

# Register the application type from the uploaded package
Write-Host "Registering the application type ProjectServiceType..." -NoNewline
$result = Register-ServiceFabricApplicationType ProjectServiceType
Write-Host "done!"

# Create the application
Write-Host "Creating an instance of ProjectServiceType..." -NoNewline
$result = New-ServiceFabricApplication -ApplicationName fabric:/ProjectService -ApplicationTypeName "ProjectServiceType" -ApplicationTypeVersion "1.0.0"
Write-Host "done!"
