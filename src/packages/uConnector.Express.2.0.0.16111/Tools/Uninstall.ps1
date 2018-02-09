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
                $newStartAction = 0
                $newStartProgram = ""
                $newStartArguments = ""

                $projectConfiguration.Properties.Item("StartAction").Value = $newStartAction
                $projectConfiguration.Properties.Item("StartProgram").Value = $newStartProgram
                $projectConfiguration.Properties.Item("StartArguments").Value = $newStartArguments
            }
        }
    }

    #$project.Save
}

Function OutputDirectories {
    $fullPath = $project.properties.item("FullPath").Value
    return @("$fullPath\bin\Debug\", "$fullPath\bin\Release\")
}

function FilesToDelete {
    return @("uConnector.Express.exe", "uConnector.Express.exe.config", "uConnector.config");
}

Function DeleteFilesInOutputDirectories {

    foreach ($directory in OutputDirectories) {
        foreach ($file in FilesToDelete) {
            $FilePath = $directory + $file
            Remove-Item $FilePath
        }
    }
}

function ResetPostBuildEvent {
    $project.Properties.Item("PostBuildEvent").Value = ""
}

function uninstall {
    ModifyConfigurations

    DeleteFilesInOutputDirectories

    ResetPostBuildEvent
}

uninstall