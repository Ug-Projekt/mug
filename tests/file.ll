declare i32 @puts(i8*)
declare void @exit(i32)
declare i32 @system(i8*)

@err0 = constant [27 x i8] c"Panic: use of null pointer\00"
@null = constant i8* null
define void @cnll(i8*) {
  %n = load i8*, i8** @null
  %a = icmp eq i8* %0, %n
  br i1 %a, label %iftrue, label %else
  iftrue:
   %x = getelementptr [27 x i8], [27 x i8]* @err0, i32 0, i32 0
   call i32 @puts(i8* %x)
   call void @exit(i32 1)
   ret void
  else:
   ret void
}
define void @main() {
  ret void
}