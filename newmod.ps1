Param(
   [Parameter(Mandatory)]
   $modname
)

Write-Output "Preparing to create new mod '$modname'"

# install latest version of mod template
Set-Location "./templates"
dotnet new install ProfuselyViolentProgression.ModTemplate --force
Set-Location "../"

# create mod
Set-Location "./BepInExPlugins"
dotnet new pvpmod -n "$modname" --description="Description of your mod"
# dotnet new pvpmod -n "$modname" --description="Description of your mod" --use-vcf
Write-Output "Created new mod at ./BepInExPlugins/$modname/"
Set-Location "../"

dotnet sln add "./BepInExPlugins/$modname/$modname.csproj"