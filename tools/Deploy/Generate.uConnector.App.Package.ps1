[CmdletBinding()]
Param(
  [Parameter(Mandatory=$False)]
  [string]$TargetDirectory = "C:\tmp\SampleApp",
    
  [Parameter(Mandatory=$False)]
  [string]$SourceDirectory, 
  
  [Parameter(Mandatory=$False)]
  [string]$RebuildSolution = "True", 
	
	[Parameter(Mandatory=$False)]
	[string]$NuspecFile="App.Manifest.nuspec"
)

function GetScriptDirectory { 
    Split-Path -parent $PSCommandPath 
}

function GetProjectFolder {
	$scriptPath = GetScriptDirectory;
	
	return "$scriptPath\..\..\src\uCommerce.uConnector"
}

function MoveNuspecFile {
  $scriptPath = GetScriptDirectory;
  $nugetPath = $scriptPath + "\..\NuGet\" + $NuspecFile;

  Copy-Item -Path $nugetPath -Destination $TargetDirectory
}

function GetVersion {
  $scriptPath = GetScriptDirectory;
  $nuspecFile = "$scriptPath\..\NuGet\" + $NuspecFile;

  [xml]$fileContents = Get-Content -Path $nuspecFile
  return $fileContents.package.metadata.version;
}

function GetSolutionFile { 
   $scriptPath = GetScriptDirectory;
   $srcFolder = "$scriptPath\..\..\src";
   return Get-ChildItem -Path $srcFolder -Filter *.sln -Recurse;
}

function UpdateAssemblyInfos {
  $version = GetVersion;
  $newVersion = 'AssemblyVersion("' + $version + '")';
  $newFileVersion = 'AssemblyFileVersion("' + $version + '")';
  
  foreach ($file in Get-ChildItem $SourceDirectory AssemblyInfo.cs -Recurse)  
  {      
    $TmpFile = $file.FullName + ".tmp"

    get-content $file.FullName | 
      %{$_ -replace 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $newVersion } |
      %{$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $newFileVersion } |
    set-content $TmpFile -force
    move-item $TmpFile $file.FullName -force
  }
}

function Run-It () {
  try {  
    $scriptPath = GetScriptDirectory;
    Import-Module "$scriptPath\..\psake\4.3.0.0\psake.psm1"
    
    #Step 01: Maintain dependencies    
    $maintainDependenciesProperties = @{
      "projects" = @(
        $SourceDirectory
      );
      "nuspecFile" = "$scriptPath\..\NuGet\$NuspecFile"
    };
        
    Invoke-PSake "$ScriptPath\Maintain.Nuget.Dependencies.ps1" "Run-It" -parameters $maintainDependenciesProperties
                       
    #Step 02 rebuild solution
    if($RebuildSolution.Equals("True")){
      $SolutionFile = GetSolutionFile;
      $rebuildProperties = @{
        "Solution_file" = $SolutionFile;
        "srcDir" = $SourceDirectory;
        "Configuration" = "Release"
      };
        
      Invoke-PSake "$ScriptPath\Rebuild.App.Solution.ps1" "Rebuild" -parameters $rebuildProperties
    }
    
    #Step 03 update assembly version on projects in sln. 
    UpdateAssemblyInfos;
    
    #Step 04 Extract files
    if ($SourceDirectory.Equals(""))
		{
			$SourceDirectory = GetProjectFolder;
		}
		
    $extractProperties = @{
      "TargetDirectory" = $TargetDirectory;
      "projects" = @(
        $SourceDirectory
      );
    };

    Invoke-PSake "$ScriptPath\Extract.Files.For.App.ps1" "Run-It" -parameters $extractProperties
   
    #Step 05 pack it up
    MoveNuspecFile;
    $nuget = $scriptPath + "\..\NuGet";
    $nuspecFilePath = $TargetDirectory + "\" + $NuspecFile;

    & "$nuget\nuget.exe" pack $nuspecFilePath -OutputDirectory $TargetDirectory;

    #Step 06 remove/delete files. 
    Remove-Item $TargetDirectory\* -exclude *.nupkg -recurse
  } catch {  
    Write-Error $_.Exception.Message -ErrorAction Stop  
  }
}

Run-It