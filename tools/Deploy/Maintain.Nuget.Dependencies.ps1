properties {
  $projects = $projects;
  $nuspecFile = $nuspecFile;
}

function LoadXmlFile ($filePath) {
  $packagesConfig = New-Object XML
  $packagesConfig.Load($filePath)
  return $packagesConfig;
}

function GetNugetDependencies {
	$dependencies = @();
	
	foreach ($project in $projects) {
    $filePath = $project + '\packages.config';
    $packagesConfig = LoadXmlFile $filePath
    
    foreach($package in $packagesConfig.packages.package)
    {
      if($dependencies -notcontains $package){ 
        $dependencies += $package
      }
    }
  }		
	return $dependencies
}

function RemoveDuplicates ($dependencies){
  $refactoredDependencies = @();
  
  foreach ($dependency in $nugetDependencies) {     
     $addDependency = $True;
     foreach ($refactoredDependency in $refactoredDependencies) {
       if($refactoredDependency.id -eq $dependency.id){ #Could check version and add difference versions. 
         $addDependency = $False;
       }   
     }
     
     if($addDependency){
      $refactoredDependencies += $dependency  
     }
  }
  return $refactoredDependencies
}

function AvoidAddingAlreadyRegisteredDependencies ($dependencyCandidates){
  $nuspec = LoadXmlFile $nuspecFile
  $dependencies = @();
  
  $addDependency = $True;
  foreach ($dependencyCandidate in $dependencyCandidates) {
    foreach ($dependency in $nuspec.package.metadata.dependencies.dependency) {
      if($dependency.id -eq $dependencyCandidate.id){
        $addDependency = $False;
      }
    }
    if($addDependency){
      $dependencies += $dependencyCandidate  
    }
  }
  return $dependencies
}

function AddDependenciesToNuspec ($dependencies){
  $packagesConfig = LoadXmlFile $nuspecFile
  
  $dependenciesEle;
  if($packagesConfig.package.metadata.dependencies){
    $dependenciesEle = $packagesConfig.package.metadata.dependencies
  }
  else {
    $dependenciesEle = $packagesConfig.CreateElement('dependencies')  
  }
  
  foreach ($package in $nugetDependencies) {          
     $dependencyEle = $packagesConfig.CreateElement('dependency')
     $dependencyEle.SetAttribute('id', $package.id)
     $dependencyEle.SetAttribute('version', $package.version)
     
     $dependenciesEle.AppendChild($dependencyEle) 
  }
  $packagesConfig.package.metadata.AppendChild($dependenciesEle) 
  $packagesConfig.Save($nuspecFile)
}

task Run-It {        
	write-host 'Maintain Nuget dependencies';
  
  $nugetDependencies = GetNugetDependencies;
  $nugetDependencies = RemoveDuplicates $nugetDependencies;
  $nugetDependencies = AvoidAddingAlreadyRegisteredDependencies $nugetDependencies;
  AddDependenciesToNuspec $nugetDependencies;
}