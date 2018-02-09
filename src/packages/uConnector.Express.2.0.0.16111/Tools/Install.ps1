param($installPath, $toolsPath, $package, $project)

function HasStartAction ($item)
{
    foreach ($property in $item.Properties)
    {
       if ($property.Name -eq "StartAction")
       {
           return $true
       }            
    } 

    return $false
}

function ModifyConfigurations
{
    $configurationManager = $project.ConfigurationManager
    $fullpath = $project.properties.item("fullpath").Value

    foreach ($name in $configurationManager.ConfigurationRowNames)
    {
        $projectConfigurations = $configurationManager.ConfigurationRow($name)

        foreach ($projectConfiguration in $projectConfigurations)
        {                

            if (HasStartAction $projectConfiguration)
            {
                $newStartAction = 1
                $newStartProgram = $fullPath + "bin\" + $name + "\uConnector.Express.exe"
                $newStartArguments = "Tasks"

                $projectConfiguration.Properties.Item("StartAction").Value = [String] $newStartAction
                $projectConfiguration.Properties.Item("StartProgram").Value = [String] $newStartProgram
                $projectConfiguration.Properties.Item("StartArguments").Value = [String] $newStartArguments
            }
        }
    }

    #$project.Save
}

function OutputDirectories {
    $fullPath = $project.properties.item("FullPath").Value
    return @("$fullPath\bin\Debug", "$fullPath\bin\Release")
}

function FilesToCopy {
    return @("$installPath\uConnector.Express.exe", "$installPath\uConnector.Express.exe.config", "$installPath\uConnector.config");
}

function CopyExeAndConfigToOutPutFolders {

    foreach ($directory in OutputDirectories) {
        foreach ($file in FilesToCopy) {
            Copy-Item $file $directory
        }
    }
}

function AddPostBuildEvent {
    
    $installPathString = "" + $installPath
    $packageFolderName = $installPathString.Substring(($installPathString.LastIndexOf("\") + 1))

    $PostBuildEventString = 'Powershell.exe -ExecutionPolicy Bypass -file "$(SolutionDir)packages\' + $packageFolderName + '\Tools\PostBuildEvent.ps1" -TargetDir "$(TargetDir)'

    $project.Properties.Item("PostBuildEvent").Value = [String] $PostBuildEventString
}

function SetCopyOptionOnSampleConfig {
    $project.ProjectItems.Item("Tasks").ProjectItems.Item("Example.config.disabled").Properties.Item("CopyToOutputDirectory").Value = 2
}

function install {    
    ModifyConfigurations 
    
    CopyExeAndConfigToOutPutFolders

    AddPostBuildEvent

    SetCopyOptionOnSampleConfig
}

install