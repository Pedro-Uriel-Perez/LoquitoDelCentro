# Abre cada manual en Word, actualiza el índice, guarda el .docx y exporta el PDF.
# Uso: powershell -ExecutionPolicy Bypass -File docs/manuales/exportar_pdf.ps1 [Manual_de_Instalacion|Manual_de_Usuario]
param([string[]]$Nombres = @('Manual_de_Instalacion', 'Manual_de_Usuario'), [string]$Carpeta = '')
$dir = if ($Carpeta) { (Resolve-Path $Carpeta).Path } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
function Log($m) { "{0:HH:mm:ss} {1}" -f (Get-Date), $m }

$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0
$word.Options.CheckGrammarAsYouType = $false
$word.Options.CheckSpellingAsYouType = $false
try {
    foreach ($name in $Nombres) {
        $docx = Join-Path $dir "$name.docx"
        $pdf = Join-Path $dir "$name.pdf"
        Log "abriendo $name"
        # Open(FileName, ConfirmConversions, ReadOnly, AddToRecentFiles)
        $doc = $word.Documents.Open($docx, $false, $false, $false)
        Log "indice"
        foreach ($toc in $doc.TablesOfContents) { $toc.Update() }
        Log "guardando"
        $doc.Save()
        Log "exportando PDF"
        $doc.SaveAs2([string]$pdf, 17)   # 17 = wdFormatPDF
        Log ("{0}: {1} paginas" -f $name, $doc.ComputeStatistics(2))
        $doc.Close($false)
    }
} finally {
    $word.Quit()
}
