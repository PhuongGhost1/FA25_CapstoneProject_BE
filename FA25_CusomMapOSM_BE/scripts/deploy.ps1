# Deploy script for IMOS to Windows VPS
param(
    [string]$ConfigPath = "deploy-config.json"
)

# Check if config file exists
if (-not (Test-Path $ConfigPath)) {
    Write-Host "Error: Config file not found: $ConfigPath" -ForegroundColor Red
    Write-Host "Please copy deploy-config.json.example to deploy-config.json and update it." -ForegroundColor Yellow
    exit 1
}

# Load config
$config = Get-Content $ConfigPath | ConvertFrom-Json

# Check if publish folder exists
$publishPath = Join-Path $PSScriptRoot "..\publish"
if (-not (Test-Path $publishPath)) {
    Write-Host "Error: Publish folder not found: $publishPath" -ForegroundColor Red
    Write-Host "Please run 'make publish' first." -ForegroundColor Yellow
    exit 1
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "IMOS Deployment Script" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Deploy files
Write-Host "Deploying files to VPS..." -ForegroundColor Yellow
Write-Host "Remote path: $($config.vps.remotePath)" -ForegroundColor Gray

if ($config.vps.useNetworkShare) {
    # Use network share (Windows file sharing)
    $remotePath = $config.vps.remotePath
    $username = $config.vps.username
    $password = $config.vps.password
    
    # Extract server path
    if ($remotePath -match "^\\\\[^\\]+\\(.+)") {
        $serverPath = $matches[1]
        $serverName = $remotePath -replace "\\\\([^\\]+)\\.+", '$1'
        $uncPath = "\\$serverName\$serverPath"
        
        Write-Host "Mapping network drive..." -ForegroundColor Gray
        $driveLetter = "Z:"
        
        # Remove existing mapping if any
        net use $driveLetter /delete /yes 2>$null
        
        # Map network drive
        $result = net use $driveLetter $uncPath /user:$username $password /persistent:no 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            try {
                Write-Host "Backing up web.config..." -ForegroundColor Gray
                
                # Backup web.config if it exists
                $webConfigPath = "$driveLetter\web.config"
                $webConfigBackup = "$env:TEMP\web.config.backup"
                
                if (Test-Path $webConfigPath) {
                    Copy-Item $webConfigPath $webConfigBackup -Force
                    Write-Host "web.config backed up to: $webConfigBackup" -ForegroundColor Green
                } else {
                    Write-Host "web.config not found in remote path." -ForegroundColor Yellow
                    $webConfigBackup = $null
                }
                
                Write-Host "Copying files..." -ForegroundColor Gray
                
                # Create destination directory if not exists
                if (-not (Test-Path $driveLetter)) {
                    Write-Host "Error: Could not access remote path: $uncPath" -ForegroundColor Red
                    exit 1
                }
                
                # Copy files using robocopy
                # Exclude web.config từ publish folder để không ghi đè
                robocopy "$publishPath" "$driveLetter" * /E /PURGE /XF web.config /NFL /NDL /NJH /NJS
                
                if ($LASTEXITCODE -lt 8) {
                    Write-Host "Files deployed successfully!" -ForegroundColor Green
                    
                    # Restore web.config if it was backed up
                    if ($webConfigBackup -and (Test-Path $webConfigBackup)) {
                        Write-Host "Restoring web.config..." -ForegroundColor Gray
                        Copy-Item $webConfigBackup $webConfigPath -Force
                        Write-Host "web.config restored successfully!" -ForegroundColor Green
                    }
                } else {
                    Write-Host "Warning: Some files may not have been copied. Exit code: $LASTEXITCODE" -ForegroundColor Yellow
                }
            } finally {
                # Disconnect network drive
                net use $driveLetter /delete /yes 2>$null
            }
        } else {
            Write-Host "Error: Could not map network drive. Check credentials and network connectivity." -ForegroundColor Red
            Write-Host $result -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "Error: Invalid remote path format. Use format: \\\\server\share\path" -ForegroundColor Red
        exit 1
    }
} elseif ($config.vps.useSCP) {
    # Use SCP (if OpenSSH is installed on VPS)
    Write-Host "Using SCP to deploy..." -ForegroundColor Gray
    $vpsHost = $config.vps.host
    $port = $config.vps.scpPort
    $username = $config.vps.username
    $remotePath = $config.vps.remotePath
    
    Write-Host "Backing up web.config..." -ForegroundColor Gray
    
    # Backup web.config
    $backupCmd = "scp -P $port ${username}@${vpsHost}:$($remotePath -replace '\\', '/')/web.config `"$env:TEMP\web.config.backup`""
    Invoke-Expression $backupCmd 2>$null
    
    Write-Host "Deploying files (excluding web.config)..." -ForegroundColor Gray
    
    # Prepare files for SCP (exclude web.config)
    $tempPublishPath = "$env:TEMP\imos_publish_temp"
    if (Test-Path $tempPublishPath) {
        Remove-Item $tempPublishPath -Recurse -Force
    }
    Copy-Item $publishPath $tempPublishPath -Recurse
    Remove-Item "$tempPublishPath\web.config" -Force -ErrorAction SilentlyContinue
    
    # Build SCP command
    $scpCommand = "scp -r -P $port `"$tempPublishPath\*`" ${username}@${vpsHost}:$($remotePath -replace '\\', '/')/"
    
    Write-Host "Executing: $scpCommand" -ForegroundColor Gray
    Invoke-Expression $scpCommand
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Files deployed successfully via SCP!" -ForegroundColor Green
        
        # Restore web.config if backup exists
        if (Test-Path "$env:TEMP\web.config.backup") {
            Write-Host "Restoring web.config..." -ForegroundColor Gray
            $restoreCmd = "scp -P $port `"$env:TEMP\web.config.backup`" ${username}@${vpsHost}:$($remotePath -replace '\\', '/')/web.config"
            Invoke-Expression $restoreCmd
            Write-Host "web.config restored successfully!" -ForegroundColor Green
        }
    } else {
        Write-Host "Error: SCP deployment failed. Exit code: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
    
    # Cleanup temp folder
    Remove-Item $tempPublishPath -Recurse -Force -ErrorAction SilentlyContinue
} else {
    Write-Host "Error: No deployment method specified. Set useNetworkShare or useSCP to true." -ForegroundColor Red
    exit 1
}

# Restart IIS if enabled
if ($config.app.restartIIS) {
    Write-Host "Restarting IIS..." -ForegroundColor Yellow
    
    if ($config.vps.useNetworkShare) {
        $hostname = $config.vps.host
        $username = $config.vps.username
        $password = $config.vps.password | ConvertTo-SecureString -AsPlainText -Force
        $credential = New-Object System.Management.Automation.PSCredential($username, $password)
        
        # Check if VPS is in TrustedHosts
        $trustedHosts = (Get-Item WSMan:\localhost\Client\TrustedHosts).Value
        $needsTrustedHost = $true
        
        if ($trustedHosts) {
            $trustedHostsList = $trustedHosts -split ','
            foreach ($hostItem in $trustedHostsList) {
                if ($hostItem.Trim() -eq $hostname -or $hostItem.Trim() -eq '*') {
                    $needsTrustedHost = $false
                    break
                }
            }
        }
        
        # Add to TrustedHosts if needed
        if ($needsTrustedHost) {
            Write-Host "Adding VPS to TrustedHosts (required for PowerShell Remoting)..." -ForegroundColor Gray
            try {
                $currentTrustedHosts = (Get-Item WSMan:\localhost\Client\TrustedHosts).Value
                if ($currentTrustedHosts) {
                    Set-Item WSMan:\localhost\Client\TrustedHosts -Value "$currentTrustedHosts,$hostname" -Force
                } else {
                    Set-Item WSMan:\localhost\Client\TrustedHosts -Value $hostname -Force
                }
                Write-Host "VPS added to TrustedHosts successfully." -ForegroundColor Green
            } catch {
                Write-Host "Warning: Could not add VPS to TrustedHosts. You may need to run PowerShell as Administrator." -ForegroundColor Yellow
                Write-Host "Or run manually: Set-Item WSMan:\localhost\Client\TrustedHosts -Value '$hostname' -Force" -ForegroundColor Yellow
            }
        }
        
        # Try to restart IIS
        try {
            $result = Invoke-Command -ComputerName $hostname -Credential $credential -ScriptBlock {
                iisreset /restart
                return @{ Success = $true; Message = "IIS restarted successfully." }
            } -ErrorAction Stop
            
            if ($result.Success) {
                Write-Host "IIS restarted successfully!" -ForegroundColor Green
            }
        } catch {
            Write-Host "Error: Could not restart IIS via PowerShell remoting." -ForegroundColor Red
            Write-Host "Error details: $_" -ForegroundColor Red
            Write-Host ""
            Write-Host "Possible solutions:" -ForegroundColor Yellow
            Write-Host "1. Enable PowerShell Remoting on VPS: Enable-PSRemoting -Force" -ForegroundColor Yellow
            Write-Host "2. Run PowerShell as Administrator and add VPS to TrustedHosts:" -ForegroundColor Yellow
            Write-Host "   Set-Item WSMan:\localhost\Client\TrustedHosts -Value '$hostname' -Force" -ForegroundColor Yellow
            Write-Host "3. Restart IIS manually on the VPS: iisreset /restart" -ForegroundColor Yellow
        }
    } else {
        Write-Host "IIS restart requires PowerShell remoting. Please restart manually: iisreset /restart" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Deployment completed!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan