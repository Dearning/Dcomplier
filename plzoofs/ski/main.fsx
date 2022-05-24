#nowarn "62"

#r "nuget: ScanRat, 1.0.0"
#load "Ski.fs" "Parser.fs"

open Parser
parseAst @"\x.fxxx"