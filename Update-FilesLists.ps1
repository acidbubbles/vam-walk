cd (Split-Path ($MyInvocation.MyCommand.Path))

# Get source files
$sourceFiles = ( `
    ls ./src/*.cs -Recurse `
    | % { $_.FullName.Substring((pwd).Path.Length + 1) } `
)

# Walk.cslist
$sourceFiles > .\Walk.cslist

# Walk.csproj
( Get-Content ".\Walk.csproj" -Raw ) -Replace "(?sm)(?<=^ +<!-- SourceFiles -->`r?`n).*?(?=`r?`n +<!-- /SourceFiles -->)", `
    [System.String]::Join("`r`n", ($sourceFiles | % { "    <Compile Include=`"$_`" />" } ) ) `
| Set-Content ".\Walk.csproj" -NoNewline

# meta.json
$allFiles = (ls ./src/*.cs -Recurse) + (ls *.cslist) `
    | % { $_.FullName.Substring((pwd).Path.Length + 1) }
( Get-Content ".\meta.json" -Raw ) -Replace "(?sm)(?<=^ *`"contentList`" ?: ?\[ ?`r?`n).*?(?=`r?`n *\],)", `
    [System.String]::Join("`r`n", ($allFiles | % { "    `"Custom\\Scripts\\AcidBubbles\\Walk\\$($_.Replace("\", "\\"))`"," } ) ).Trim(",") `
| Set-Content ".\meta.json" -NoNewline
