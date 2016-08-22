#
# Script to upload packaged Service Fabric application to cluster
# 
# Note: - package the service fabric application type and copy to $packagePath location below
#       - update $serviceFabricConnectionEndpoint below to point to your Service Fabric cluster
#

Param(
	[string] $packagePath = "C:\ServiceFabric\ProjectServiceType",
	[string] $serviceFabricConnectionEndpoint = "cadclustermmpnwoqqgktfk.australiasoutheast.cloudapp.azure.com:19000"
	)

Write-Host "Importing Service Fabric SDK module..." -NoNewline
Import-Module "$ENV:ProgramFiles\Microsoft SDKs\Service Fabric\Tools\PSModule\ServiceFabricSDK\ServiceFabricSDK.psm1"
Write-Host "done!"

# Test if package is ready for uploading to Azure Service Fabric
Write-Host "Testing to see if package is ready for uploading to Azure Service Fabric..." -NoNewline
$result = Test-ServiceFabricApplicationPackage $packagePath
if($result -ne "True")
{
	Write-Host " Package failed test. Please verify package is ready for uploading to Azure Service Fabric." -ForegroundColor Red
	break
} else {
	Write-Host "done!"
}

# Connect to the Azure Service Fabric Cluster
Write-Host "Connecting to the Azure Service Fabric Cluster at " -NoNewline; Write-Host $serviceFabricConnectionEndpoint -NoNewline; Write-Host "..." -NoNewline
$serviceFabricClusterConnection = Connect-ServiceFabricCluster -ConnectionEndpoint $serviceFabricConnectionEndpoint
Write-Host "done!"

# Upload application package to cluster Image Store
Write-Host "Uploading application package to the cluster image store..." -NoNewline
$result = Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $packagePath -ImageStoreConnectionString (Get-ImageStoreConnectionStringFromClusterManifest(Get-ServiceFabricClusterManifest))
Write-Host $result

# Register the application type from the uploaded package
Write-Host "Registering the application type ProjectServiceType..." -NoNewline
$result = Register-ServiceFabricApplicationType ProjectServiceType
Write-Host "done!"

# Create the application
Write-Host "Creating an instance of ProjectServiceType..." -NoNewline
$result = New-ServiceFabricApplication -ApplicationName fabric:/ProjectService -ApplicationTypeName "ProjectServiceType" -ApplicationTypeVersion "1.0.0"
Write-Host "done!"
