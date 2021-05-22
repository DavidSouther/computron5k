param($installPath, $toolsPath, $package, $project)


if (Get-Module | ?{ $_.Name -eq 'YaccLexTools' })
{
	Remove-YaccLexToolsSettings
    Remove-Module YaccLexTools
}
