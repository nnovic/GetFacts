set zipfile=GetFacts_0.2_Portable.zip
del .\Output\%zipfile%
.\7-Zip\7za.exe a -x!GetFactsApp.exe -x!*.xml -x!*.pdb .\Output\%zipfile% .\GetFactsApp\bin\Debug\*
pause