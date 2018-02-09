properties {
  $ContentTargetDirectory = $TargetDirectory + "\Content";
  $LibTargetDirectory = $TargetDirectory + "\Lib\net400";
  $projects = $projects;
}

function FileExtensionBlackList {
  return "*.dll*","*.cd","*.cs","*.dll","*.xml","*obj*","*.pdb","*.csproj*","*.cache","*.orig", "app.config", "packages.config";  
}

function GetFilesToCopy($path){
	return Get-ChildItem $path -name -recurse -include *.* -exclude (FileExtensionBlackList);
}

function CopyFiles ($project) {	 
  $filesToCopy += GetFilesToCopy($project);

  foreach($fileToCopy in $filesToCopy)
  {
    $sourceFile = $project + "\" + $fileToCopy;
    $targetFile = $ContentTargetDirectory + "\" + $fileToCopy;
    
    # Create the folder structure and empty destination file,
    New-Item -ItemType File -Path $targetFile -Force
    Copy-Item $sourceFile $targetFile -Force
  }
}

function LoadXmlFile ($filePath) {
  $packagesConfig = New-Object XML
  $packagesConfig.Load($filePath)
  return $packagesConfig;
}

function FindDlls($path) { 
  $projectCsprojFileName = Get-ChildItem -Path $path -Filter "*.csproj";  
  $projectCsprojFilePath = $path + "\" + $projectCsprojFileName   
  $projectCsprojFile = LoadXmlFile($projectCsprojFilePath);
  
  $dlls = @();
  
  #Finding reference
  foreach($hintPath in $projectCsprojFile.Project.ItemGroup.Reference.HintPath)
  {
    if($hintPath -and $hintPath -notlike '*packages*') {
      $dlls += $hintPath
    }    
  }
  
  #Finding project reference
  foreach($projectReferenceName in $projectCsprojFile.Project.ItemGroup.ProjectReference.Name)
  {
    if($hintPath -and $hintPath -notlike '*packages*') {
      $dlls += "bin\debug\" + $projectReferenceName + ".dll"
    }    
  }
  
  #Finding own dll
  $assemblyName = $projectCsprojFile.Project.PropertyGroup.AssemblyName
  $dlls += "bin\debug\" + $assemblyName + ".dll" -replace '\s',''
  
  return $dlls;
}

function CopyDllToLib ($project) {  
	$filesToCopy = FindDlls($project);
	
	foreach($fileToCopy in $filesToCopy)
	{  	
    $sourceFile = $project + "\" + $fileToCopy;
    $fileName = split-path $sourceFile -leaf
    $targetFile = $LibTargetDirectory + "\" + $fileName;	
     
    if (Test-Path $sourceFile){
      New-Item -ItemType File -Path $targetFile -Force
      Copy-Item $sourceFile $targetFile -Force   
    }
	}
}

task Run-It {        
	write-host 'Extracting app to' + $ContentTargetDirectory;
    
  #Creates app directory
  if (!(Test-Path $ContentTargetDirectory)) {
		write-host 'Creating directory: ' + $ContentTargetDirectory;
    New-Item $ContentTargetDirectory -type directory 
  }	
	
	foreach($project in $projects)
	{
    CopyFiles($project);

    CopyDllToLib($project);
  } 
  
  write-host 'Extracted app to' + $ContentTargetDirectory;   
}
