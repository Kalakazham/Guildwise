[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$baselinePath = Join-Path $PSScriptRoot "code-quality-baseline.json"

$rules = @{
    RazorPage = @{
        Label = "Razor page"
        Target = 350
        Gate = 500
    }
    RazorSharedComponent = @{
        Label = "Razor shared component"
        Target = 150
        Gate = 250
    }
    ProductionCs = @{
        Label = "Production C#"
        Target = 200
        Gate = 300
    }
    TestCs = @{
        Label = "Test C#"
        Target = 400
        Gate = 800
    }
}

function Get-RelativeRepositoryPath {
    param([string] $Path)

    return [System.IO.Path]::GetRelativePath($repoRoot.Path, $Path).Replace("\", "/")
}

function Get-LineCount {
    param([string] $Path)

    $count = 0
    $reader = [System.IO.File]::OpenText($Path)
    try {
        while ($null -ne $reader.ReadLine()) {
            $count++
        }
    }
    finally {
        $reader.Dispose()
    }

    return $count
}

function Test-IgnoredFile {
    param(
        [string] $RelativePath,
        [string] $FileName
    )

    if ($RelativePath -match "(^|/)(\.git|bin|obj)(/|$)") {
        return $true
    }

    if ($RelativePath -match "^src/Guildwise\.Infrastructure/Persistence/Migrations/") {
        return $true
    }

    if ($FileName -eq "GuildwiseDbContextModelSnapshot.cs") {
        return $true
    }

    if ($FileName -match "\.(Designer|generated|g|g\.i)\.cs$") {
        return $true
    }

    if ($FileName -match "(AssemblyInfo|GlobalUsings)\.cs$") {
        return $true
    }

    return $false
}

function Get-QualityCategory {
    param([string] $RelativePath)

    if ($RelativePath.EndsWith(".razor", [StringComparison]::OrdinalIgnoreCase)) {
        if ($RelativePath -match "^src/Guildwise\.Web/Components/Pages/") {
            return "RazorPage"
        }

        return "RazorSharedComponent"
    }

    if ($RelativePath.EndsWith(".cs", [StringComparison]::OrdinalIgnoreCase)) {
        if ($RelativePath -match "^src/") {
            return "ProductionCs"
        }

        if ($RelativePath -match "^tests/") {
            return "TestCs"
        }
    }

    return $null
}

$baselineByPath = @{}
if (Test-Path -LiteralPath $baselinePath) {
    $baseline = Get-Content -Raw -LiteralPath $baselinePath | ConvertFrom-Json
    foreach ($entry in $baseline.knownDebt) {
        $baselineByPath[$entry.path] = [int] $entry.maxLines
    }
}

$warnings = New-Object System.Collections.Generic.List[string]
$errors = New-Object System.Collections.Generic.List[string]
$seenBaselinePaths = New-Object System.Collections.Generic.HashSet[string]
$checkedFiles = 0

Get-ChildItem -LiteralPath $repoRoot.Path -Recurse -File -Include *.cs,*.razor |
    ForEach-Object {
        $relativePath = Get-RelativeRepositoryPath $_.FullName
        if (Test-IgnoredFile -RelativePath $relativePath -FileName $_.Name) {
            return
        }

        $category = Get-QualityCategory -RelativePath $relativePath
        if ($null -eq $category) {
            return
        }

        $checkedFiles++
        $rule = $rules[$category]
        $lineCount = Get-LineCount -Path $_.FullName

        if ($baselineByPath.ContainsKey($relativePath)) {
            [void] $seenBaselinePaths.Add($relativePath)
            $maxLines = $baselineByPath[$relativePath]
            if ($lineCount -gt $maxLines) {
                $errors.Add("Known debt grew: $relativePath has $lineCount lines, baseline allows $maxLines.")
            }
            elseif ($lineCount -gt $rule.Target) {
                $warnings.Add("$($rule.Label) target exceeded by known debt: $relativePath has $lineCount lines, target is $($rule.Target), baseline is $maxLines.")
            }

            return
        }

        if ($lineCount -gt $rule.Gate) {
            $errors.Add("$($rule.Label) gate exceeded: $relativePath has $lineCount lines, gate is $($rule.Gate).")
        }
        elseif ($lineCount -gt $rule.Target) {
            $warnings.Add("$($rule.Label) target exceeded: $relativePath has $lineCount lines, target is $($rule.Target).")
        }
    }

foreach ($baselinePathEntry in $baselineByPath.Keys) {
    if (-not $seenBaselinePaths.Contains($baselinePathEntry)) {
        $warnings.Add("Baseline entry was not found in the current tree: $baselinePathEntry.")
    }
}

Write-Host "Code quality check scanned $checkedFiles files."

foreach ($warning in $warnings) {
    Write-Host "WARNING: $warning"
}

if ($errors.Count -gt 0) {
    foreach ($errorMessage in $errors) {
        Write-Host "ERROR: $errorMessage"
    }

    Write-Host "Code quality check failed with $($errors.Count) gate violation(s)."
    exit 1
}

Write-Host "Code quality check passed with $($warnings.Count) warning(s)."
