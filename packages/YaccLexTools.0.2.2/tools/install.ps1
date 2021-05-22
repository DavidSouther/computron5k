param($installPath, $toolsPath, $package, $project)

Write-Host
Write-Host "Type 'get-help YaccLexTools' to see all available YaccLexTools commands."

if (Get-Module | ?{ $_.Name -eq 'YaccLexTools' })
{
	Add-YaccLexToolsSettings
}
