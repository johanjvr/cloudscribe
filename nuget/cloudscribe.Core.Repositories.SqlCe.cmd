mkdir nupkgs
mkdir cloudscribe.Core.Repositories.SqlCe\lib
mkdir cloudscribe.Core.Repositories.SqlCe\lib\net45

mkdir cloudscribe.Core.Repositories.SqlCe\content
mkdir cloudscribe.Core.Repositories.SqlCe\content\Config
mkdir cloudscribe.Core.Repositories.SqlCe\content\Config\applications
mkdir cloudscribe.Core.Repositories.SqlCe\content\Config\applications\cloudscribe-core
mkdir cloudscribe.Core.Repositories.SqlCe\content\Config\applications\cloudscribe-core\SchemaInstallScripts
mkdir cloudscribe.Core.Repositories.SqlCe\content\Config\applications\cloudscribe-core\SchemaInstallScripts\sqlce

mkdir cloudscribe.Core.Repositories.SqlCe\content\Config\applications\cloudscribe-core\SchemaUpgradeScripts
mkdir cloudscribe.Core.Repositories.SqlCe\content\Config\applications\cloudscribe-core\SchemaUpgradeScripts\sqlce

xcopy  ..\src\cloudscribe.WebHost\Config\applications\cloudscribe-core\SchemaInstallScripts\sqlce\* ..\nuget\cloudscribe.Core.Repositories.SqlCe\content\Config\applications\cloudscribe-core\SchemaInstallScripts\sqlce\ /s /y /d

xcopy ..\src\cloudscribe.WebHost\Config\applications\cloudscribe-core\SchemaUpgradeScripts\sqlce\* cloudscribe.Core.Repositories.SqlCe\content\Config\applications\cloudscribe-core\SchemaUpgradeScripts\sqlce\ /s /y /d

xcopy ..\src\cloudscribe.Core.Repositories.SqlCe\bin\Release\cloudscribe.Core.Repositories.SqlCe.dll cloudscribe.Core.Repositories.SqlCe\lib\net45 /y

xcopy ..\src\cloudscribe.Core.Repositories.SqlCe\bin\Release\cloudscribe.Core.Repositories.SqlCe.pdb cloudscribe.Core.Repositories.SqlCe\lib\net45 /y

NuGet.exe pack cloudscribe.Core.Repositories.SqlCe\cloudscribe.Core.Repositories.SqlCe.nuspec -OutputDirectory "nupkgs"