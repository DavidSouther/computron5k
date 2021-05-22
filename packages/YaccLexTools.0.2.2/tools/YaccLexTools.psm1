# Copyright (c) Abriom srl.  All rights reserved.

$knownExceptions = @(
    'YaccLexTools.PowerShell.YaccLexToolsException',
    'YaccLexTools.PowerShell.ProjectTypeNotSupportedException'
)

<#
.SYNOPSIS
    Show tools version.

.DESCRIPTION
    Shows Yacc/Lex Tools package version.

#>
function Get-YltVersion
{
	[CmdletBinding()] 
	param() 

    $runner = New-YltRunner $ProjectName $StartUpProjectName $null $ConfigurationTypeName

    try
    {
        Invoke-RunnerCommand $runner YaccLexTools.PowerShell.GetYaccLexToolsVersionCommand @( )
        $error = Get-RunnerError $runner       		

        if ($error)
        {
            if ($knownExceptions -notcontains $error.TypeName)
            {
                Write-Host $error.StackTrace
            }
            else
            {
                Write-Verbose $error.StackTrace
            }

            throw $error.Message
        }		
        $(Get-VSComponentModel).GetService([NuGetConsole.IPowerConsoleWindow]).Show()	        
    }
    finally
    {			
        Remove-Runner $runner		
    }
}

<#
.SYNOPSIS
    Adds a new parser.

.DESCRIPTION
    Adds a new parser to the project by creating all files necessary.

#>
function Add-Parser
{
	[CmdletBinding()] 
	param(
		[parameter(Position = 0,
            Mandatory = $true)]
		[string] $ParserName,
		[parameter(Position = 1)]
		[string] $Namespace
	) 

	$ParserKey = Get-ParserKey $ParserName $Namespace

	$project = Get-Project
	$buildProject = Get-MSBuildProject
	$xml = $buildProject.Xml

	$itemGroup = $xml.ItemGroups | ?{ $_.Label -eq $ParserKey + 'Files' }
	if (!$itemGroup)
	{
		$runner = New-YltRunner $ProjectName $StartUpProjectName $null $ConfigurationTypeName

		try
		{
			Invoke-RunnerCommand $runner YaccLexTools.PowerShell.AddParserCommand @( $ParserName, $Namespace )
			$error = Get-RunnerError $runner

			if ($error)
			{
				if ($knownExceptions -notcontains $error.TypeName)
				{
					Write-Host $error.StackTrace
				}
				else
				{
					Write-Verbose $error.StackTrace
				}

				throw $error.Message
			}		
			$(Get-VSComponentModel).GetService([NuGetConsole.IPowerConsoleWindow]).Show()
		}
		finally
		{			
			Remove-Runner $runner		
		}
	}

	Add-ProjectSettings $ParserName $Namespace
}


<#
.SYNOPSIS
    Remove parser build settings.

.DESCRIPTION
    Remove parser build settings.

#>
function Remove-Parser
{
	[CmdletBinding()] 
	param(
		[parameter(Position = 0,
            Mandatory = $true)]
		[string] $ParserName,
		[parameter(Position = 1)]
		[string] $Namespace
	) 

	$ParserKey = Get-ParserKey $ParserName $Namespace	

	$project = Get-Project
	$buildProject = Get-MSBuildProject
	$xml = $buildProject.Xml


	$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'YltParsers' }
	if ($pg)
	{
		$parsers = $pg.Properties | ?{ $_.Name -eq 'Names' }
		$v = $parsers.Value.Split(';')
		if ($v.Contains($ParserKey))
		{
			$s = ''
			foreach ($item in $v) { if (!($item -eq $ParserKey)) { $s += $item + ';' } }
			if ($s.Length -gt 0) 
			{ 
				$s = $s.Substring(0, $s.Length - 1)
				$parsers.Value = $s
			}
			else
			{
				$pg.Parent.RemoveChild($pg)
				$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'YltProperties' }
				if ($pg)
				{
					$pg.Parent.RemoveChild($pg)
				}
			}
		}
	}


	$targetBuildGen = $xml.Targets | ?{ $_.Name -eq 'YltBuildGen' }
	if ($targetBuildGen)
	{
		$dependency = 'Generate' + $ParserKey
		Remove-TargetDependency $targetBuildGen $dependency
		if ($targetBuildGen.DependsOnTargets -eq '')
		{
			$targetBuildGen.Parent.RemoveChild($targetBuildGen)
			
			$targetBeforeBuild = $xml.Targets | ?{ $_.Name -eq 'BeforeBuild' }
			if ($targetBeforeBuild)
			{
				Remove-TargetDependency $targetBeforeBuild 'YltBuildGen'
			}
		}
	}

	
	$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'Generate' + $ParserKey + 'Properties' }
	if ($pg)
	{
		$pg.Parent.RemoveChild($pg)
	}

	$target = $xml.Targets | ?{ $_.Name -eq 'Generate' + $ParserKey }
	if ($target)
	{
		$target.Parent.RemoveChild($target)
	}

	$project.Save()
}

<#
.SYNOPSIS
    Remove parser build settings.

.DESCRIPTION
    Remove parser build settings.

#>
function Remove-AllParsers
{
	[CmdletBinding()] 
	param() 


	$project = Get-Project
	$buildProject = Get-MSBuildProject
	$xml = $buildProject.Xml

	$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'YltParsers' }
	if ($pg)
	{
		$parsers = $pg.Properties | ?{ $_.Name -eq 'Names' }
		$v = $parsers.Value.Split(';')
		
		foreach ($item in $v)
		{
			Remove-Parser($item)
		}
	}
}


<#
.SYNOPSIS
    Adds a new parser example.

.DESCRIPTION
    Adds a calculator parser example.

#>
function Add-CalculatorExample
{
	[CmdletBinding()] 
	param() 

    $runner = New-YltRunner $ProjectName $StartUpProjectName $null $ConfigurationTypeName

    try
    {
        Invoke-RunnerCommand $runner YaccLexTools.PowerShell.AddCalculatorExampleCommand @( )
        $error = Get-RunnerError $runner

        if ($error)
        {
            if ($knownExceptions -notcontains $error.TypeName)
            {
                Write-Host $error.StackTrace
            }
            else
            {
                Write-Verbose $error.StackTrace
            }

            throw $error.Message
        }		
        $(Get-VSComponentModel).GetService([NuGetConsole.IPowerConsoleWindow]).Show()

		Add-ProjectSettings 'Calculator' ''
    }
    finally
    {			
        Remove-Runner $runner		
    }	
}



function New-YltRunner($ProjectName, $StartUpProjectName, $ContextProjectName, $ConfigurationTypeName)
{
    $project = Get-SingleProject $ProjectName

    $installPath = Get-YaccLexToolsInstallPath $project
    $toolsPath = Join-Path $installPath tools

    $info = New-AppDomainSetup $project $installPath

    $domain = [AppDomain]::CreateDomain('YaccLexTools', $null, $info)
    $domain.SetData('project', $project)
    $domain.SetData('contextProject', $contextProject)
    $domain.SetData('startUpProject', $startUpProject)
    $domain.SetData('configurationTypeName', $ConfigurationTypeName)
    
    $dispatcher = New-DomainDispatcher $toolsPath
    $domain.SetData('yltDispatcher', $dispatcher)

    return @{
        Domain = $domain;
        ToolsPath = $toolsPath
    }
}


function New-AppDomainSetup($Project, $InstallPath)
{
    $info = New-Object System.AppDomainSetup -Property @{
            ShadowCopyFiles = 'true';
            ApplicationBase = $InstallPath;
            PrivateBinPath = 'tools';
            ConfigurationFile = ([AppDomain]::CurrentDomain.SetupInformation.ConfigurationFile)
        }
    
    $targetFrameworkVersion = (New-Object System.Runtime.Versioning.FrameworkName ($Project.Properties.Item('TargetFrameworkMoniker').Value)).Version

    if ($targetFrameworkVersion -lt (New-Object Version @( 4, 5 )))
    {
        $info.PrivateBinPath += ';lib\net40'
    }
    else
    {
        $info.PrivateBinPath += ';lib\net45'
    }

    return $info
}


function New-DomainDispatcher($ToolsPath)
{
    $utilityAssembly = [System.Reflection.Assembly]::LoadFrom((Join-Path $ToolsPath YaccLexTools.PowerShell.Utility.dll))
    $dispatcher = $utilityAssembly.CreateInstance(
        'YaccLexTools.PowerShell.Utilities.DomainDispatcher',
        $false,
        [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Public,
        $null,
        $PSCmdlet,
        $null,
        $null)

    return $dispatcher
}


function Get-ParserKey($ParserName, $Namespace)
{
	if (!$Namespace)
	{
		return $ParserName
	}
	
	$key = $ParserName
	if ($Namespace.Length -gt 0)
	{
		$key += '-' + $Namespace.Replace('.', '-')
	}
	return $key
}


function Get-YaccLexToolsInstallPath($project)
{
    $package = Get-Package -ProjectName $project.FullName | ?{ $_.Id -eq 'YaccLexTools' }

    if (!$package)
    {
        $projectName = $project.Name

        throw "The YaccLexTools package is not installed on project '$projectName'."
    }
    
    return Get-PackageInstallPath $package
}


function Get-PackageInstallPath($package)
{
    $componentModel = Get-VsComponentModel
    $packageInstallerServices = $componentModel.GetService([NuGet.VisualStudio.IVsPackageInstallerServices])

    $vsPackage = $packageInstallerServices.GetInstalledPackages() | ?{ $_.Id -eq $package.Id -and $_.Version -eq $package.Version }
    
    return $vsPackage.InstallPath
}


function Invoke-RunnerCommand($runner, $command, $parameters, $anonymousArguments)
{
    $domain = $runner.Domain

    if ($anonymousArguments)
    {
        $anonymousArguments.GetEnumerator() | %{
            $domain.SetData($_.Name, $_.Value)
        }
    }

    $domain.CreateInstanceFrom(
        (Join-Path $runner.ToolsPath YaccLexTools.PowerShell.dll),
        $command,
        $false,
        0,
        $null,
        $parameters,
        $null,
        $null) | Out-Null
}


function Get-RunnerError($runner)
{
    $domain = $runner.Domain

    if (!$domain.GetData('wasError'))
    {
        return $null
    }

    return @{
            Message = $domain.GetData('error.Message');
            TypeName = $domain.GetData('error.TypeName');
            StackTrace = $domain.GetData('error.StackTrace')
    }
}


function Remove-Runner($runner)
{
    [AppDomain]::Unload($runner.Domain)
}


function Get-SingleProject($name)
{
	if ($name)
	{
		$project = Get-Project $name

		if ($project -is [array])
		{
			throw "More than one project '$name' was found. Specify the full name of the one to use."
		}
	}
	else
	{
		$project = Get-Project
	}
    return $project
}



function Get-MSBuildProject {
    param(
        [parameter(ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    Process {
        (Resolve-ProjectName $ProjectName) | % {
            $path = $_.FullName
            @([Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($path))[0]
        }
    }
}

function Resolve-ProjectName {
    param(
        [parameter(ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    
    if($ProjectName) {
        $projects = Get-Project $ProjectName
    }
    else {
        # All projects by default
        $projects = Get-Project
    }
    
    $projects
}


function Add-ProjectSettings($ParserName, $Namespace)
{
	$project = Get-Project
	$buildProject = Get-MSBuildProject
	$xml = $buildProject.Xml

	$ParserKey = Get-ParserKey $ParserName $Namespace

	if (!$Namespace) { $Namespace = '' }

	$rootNamespace = ($project.Properties | ?{ $_.Name -eq 'RootNamespace' }).Value
	$path = $Namespace.Replace('.', '\')
	if ($Namespace.StartsWith($rootNamespace))
	{
		$path = $path.Substring($rootNamespace.Length);
		if ($path.StartsWith('\')) { $path = $path.Substring(1) }
	}
	if ($path.Length -gt 0) { $path += '\' }


	$parsers = $xml.PropertyGroups | ?{ $_.Label -eq 'YltParsers' }
	if (!$parsers)
	{
		$parsers = $xml.AddPropertyGroup()
		$parsers.Label = 'YltParsers'
		$void = $parsers.AddProperty('Names', $ParserKey)
	}
	else
	{
		$names = $parsers.Properties | ?{ $_.Name -eq 'Names' }
		if (!$names.Value.Split(';').Contains($ParserKey))
		{
			$names.Value += ';' + $ParserKey
		}
	}

	$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'YltProperties' }
	if (!$pg)
	{
		$pg = $xml.AddPropertyGroup()
		$pg.Label = 'YltProperties'

		$void = $pg.AddProperty('YltTools', '$(SolutionDir)packages\YaccLexTools.0.2.2\tools\')
		$void = $pg.AddProperty('GplexTool', '"$(YltTools)gplex.exe"')
		$void = $pg.AddProperty('GppgTool', '"$(YltTools)gppg.exe"')
	}


	$targetBeforeBuild = $xml.Targets | ?{ $_.Name -eq 'BeforeBuild' }
	$s = 'YltBuildGen'
	if (!$targetBeforeBuild)
	{
		$targetBeforeBuild = $xml.AddTarget('BeforeBuild')
		$targetBeforeBuild.DependsOnTargets = $s
	}
	else 
	{
		if ($targetBeforeBuild.DependsOnTargets -eq '')
		{
			$targetBeforeBuild.DependsOnTargets = $s
		}
		else
		{
			if (!$targetBeforeBuild.DependsOnTargets.Split(';').Contains($s))
			{
				$targetBeforeBuild.DependsOnTargets += ';' + $s
			}
		}
	}


	$targetBuildGen = $xml.Targets | ?{ $_.Name -eq 'YltBuildGen' }
	$s = 'Generate' + $ParserKey
	if (!$targetBuildGen)
	{
		$targetBuildGen = $xml.AddTarget('YltBuildGen')
		$targetBuildGen.DependsOnTargets = $s
	}
	else 
	{
		if ($targetBuildGen.DependsOnTargets -eq '')
		{
			$targetBuildGen.DependsOnTargets = $s
		}
		else
		{
			if (!$targetBuildGen.DependsOnTargets.Split(';').Contains($s))
			{
				$targetBuildGen.DependsOnTargets += ';' + $s
			}
		}
	}


	$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'Generate' + $ParserKey + 'Properties' }
	if (!$pg)
	{
		$pg = $xml.AddPropertyGroup()
		$pg.Label = 'Generate' + $ParserKey + 'Properties'

		$void = $pg.AddProperty($ParserKey + 'Parser', '$(ProjectDir)' + $path + $ParserName)
	}


	$target = $xml.Targets | ?{ $_.Name -eq 'Generate' + $ParserKey }
	if (!$target)
	{
		$target = $xml.AddTarget('Generate' + $ParserKey)
	
		$parserPrefix = '$(' + $ParserKey + 'Parser)'
	
		$target.Inputs = $parserPrefix + '.Language.analyzer.lex;' + $parserPrefix + '.Language.grammar.y'
		$target.Outputs = $parserPrefix + '.Scanner.Generated.cs;' + $parserPrefix + '.Parser.Generated.cs'

		$task = $target.AddTask('Message')
		$task.SetParameter('Text', 'Generating scanner for ' + $parserPrefix + ' ...')
	
		$task = $target.AddTask('Exec')
		$task.SetParameter('Command', '$(GplexTool) "/out:' + $parserPrefix + '.Scanner.Generated.cs" "' + $parserPrefix + '.Language.analyzer.lex"')
		$task.SetParameter('WorkingDirectory', '$(ProjectDir)')
		$task.SetParameter('Outputs', '$(GenDir)Scanner.cs')
		$void = $task.AddOutputItem('Outputs', $ParserName + 'Scanner')

		$task = $target.AddTask('Message')
		$task.SetParameter('Text', 'Generating parser for ' + $parserPrefix + ' ...')
		
		$task = $target.AddTask('Exec')
		$task.SetParameter('Command', '$(GppgTool) /no-lines /gplex "' + $parserPrefix + '.Language.grammar.y" > "' + $parserPrefix + '.Parser.Generated.cs"')
		$task.SetParameter('WorkingDirectory', '$(ProjectDir)')
		$task.SetParameter('Outputs', $parserPrefix + '.Parser.Generated.cs')
		$void = $task.AddOutputItem('Outputs', $ParserName)
	}

	
	$itemGroup = $xml.ItemGroups | ?{ $_.Label -eq $ParserKey + 'Files' }
	if (!$itemGroup)
	{
		$itemGroup = $xml.AddItemGroup()
		$itemGroup.Label = $ParserKey + 'Files'

		$item = $itemGroup.AddItem('None', $path + $ParserName + '.parser')
	
		$item = $itemGroup.AddItem('Compile', $path + $ParserName + '.Parser.cs')
		$void = $item.AddMetadata('DependentUpon', $ParserName + '.parser')
	
		$item = $itemGroup.AddItem('Compile', $path + $ParserName + '.Scanner.cs')
		$void = $item.AddMetadata('DependentUpon', $ParserName + '.parser')
	
		$item = $itemGroup.AddItem('None', $path + $ParserName + '.Language.grammar.y')
		$void = $item.AddMetadata('DependentUpon', $ParserName + '.parser')
	
		$item = $itemGroup.AddItem('Compile', $path + $ParserName + '.Parser.Generated.cs')
		$void = $item.AddMetadata('AutoGen', 'True')
		$void = $item.AddMetadata('DesignTime', 'True')
		$void = $item.AddMetadata('DependentUpon', $ParserName + '.Language.grammar.y')
	
		$item = $itemGroup.AddItem('None', $path + $ParserName + '.Language.analyzer.lex')
		$void = $item.AddMetadata('DependentUpon', $ParserName + '.parser')
	
		$item = $itemGroup.AddItem('Compile', $path + $ParserName + '.Scanner.Generated.cs')
		$void = $item.AddMetadata('AutoGen', 'True')
		$void = $item.AddMetadata('DesignTime', 'True')
		$void = $item.AddMetadata('DependentUpon', $ParserName + '.Language.analyzer.lex')
	}

	$buildProject.Save()
}


function Remove-TargetDependency($target, $dependency)
{
	$v = $target.DependsOnTargets.Split(';')
	if ($v.Contains($dependency))
	{
		$s = ''
		foreach ($item in $v) { if (!($item -eq $dependency)) { $s += $item + ';' } }
		if ($s.Length -gt 0) { $s = $s.Substring(0, $s.Length - 1) }
		$target.DependsOnTargets = $s
	}
}


function Add-YaccLexToolsSettings
{
	$project = Get-Project
	$buildProject = Get-MSBuildProject
	$xml = $buildProject.Xml

	$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'YltParsers' }
	if ($pg)
	{
		$parsers = $pg.Properties | ?{ $_.Name -eq 'Names' }
		$v = $parsers.Value.Split(';')


		$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'YltProperties' }
		if (!$pg)
		{
			$pg = $xml.AddPropertyGroup()
			$pg.Label = 'YltProperties'

			$void = $pg.AddProperty('YltTools', '$(SolutionDir)packages\YaccLexTools.0.2.2\tools\')
			$void = $pg.AddProperty('GplexTool', '"$(YltTools)gplex.exe"')
			$void = $pg.AddProperty('GppgTool', '"$(YltTools)gppg.exe"')
		}


		$targetBeforeBuild = $xml.Targets | ?{ $_.Name -eq 'BeforeBuild' }
		$s = 'YltBuildGen'
		if (!$targetBeforeBuild)
		{
			$targetBeforeBuild = $xml.AddTarget('BeforeBuild')
			$targetBeforeBuild.DependsOnTargets = $s
		}
		else 
		{
			if ($targetBeforeBuild.DependsOnTargets -eq '')
			{
				$targetBeforeBuild.DependsOnTargets = $s
			}
			else
			{
				if (!$targetBeforeBuild.DependsOnTargets.Split(';').Contains($s))
				{
					$targetBeforeBuild.DependsOnTargets += ';' + $s
				}
			}
		}

		
		foreach ($ParserKey in $v)
		{
			$targetBuildGen = $xml.Targets | ?{ $_.Name -eq 'YltBuildGen' }
			$s = 'Generate' + $ParserKey
			if (!$targetBuildGen)
			{
				$targetBuildGen = $xml.AddTarget('YltBuildGen')
				$targetBuildGen.DependsOnTargets = $s
			}
			else 
			{
				if ($targetBuildGen.DependsOnTargets -eq '')
				{
					$targetBuildGen.DependsOnTargets = $s
				}
				else
				{
					if (!$targetBuildGen.DependsOnTargets.Split(';').Contains($s))
					{
						$targetBuildGen.DependsOnTargets += ';' + $s
					}
				}
			}


			$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'Generate' + $ParserKey + 'Properties' }
			if (!$pg)
			{
				$pg = $xml.AddPropertyGroup()
				$pg.Label = 'Generate' + $ParserKey + 'Properties'

				$ParserName = $ParserKey
				$path = ''
				
				$idx = $ParserKey.IndexOf('-')
				if ($idx -gt 0)
				{
					$rootNamespaceProperty = $xml.Properties | ?{ $_.Name -eq 'RootNamespace' }
					$rootNamespace = $rootNamespaceProperty.Value

					$ParserName = $ParserKey.Substring(0, $idx)
					$Namespace = $ParserKey.Substring($idx + 1).Replace('-', '.')
					
					$path = $Namespace.Replace('.', '\')

					if ($rootNamespace)
					{
						if ($Namespace.StartsWith($rootNamespace))
						{
							$path = $path.Substring($rootNamespace.Length)
							if ($path.StartsWith('\')) { $path = $path.Substring(1) }
						}
					}
					$path += '\'
				}

				$void = $pg.AddProperty($ParserKey + 'Parser', '$(ProjectDir)' + $path + $ParserName)
			}


			$target = $xml.Targets | ?{ $_.Name -eq 'Generate' + $ParserKey }
			if (!$target)
			{
				$target = $xml.AddTarget('Generate' + $ParserKey)
	
				$parserPrefix = '$(' + $ParserKey + 'Parser)'
	
				$target.Inputs = $parserPrefix + '.Language.analyzer.lex;' + $parserPrefix + '.Language.grammar.y'
				$target.Outputs = $parserPrefix + '.Scanner.Generated.cs;' + $parserPrefix + '.Parser.Generated.cs'

				$task = $target.AddTask('Message')
				$task.SetParameter('Text', 'Generating scanner for ' + $parserPrefix + ' ...')
	
				$task = $target.AddTask('Exec')
				$task.SetParameter('Command', '$(GplexTool) "/out:' + $parserPrefix + '.Scanner.Generated.cs" "' + $parserPrefix + '.Language.analyzer.lex"')
				$task.SetParameter('WorkingDirectory', '$(ProjectDir)')
				$task.SetParameter('Outputs', '$(GenDir)Scanner.cs')
				$void = $task.AddOutputItem('Outputs', $ParserName + 'Scanner')

				$task = $target.AddTask('Message')
				$task.SetParameter('Text', 'Generating parser for ' + $parserPrefix + ' ...')
		
				$task = $target.AddTask('Exec')
				$task.SetParameter('Command', '$(GppgTool) /no-lines /gplex "' + $parserPrefix + '.Language.grammar.y" > "' + $parserPrefix + '.Parser.Generated.cs"')
				$task.SetParameter('WorkingDirectory', '$(ProjectDir)')
				$task.SetParameter('Outputs', $parserPrefix + '.Parser.Generated.cs')
				$void = $task.AddOutputItem('Outputs', $ParserName)
			}
		}
	}
}


function Remove-YaccLexToolsSettings
{
	$project = Get-Project
	$buildProject = Get-MSBuildProject
	$xml = $buildProject.Xml


	$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'YltProperties' }
	if ($pg)
	{
		$pg.Parent.RemoveChild($pg)
	}


	$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'YltParsers' }
	if ($pg)
	{
		$parsers = $pg.Properties | ?{ $_.Name -eq 'Names' }
		$v = $parsers.Value.Split(';')
		
		foreach ($ParserKey in $v)
		{
			$targetBuildGen = $xml.Targets | ?{ $_.Name -eq 'YltBuildGen' }
			if ($targetBuildGen)
			{
				$dependency = 'Generate' + $ParserKey
				Remove-TargetDependency $targetBuildGen $dependency
				if ($targetBuildGen.DependsOnTargets -eq '')
				{
					$targetBuildGen.Parent.RemoveChild($targetBuildGen)
			
					$targetBeforeBuild = $xml.Targets | ?{ $_.Name -eq 'BeforeBuild' }
					if ($targetBeforeBuild)
					{
						Remove-TargetDependency $targetBeforeBuild 'YltBuildGen'
					}
				}
			}

	
			$pg = $xml.PropertyGroups | ?{ $_.Label -eq 'Generate' + $ParserKey + 'Properties' }
			if ($pg)
			{
				$pg.Parent.RemoveChild($pg)
			}

			$target = $xml.Targets | ?{ $_.Name -eq 'Generate' + $ParserKey }
			if ($target)
			{
				$target.Parent.RemoveChild($target)
			}

			$project.Save()
		}
	}
}



Export-ModuleMember @( 'Add-Parser', 'Add-CalculatorExample', 'Remove-Parser', 'Remove-AllParsers', 'Add-YaccLexToolsSettings', 'Remove-YaccLexToolsSettings' )

