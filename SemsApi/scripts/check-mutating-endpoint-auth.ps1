# Verifies mutating controller actions require authorization.
# Usage: powershell -ExecutionPolicy Bypass -File SemsApi/scripts/check-mutating-endpoint-auth.ps1
$ErrorActionPreference = 'Stop'
$controllers = Get-ChildItem (Join-Path $PSScriptRoot '..\Controllers') -Filter *.cs
# Auth-flow endpoints intentionally allow anonymous/credential-specific access.
$allow = @(
  'AuthController.cs:POST:api/auth/refresh',
  'AuthController.cs:POST:api/auth/logout'
)
$failures = @()
foreach ($file in $controllers) {
  $lines = Get-Content $file.FullName
  $routePrefix = ''
  $classAuth = $false
  for ($i=0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match '^\s*(public\s+)?(partial\s+)?class\b') { break }
    if ($lines[$i] -match '^\s*\[Authorize') { $classAuth = $true }
    if ($lines[$i] -match '^\s*\[Route\("([^"]+)"\)\]') { $routePrefix = $Matches[1] }
  }
  for ($i=0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match '^\s*\[Http(Post|Put|Delete|Patch)(?:\("([^"]*)"\))?\]') {
      $method = $Matches[1].ToUpper()
      $suffix = $Matches[2]
      $path = if ($suffix) { "$routePrefix/$suffix" } else { $routePrefix }
      $key = '{0}:{1}:{2}' -f $file.Name, $method, $path
      $hasAuth = $classAuth
      for ($j=[Math]::Max(0,$i-6); $j -lt $i; $j++) { if ($lines[$j] -match '^\s*\[Authorize') { $hasAuth = $true } }
      if (-not $hasAuth -and $allow -notcontains $key) { $failures += ('{0}:{1} {2} ({3})' -f $file.Name, ($i+1), $lines[$i].Trim(), $key) }
    }
  }
}
if ($failures.Count -gt 0) { Write-Output ("Mutating endpoints missing [Authorize]:`n" + ($failures -join "`n")); exit 1 }
Write-Output 'OK: all non-allowlisted mutating endpoints require authorization.'
