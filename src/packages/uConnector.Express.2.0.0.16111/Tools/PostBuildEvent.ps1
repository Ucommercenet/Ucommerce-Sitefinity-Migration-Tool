[CmdletBinding()]
Param(
	[Parameter(Mandatory=$True)]
	[string]$TargetDir
)

function FilesToCopy {
    return @("uConnector.Express.exe", "uConnector.Express.exe.config", "uConnector.config");
}

function CopyFiles {
    $SourceDirectory = $PSScriptRoot + "\..\"

    foreach ($file in FilesToCopy) {
        $FilePath = $SourceDirectory + $file
        Copy-Item $FilePath $TargetDir
    }
}

function PostBuildEvent {
    CopyFiles    
}

PostBuildEvent
