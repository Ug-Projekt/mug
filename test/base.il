.assembly extern mscorlib {}
.assembly base {
	.ver 0:0:0:1
}
.method public static void name() cil managed {
	.entrypoint
	.maxstack 8
	.locals init (
	)

	ldstr "Hello World!"
	call void [mscorlib]System.Console::WriteLine(string)
	ldstr "Hello World!"
	call void [mscorlib]System.Console::WriteLine(string)
	IL_EOM: ret 
}