
Function Install-AzureConfiguaration {
    [CmdletBinding(ConfirmImpact="High")]
    param(
            $azureCredentials = (Get-Credential -Message "Enter your azure credentials here.")
        )

        if(!$PSCmdlet.ShouldProcess("Install-AzureConfiguaration","Untested script... confirm to execute", "3")) {
            return
        }

        Choco Install AzurePowershell

        Add-AzureAccount -Credential $azureCredentials
        
        Get-AzurePublishSettingsFile
}

Function Initialize-Azure {
    if(!(Get-Module Azure -ListAvailable)) {
        Write-Warning "The Azure module is not installed.  To install run 'Choco Install WindowsAzurePowershell'"
        return
    }

    Import-Module Azure 
    Set-AzureSubscription -SubscriptionName "Azure Pass" 
}


Function New-AzureVM {
    [CmdletBinding()] Param(
        [Parameter(Mandatory)][string]$imageName
        , [Parameter(Mandatory)][string] $newName
        , [PSCredential] $credential
        , [string] $instanceSize = "Small"
        , [string] $location = "West US"
        #[OSImageContext] $image
    )

    $vmConfig = New-AzureVMConfig -Name $newName -InstanceSize $instanceSize -Image $imageName |
        Add-AzureProvisioningConfig -Windows -AdminUserName $credential.UserName -Password (Get-CredentialPassword $credential) |
        . Azure\New-AzureVM -ServiceName $newName -Location $location
} 


Function Enter-AzurePSSEssion {
<#
    .SYNOPSIS
        Enter into a Remote PowerShell session runnning on Azure VM.
#>
    [CmdletBinding()]Param(
        #The specific virtual machine from which the certificate should be imported.
        [Parameter(Mandatory,ValueFromPipeline,ParameterSetName="InputObject")]
            [Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMRoleContext]$inputObject,
        [Parameter(Mandatory,ValueFromPipeline)][string]$dnsName,
        $credential = (Get-Credential) 
        #TODO: Add a parameter set that takes a session
    )

    switch ($PsCmdlet.ParameterSetName) 
    { 
        "InputObject"  { $dnsName = $inputObject.Name; break} 
    } 

    
    if($dnsName -notlike "*.cloudapp.net") {
        $dnsName = "$dnsName.cloudapp.net"
    }

    try {
        $pssession = New-AzurePSSEssion $dnsName $credential
        Enter-PSSession -session $pssession
        return $pssession

    }
    #TODO Catch not working!!!
    catch <#[System.Management.Automation.Remoting.PSRemotingTransportException]#> {
        switch -Wildcard ($_.Message) {
            "*The WinRM client cannot process the request because the server name cannot be resolved.*" {
                Throw "Either the virtual machine is off or the port, '5986', is incorret."
            }
        }
    }
}


Function New-AzurePSSession {
    [CmdletBinding()]Param(
        [Parameter(Mandatory,ValueFromPipeline)][string]$dnsName,
        $credential = (Get-Credential) 
    )
    
    if($dnsName -notlike "*.cloudapp.net") {
        $dnsName = "$dnsName.cloudapp.net"
    }

    try {
        $pssession = New-PSSession -ComputerName $dnsName -Port 5986 -Credential $credential -UseSSL
        return $pssession
    }
    #TODO Catch not working!!!
    catch <#[System.Management.Automation.Remoting.PSRemotingTransportException]#> {
        switch -Wildcard ($_.Message) {
            "*The WinRM client cannot process the request because the server name cannot be resolved.*" {
                Throw "Either the virtual machine is off or the port, '5986', is incorret."
            }
        }
    }
}

Function Reset-AzureVMCredentials {
    [CmdletBinding()] Param (
        [string]$serverURL,
        [PSCredential]$newCredential
    )

    $password = Get-CredentialPassword($newCredential)

    get-azurevm $serverURL | Set-AzureVMAccessExtension -UserName $newCredential.UserName -Password $password |Update-AzureVM
}

Function Import-AzureVMCertificate {
<#
    .SYNOPSIS
        Import the Azure Virtual Machine Certificate
#>
    [CmdletBinding()]Param(
        #The specific virtual machine from which the certificate should be imported.
        [Parameter(Mandatory,ValueFromPipeline,ParameterSetName="InputObject")][Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMRoleContext]$inputObject,
        #Cloud Service name/DNS name for your VM (without the .cloudapp.net part)
        [Parameter(Mandatory,ParameterSetName="ServiceName")][string]$serviceName
    )

    switch ($PsCmdlet.ParameterSetName) 
    { 
        "InputObject"  { $azureVM = $inputObject; break} 
        "ServiceName"  { $azureVM = Get-AzureVM -ServiceName $serviceName; break} 
    } 

    try{
        $tempFile = [IO.Path]::GetTempFileName()
        (Get-AzureCertificate -ServiceName $azureVM.ServiceName -Thumbprint $azureVM.VM.DefaultWinRmCertificateThumbprint -ThumbprintAlgorithm SHA1).Data | 
            Out-File $tempFile
 
        $X509Object = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 $tempFile
        $X509Store = New-Object System.Security.Cryptography.X509Certificates.X509Store "Root", "LocalMachine"
        
        try {    
            $X509Store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
            $X509Store.Add($X509Object)
        }
        finally {
            $X509Store.Close()
        }
    }
    finally {
        Remove-Item $tempFile
    }
}


#TODO: Move to somewhere more general
Function Get-CredentialPassword{
    [CmdletBinding()] param (
        [Parameter(Mandatory,ValueFromPipeline)][PSCredential]$credential
    )

    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($credential.Password)
    $password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    return $password;
}



Return

Function Register-AzurePublishSettings {
    #Incomplete
    [CmdletBinding()]Param(
        [string]$publishSettingsFilePath = (Join-Path $env:ALLUSERSPROFILE "Azure.publishsettings")
    )
        if(!$publishSettingsFilePath) {
            Get-AzurePublishSettingsFile
            $publishSettingsFilePath = Read-Host -Prompt "Enter the path to the downloaded publishSettings file:"
        }
        Import-AzurePublishSettingsFile $publishSettingsFilePath
        Set-AzureService
        Write-Warning "More stuff needed in order to support 'Get-AzureCertificate'"
        # See  see http://michaelwasham.com/windows-azure-powershell-reference-guide/getting-started-with-windows-azure-powershell/
}

Function Get-AzureStarted {
    #see http://blogs.technet.com/b/heyscriptingguy/archive/2013/06/22/weekend-scripter-getting-started-with-windows-azure-and-powershell.aspx
    Add-AzureAccount
}

