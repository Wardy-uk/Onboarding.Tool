Param(
  [string]$pathToSearch = $env:BUILD_SOURCESDIRECTORY,
  [string]$buildNumber = $env:BUILD_BUILDNUMBER,
  [string]$csProjSearchFilter = "*.csproj",
  [string]$csSearchFilter = "AssemblyInfo.*",
  [regex]$csProjPattern = ">(?<Major>\d+)\.(?<Minor>\d+)\.(?<Build>\d+)\.(?<Revision>\d+)<",
  [regex]$csPattern = "(?<year>\d+)\.(?<month>\d+)\.(?<day>\d+)\.(?<buildId>\d+)",
  [regex]$buildNumberPattern = "(?<Major>\d+)\.(?<Minor>\d+)\.(?<Build>\d+)\.(?<Revision>\d+)"
  )
Write-Host "Updating Build Number in source code"
Write-Host $pathToSearch
Write-Host $buildNumber

if ($buildNumber -match $buildNumberPattern -ne $true) {
    Write-Host "Could not extract a version from [$buildNumber] using pattern [$csPattern]"
} else {
	$major=$Matches['Major']
	$minor=$Matches['Minor']
	$build=$Matches['Build']
	$revision=$Matches['Revision']

    $extractedBuildNumber = ">"+ $Matches[0] + "<"
	Write-Host "Processing csproj files using version " $extractedBuildNumber
    gci -Path $pathToSearch -Filter $csProjSearchFilter -Recurse | %{
        Write-Host "  -> Changing $($_.FullName)"

		# remove the read-only bit on the file
		sp $_.FullName IsReadOnly $false

		# run the regex replace
        (gc $_.FullName) | % { $_ -replace $csProjPattern,  $extractedBuildNumber } | sc $_.FullName
    }

	$extractedBuildNumber = $Matches[0]
	Write-Host "Processing .Net assemblyInfo files using version " $extractedBuildNumber

    gci -Path $pathToSearch -Filter $csSearchFilter -Recurse | %{
       Write-Host "  -> Changing $($_.FullName)" 
	
	# remove the read-only bit on the file
	sp $_.FullName IsReadOnly $false

	# run the regex replace
       (gc $_.FullName) | % { $_ -replace $csPattern, $extractedBuildNumber } | sc $_.FullName
   }

    Write-Host "Done!"
}