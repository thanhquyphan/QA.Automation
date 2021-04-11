Start-Sleep -s 1

$reportDir=$args[0]
$executionTime=$args[1];

$fileRegex =  '.*_(\d{4}-\d{2}-\d{2}T\d{6}).html'

write-host $reportDir
write-host $executionTime

$mostRecentReport = Get-ChildItem -Path $reportDir |
    Where-Object -FilterScript {$_.Name -match $fileRegex} |
    Sort-Object -Descending |
    Select-Object -First 1

    write-host $mostRecentReport.Name;

    $matches = [regex]::Matches($mostRecentReport.Name, $fileRegex)
    $fileNameDateTimePart = $matches[0].Groups[1].Value;

    if($fileNameDateTimePart -gt $executionTime){
     Start-Process $mostRecentReport.FullName
    }else{
        write-host "No report found to open. Most recent report was" $fileNameDateTimePart "which was before test execution time of" $executionTime
    }