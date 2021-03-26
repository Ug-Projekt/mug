; ModuleID = 'command_line.mug'
source_filename = "command_line.mug"
target datalayout = "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-pc-windows-msvc19.27.29112"

%struct._iobuf = type { i8* }
%struct.__crt_locale_pointers = type { %struct.__crt_locale_data*, %struct.__crt_multibyte_data* }
%struct.__crt_locale_data = type opaque
%struct.__crt_multibyte_data = type opaque

$printf = comdat any

$__local_stdio_printf_options = comdat any

$"??_C@_02DPKJAMEF@?$CFd?$AA@" = comdat any

$"??_C@_02HAOIJKIC@?$CFc?$AA@" = comdat any

$"??_C@_02DKCKIIND@?$CFs?$AA@" = comdat any

$"??_C@_03OFAPEBGM@?$CFs?6?$AA@" = comdat any

$"??_C@_03LCPHGAHP@cls?$AA@" = comdat any

@"??_C@_02DPKJAMEF@?$CFd?$AA@" = linkonce_odr dso_local unnamed_addr constant [3 x i8] c"%d\00", comdat, align 1
@__local_stdio_printf_options._OptionsStorage = internal global i64 0, align 8
@"??_C@_02HAOIJKIC@?$CFc?$AA@" = linkonce_odr dso_local unnamed_addr constant [3 x i8] c"%c\00", comdat, align 1
@"??_C@_02DKCKIIND@?$CFs?$AA@" = linkonce_odr dso_local unnamed_addr constant [3 x i8] c"%s\00", comdat, align 1
@"??_C@_03OFAPEBGM@?$CFs?6?$AA@" = linkonce_odr dso_local unnamed_addr constant [4 x i8] c"%s\0A\00", comdat, align 1
@"??_C@_03LCPHGAHP@cls?$AA@" = linkonce_odr dso_local unnamed_addr constant [4 x i8] c"cls\00", comdat, align 1
@CommandLine = external global { i8*, i8**, i32, i1 }
@0 = private unnamed_addr constant [1 x i8] zeroinitializer, align 1
@1 = private unnamed_addr constant [1 x i8] zeroinitializer, align 1
@2 = private unnamed_addr constant [4 x i8] c"$> \00", align 1
@Stack = external global { i8**, i32, i32 }
@3 = private unnamed_addr constant [1 x i8] zeroinitializer, align 1
@4 = private unnamed_addr constant [25 x i8] c"reached max stack length\00", align 1
@5 = private unnamed_addr constant [1 x i8] zeroinitializer, align 1
@6 = private unnamed_addr constant [28 x i8] c"called pop over empty stack\00", align 1
@7 = private unnamed_addr constant [4 x i8] c"cls\00", align 1
@8 = private unnamed_addr constant [9 x i8] c"echo_off\00", align 1
@9 = private unnamed_addr constant [8 x i8] c"echo_on\00", align 1
@10 = private unnamed_addr constant [5 x i8] c"echo\00", align 1
@11 = private unnamed_addr constant [3 x i8] c"ls\00", align 1
@12 = private unnamed_addr constant [2 x i8] c".\00", align 1
@13 = private unnamed_addr constant [18 x i8] c"Invalid command `\00", align 1
@14 = private unnamed_addr constant [2 x i8] c"`\00", align 1
@15 = private unnamed_addr constant [8 x i8] c"Error: \00", align 1

; Function Attrs: nounwind uwtable
define dso_local void @print_int(i32 %0) local_unnamed_addr #0 {
  %2 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @"??_C@_02DPKJAMEF@?$CFd?$AA@", i64 0, i64 0), i32 %0)
  ret void
}

; Function Attrs: inlinehint nobuiltin nounwind uwtable
define linkonce_odr dso_local i32 @printf(i8* %0, ...) local_unnamed_addr #1 comdat {
  %2 = alloca i8*, align 8
  %3 = bitcast i8** %2 to i8*
  call void @llvm.lifetime.start.p0i8(i64 8, i8* nonnull %3) #3
  call void @llvm.va_start(i8* nonnull %3)
  %4 = load i8*, i8** %2, align 8, !tbaa !3
  %5 = call %struct._iobuf* @__acrt_iob_func(i32 1) #3
  %6 = call i64* @__local_stdio_printf_options() #3
  %7 = load i64, i64* %6, align 8, !tbaa !7
  %8 = call i32 @__stdio_common_vfprintf(i64 %7, %struct._iobuf* %5, i8* %0, %struct.__crt_locale_pointers* null, i8* %4) #3
  call void @llvm.va_end(i8* nonnull %3)
  call void @llvm.lifetime.end.p0i8(i64 8, i8* nonnull %3) #3
  ret i32 %8
}

; Function Attrs: argmemonly nounwind willreturn
declare void @llvm.lifetime.start.p0i8(i64 immarg, i8* nocapture) #2

; Function Attrs: nounwind
declare void @llvm.va_start(i8*) #3

declare dso_local %struct._iobuf* @__acrt_iob_func(i32) local_unnamed_addr #4

; Function Attrs: noinline nounwind uwtable
define linkonce_odr dso_local i64* @__local_stdio_printf_options() local_unnamed_addr #5 comdat {
  ret i64* @__local_stdio_printf_options._OptionsStorage
}

declare dso_local i32 @__stdio_common_vfprintf(i64, %struct._iobuf*, i8*, %struct.__crt_locale_pointers*, i8*) local_unnamed_addr #4

; Function Attrs: nounwind
declare void @llvm.va_end(i8*) #3

; Function Attrs: argmemonly nounwind willreturn
declare void @llvm.lifetime.end.p0i8(i64 immarg, i8* nocapture) #2

; Function Attrs: nounwind uwtable
define dso_local void @print_char(i8 %0) local_unnamed_addr #0 {
  %2 = sext i8 %0 to i32
  %3 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @"??_C@_02HAOIJKIC@?$CFc?$AA@", i64 0, i64 0), i32 %2)
  ret void
}

; Function Attrs: nounwind uwtable
define dso_local void @print_string(i8* %0) local_unnamed_addr #0 {
  %2 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @"??_C@_02DKCKIIND@?$CFs?$AA@", i64 0, i64 0), i8* %0)
  ret void
}

; Function Attrs: nounwind uwtable
define dso_local void @println_string(i8* %0) local_unnamed_addr #0 {
  %2 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @"??_C@_03OFAPEBGM@?$CFs?6?$AA@", i64 0, i64 0), i8* %0)
  ret void
}

; Function Attrs: nounwind uwtable
define dso_local i8* @readln(i8* %0) local_unnamed_addr #0 {
  %2 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @"??_C@_02DKCKIIND@?$CFs?$AA@", i64 0, i64 0), i8* %0)
  %3 = tail call noalias dereferenceable_or_null(100) i8* @malloc(i64 100)
  %4 = tail call i8* @gets_s(i8* %3, i64 100) #3
  br label %10

5:                                                ; preds = %10
  %6 = add nuw nsw i64 %11, 1
  %7 = getelementptr inbounds i8, i8* %3, i64 %6
  %8 = load i8, i8* %7, align 1, !tbaa !9
  %9 = icmp eq i8 %8, 0
  br i1 %9, label %15, label %21

10:                                               ; preds = %36, %1
  %11 = phi i64 [ 0, %1 ], [ %37, %36 ]
  %12 = getelementptr inbounds i8, i8* %3, i64 %11
  %13 = load i8, i8* %12, align 1, !tbaa !9
  %14 = icmp eq i8 %13, 0
  br i1 %14, label %15, label %5

15:                                               ; preds = %31, %26, %21, %5, %10
  %16 = phi i64 [ %11, %10 ], [ %6, %5 ], [ %22, %21 ], [ %27, %26 ], [ %32, %31 ]
  %17 = and i64 %16, 4294967295
  %18 = tail call noalias i8* @realloc(i8* nonnull %3, i64 %17)
  br label %19

19:                                               ; preds = %36, %15
  %20 = phi i8* [ %18, %15 ], [ %3, %36 ]
  ret i8* %20

21:                                               ; preds = %5
  %22 = add nuw nsw i64 %11, 2
  %23 = getelementptr inbounds i8, i8* %3, i64 %22
  %24 = load i8, i8* %23, align 1, !tbaa !9
  %25 = icmp eq i8 %24, 0
  br i1 %25, label %15, label %26

26:                                               ; preds = %21
  %27 = add nuw nsw i64 %11, 3
  %28 = getelementptr inbounds i8, i8* %3, i64 %27
  %29 = load i8, i8* %28, align 1, !tbaa !9
  %30 = icmp eq i8 %29, 0
  br i1 %30, label %15, label %31

31:                                               ; preds = %26
  %32 = add nuw nsw i64 %11, 4
  %33 = getelementptr inbounds i8, i8* %3, i64 %32
  %34 = load i8, i8* %33, align 1, !tbaa !9
  %35 = icmp eq i8 %34, 0
  br i1 %35, label %15, label %36

36:                                               ; preds = %31
  %37 = add nuw nsw i64 %11, 5
  %38 = icmp eq i64 %37, 100
  br i1 %38, label %19, label %10
}

; Function Attrs: nofree nounwind
declare dso_local noalias i8* @malloc(i64) local_unnamed_addr #6

declare dso_local i8* @gets_s(i8*, i64) local_unnamed_addr #4

; Function Attrs: nounwind
declare dso_local noalias i8* @realloc(i8* nocapture, i64) local_unnamed_addr #7

; Function Attrs: nofree nounwind uwtable
define dso_local noalias i8* @string_concat(i8* nocapture readonly %0, i8* nocapture readonly %1) local_unnamed_addr #8 {
  %3 = tail call i64 @strlen(i8* nonnull dereferenceable(1) %0)
  %4 = tail call i64 @strlen(i8* nonnull dereferenceable(1) %1)
  %5 = add i64 %4, %3
  %6 = tail call noalias i8* @malloc(i64 %5)
  %7 = load i8, i8* %0, align 1, !tbaa !9
  %8 = icmp eq i8 %7, 0
  br i1 %8, label %11, label %16

9:                                                ; preds = %16
  %10 = and i64 %20, 4294967295
  br label %11

11:                                               ; preds = %9, %2
  %12 = phi i64 [ 0, %2 ], [ %10, %9 ]
  %13 = load i8, i8* %1, align 1, !tbaa !9
  %14 = icmp eq i8 %13, 0
  %15 = getelementptr inbounds i8, i8* %6, i64 %12
  br i1 %14, label %35, label %24

16:                                               ; preds = %2, %16
  %17 = phi i64 [ %20, %16 ], [ 0, %2 ]
  %18 = phi i8 [ %22, %16 ], [ %7, %2 ]
  %19 = getelementptr inbounds i8, i8* %6, i64 %17
  store i8 %18, i8* %19, align 1, !tbaa !9
  %20 = add nuw nsw i64 %17, 1
  %21 = getelementptr inbounds i8, i8* %0, i64 %20
  %22 = load i8, i8* %21, align 1, !tbaa !9
  %23 = icmp eq i8 %22, 0
  br i1 %23, label %9, label %16

24:                                               ; preds = %11, %24
  %25 = phi i64 [ %29, %24 ], [ 0, %11 ]
  %26 = phi i64 [ %30, %24 ], [ %12, %11 ]
  %27 = phi i8* [ %34, %24 ], [ %15, %11 ]
  %28 = phi i8 [ %32, %24 ], [ %13, %11 ]
  store i8 %28, i8* %27, align 1, !tbaa !9
  %29 = add nuw nsw i64 %25, 1
  %30 = add nuw i64 %26, 1
  %31 = getelementptr inbounds i8, i8* %1, i64 %29
  %32 = load i8, i8* %31, align 1, !tbaa !9
  %33 = icmp eq i8 %32, 0
  %34 = getelementptr inbounds i8, i8* %6, i64 %30
  br i1 %33, label %35, label %24

35:                                               ; preds = %24, %11
  %36 = phi i8* [ %15, %11 ], [ %34, %24 ]
  store i8 0, i8* %36, align 1, !tbaa !9
  ret i8* %6
}

; Function Attrs: argmemonly nofree nounwind readonly
declare dso_local i64 @strlen(i8* nocapture) local_unnamed_addr #9

; Function Attrs: nofree nounwind uwtable
define dso_local noalias i8* @string_concat_char(i8* nocapture readonly %0, i8 %1) local_unnamed_addr #8 {
  %3 = tail call i64 @strlen(i8* nonnull dereferenceable(1) %0)
  %4 = add i64 %3, 2
  %5 = tail call noalias i8* @malloc(i64 %4)
  %6 = load i8, i8* %0, align 1, !tbaa !9
  %7 = icmp eq i8 %6, 0
  br i1 %7, label %20, label %8

8:                                                ; preds = %2, %8
  %9 = phi i64 [ %12, %8 ], [ 0, %2 ]
  %10 = phi i8* [ %16, %8 ], [ %5, %2 ]
  %11 = phi i8 [ %14, %8 ], [ %6, %2 ]
  store i8 %11, i8* %10, align 1, !tbaa !9
  %12 = add nuw nsw i64 %9, 1
  %13 = getelementptr inbounds i8, i8* %0, i64 %12
  %14 = load i8, i8* %13, align 1, !tbaa !9
  %15 = icmp eq i8 %14, 0
  %16 = getelementptr inbounds i8, i8* %5, i64 %12
  br i1 %15, label %17, label %8

17:                                               ; preds = %8
  %18 = add nuw i64 %9, 2
  %19 = and i64 %18, 4294967295
  br label %20

20:                                               ; preds = %17, %2
  %21 = phi i64 [ 1, %2 ], [ %19, %17 ]
  %22 = phi i8* [ %5, %2 ], [ %16, %17 ]
  store i8 %1, i8* %22, align 1, !tbaa !9
  %23 = getelementptr inbounds i8, i8* %5, i64 %21
  store i8 0, i8* %23, align 1, !tbaa !9
  ret i8* %5
}

; Function Attrs: nounwind uwtable
define dso_local i8* @int_to_string(i32 %0) local_unnamed_addr #0 {
  %2 = tail call noalias dereferenceable_or_null(15) i8* @malloc(i64 15)
  %3 = tail call i8* @_itoa(i32 %0, i8* %2, i32 10) #3
  %4 = load i8, i8* %2, align 1, !tbaa !9
  %5 = icmp eq i8 %4, 0
  br i1 %5, label %10, label %6

6:                                                ; preds = %1
  %7 = getelementptr inbounds i8, i8* %2, i64 1
  %8 = load i8, i8* %7, align 1, !tbaa !9
  %9 = icmp eq i8 %8, 0
  br i1 %9, label %10, label %16

10:                                               ; preds = %64, %60, %56, %52, %48, %44, %40, %36, %32, %28, %24, %20, %16, %6, %1
  %11 = phi i64 [ 0, %1 ], [ 1, %6 ], [ 2, %16 ], [ 3, %20 ], [ 4, %24 ], [ 5, %28 ], [ 6, %32 ], [ 7, %36 ], [ 8, %40 ], [ 9, %44 ], [ 10, %48 ], [ 11, %52 ], [ 12, %56 ], [ 13, %60 ], [ 14, %64 ]
  %12 = tail call noalias i8* @realloc(i8* nonnull %2, i64 %11)
  %13 = getelementptr inbounds i8, i8* %12, i64 %11
  store i8 0, i8* %13, align 1, !tbaa !9
  br label %14

14:                                               ; preds = %64, %10
  %15 = phi i8* [ %12, %10 ], [ %2, %64 ]
  ret i8* %15

16:                                               ; preds = %6
  %17 = getelementptr inbounds i8, i8* %2, i64 2
  %18 = load i8, i8* %17, align 1, !tbaa !9
  %19 = icmp eq i8 %18, 0
  br i1 %19, label %10, label %20

20:                                               ; preds = %16
  %21 = getelementptr inbounds i8, i8* %2, i64 3
  %22 = load i8, i8* %21, align 1, !tbaa !9
  %23 = icmp eq i8 %22, 0
  br i1 %23, label %10, label %24

24:                                               ; preds = %20
  %25 = getelementptr inbounds i8, i8* %2, i64 4
  %26 = load i8, i8* %25, align 1, !tbaa !9
  %27 = icmp eq i8 %26, 0
  br i1 %27, label %10, label %28

28:                                               ; preds = %24
  %29 = getelementptr inbounds i8, i8* %2, i64 5
  %30 = load i8, i8* %29, align 1, !tbaa !9
  %31 = icmp eq i8 %30, 0
  br i1 %31, label %10, label %32

32:                                               ; preds = %28
  %33 = getelementptr inbounds i8, i8* %2, i64 6
  %34 = load i8, i8* %33, align 1, !tbaa !9
  %35 = icmp eq i8 %34, 0
  br i1 %35, label %10, label %36

36:                                               ; preds = %32
  %37 = getelementptr inbounds i8, i8* %2, i64 7
  %38 = load i8, i8* %37, align 1, !tbaa !9
  %39 = icmp eq i8 %38, 0
  br i1 %39, label %10, label %40

40:                                               ; preds = %36
  %41 = getelementptr inbounds i8, i8* %2, i64 8
  %42 = load i8, i8* %41, align 1, !tbaa !9
  %43 = icmp eq i8 %42, 0
  br i1 %43, label %10, label %44

44:                                               ; preds = %40
  %45 = getelementptr inbounds i8, i8* %2, i64 9
  %46 = load i8, i8* %45, align 1, !tbaa !9
  %47 = icmp eq i8 %46, 0
  br i1 %47, label %10, label %48

48:                                               ; preds = %44
  %49 = getelementptr inbounds i8, i8* %2, i64 10
  %50 = load i8, i8* %49, align 1, !tbaa !9
  %51 = icmp eq i8 %50, 0
  br i1 %51, label %10, label %52

52:                                               ; preds = %48
  %53 = getelementptr inbounds i8, i8* %2, i64 11
  %54 = load i8, i8* %53, align 1, !tbaa !9
  %55 = icmp eq i8 %54, 0
  br i1 %55, label %10, label %56

56:                                               ; preds = %52
  %57 = getelementptr inbounds i8, i8* %2, i64 12
  %58 = load i8, i8* %57, align 1, !tbaa !9
  %59 = icmp eq i8 %58, 0
  br i1 %59, label %10, label %60

60:                                               ; preds = %56
  %61 = getelementptr inbounds i8, i8* %2, i64 13
  %62 = load i8, i8* %61, align 1, !tbaa !9
  %63 = icmp eq i8 %62, 0
  br i1 %63, label %10, label %64

64:                                               ; preds = %60
  %65 = getelementptr inbounds i8, i8* %2, i64 14
  %66 = load i8, i8* %65, align 1, !tbaa !9
  %67 = icmp eq i8 %66, 0
  br i1 %67, label %10, label %14
}

declare dso_local i8* @_itoa(i32, i8*, i32) local_unnamed_addr #4

; Function Attrs: nounwind readonly uwtable
define dso_local i32 @string_compare(i8* nocapture readonly %0, i8* nocapture readonly %1) local_unnamed_addr #10 {
  %3 = tail call i64 @strlen(i8* nonnull dereferenceable(1) %0)
  %4 = and i64 %3, 4294967295
  %5 = tail call i64 @strlen(i8* nonnull dereferenceable(1) %1)
  %6 = icmp ne i64 %4, %5
  %7 = trunc i64 %3 to i32
  %8 = icmp eq i32 %7, 0
  %9 = or i1 %6, %8
  br i1 %9, label %14, label %10

10:                                               ; preds = %2
  %11 = tail call i32 @strcmp(i8* nonnull dereferenceable(1) %0, i8* nonnull dereferenceable(1) %1)
  %12 = icmp eq i32 %11, 0
  %13 = zext i1 %12 to i32
  br label %14

14:                                               ; preds = %2, %10
  %15 = phi i32 [ %13, %10 ], [ 0, %2 ]
  ret i32 %15
}

; Function Attrs: nofree nounwind readonly
declare dso_local i32 @strcmp(i8* nocapture, i8* nocapture) local_unnamed_addr #11

; Function Attrs: nofree norecurse nounwind uwtable writeonly
define dso_local void @set_char_in_string_at_index(i8* nocapture %0, i32 %1, i8 %2) local_unnamed_addr #12 {
  %4 = sext i32 %1 to i64
  %5 = getelementptr inbounds i8, i8* %0, i64 %4
  store i8 %2, i8* %5, align 1, !tbaa !9
  ret void
}

declare void @exit(i32)

declare i64 @_msize(i8*)

declare void @free(i8*)

; Function Attrs: nofree nounwind uwtable
define dso_local void @clear() local_unnamed_addr #8 {
  %1 = tail call i32 @system(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @"??_C@_03LCPHGAHP@cls?$AA@", i64 0, i64 0)) #3
  ret void
}

; Function Attrs: nofree
declare dso_local i32 @system(i8* nocapture readonly) local_unnamed_addr #13

define void @main() {
  %1 = alloca { i8*, i8**, i32, i1 }, align 8
  %2 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %1, i32 0, i32 3
  store i1 true, i1* %2, align 1
  %3 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %1, i32 0, i32 0
  store i8* getelementptr inbounds ([1 x i8], [1 x i8]* @0, i32 0, i32 0), i8** %3, align 8
  %malloccall = tail call i8* bitcast (i8* (i64)* @malloc to i8* (i32)*)(i32 0)
  %4 = bitcast i8* %malloccall to i8**
  %5 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %1, i32 0, i32 1
  store i8** %4, i8*** %5, align 8
  %6 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %1, i32 0, i32 2
  store i32 0, i32* %6, align 4
  %7 = load { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %1, align 8
  %cmd = alloca { i8*, i8**, i32, i1 }, align 8
  store { i8*, i8**, i32, i1 } %7, { i8*, i8**, i32, i1 }* %cmd, align 8
  br label %8

8:                                                ; preds = %22, %0
  br i1 true, label %9, label %12

9:                                                ; preds = %8
  %prompt = alloca i8*, align 8
  store i8* getelementptr inbounds ([1 x i8], [1 x i8]* @1, i32 0, i32 0), i8** %prompt, align 8
  %10 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %cmd, i32 0, i32 3
  %11 = load i1, i1* %10, align 1
  br i1 %11, label %13, label %14

12:                                               ; preds = %8
  ret void

13:                                               ; preds = %9
  store i8* getelementptr inbounds ([4 x i8], [4 x i8]* @2, i32 0, i32 0), i8** %prompt, align 8
  br label %14

14:                                               ; preds = %13, %9
  %15 = load i8*, i8** %prompt, align 8
  %16 = call i8* @readln(i8* %15)
  %17 = call { i8**, i32, i32 } @"str.split(chr)"(i8* %16, i8 32)
  %splitted = alloca { i8**, i32, i32 }, align 8
  store { i8**, i32, i32 } %17, { i8**, i32, i32 }* %splitted, align 8
  %18 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %splitted, i32 0, i32 1
  %19 = load i32, i32* %18, align 4
  %20 = icmp eq i32 %19, 0
  br i1 %20, label %21, label %22

21:                                               ; preds = %21, %14
  br label %21

22:                                               ; preds = %14
  %23 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %splitted, align 8
  %24 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %cmd, i32 0, i32 0
  %25 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %cmd, i32 0, i32 1
  %26 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %cmd, i32 0, i32 2
  call void @"get_command_and_args(Stack, *str, *[str], *i32)"({ i8**, i32, i32 } %23, i8** %24, i8*** %25, i32* %26)
  call void @"*CommandLine.interpret_command()"({ i8*, i8**, i32, i1 }* %cmd)
  br label %8
}

define { i8**, i32, i32 } @"str.split(chr)"(i8* %0, i8 %1) {
  %char = alloca i8, align 1
  store i8 %1, i8* %char, align 1
  %3 = call i64 @strlen(i8* %0)
  %4 = trunc i64 %3 to i32
  %i = alloca i32, align 4
  store i32 0, i32* %i, align 4
  %5 = call { i8**, i32, i32 } @"Stack<str>(i32)"(i32 10)
  %result = alloca { i8**, i32, i32 }, align 8
  store { i8**, i32, i32 } %5, { i8**, i32, i32 }* %result, align 8
  %builder = alloca i8*, align 8
  store i8* getelementptr inbounds ([1 x i8], [1 x i8]* @3, i32 0, i32 0), i8** %builder, align 8
  %builder_len = alloca i32, align 4
  store i32 0, i32* %builder_len, align 4
  br label %6

6:                                                ; preds = %26, %2
  %7 = load i32, i32* %i, align 4
  %8 = icmp slt i32 %7, %4
  br i1 %8, label %9, label %15

9:                                                ; preds = %6
  %10 = load i32, i32* %i, align 4
  %11 = getelementptr i8, i8* %0, i32 %10
  %12 = load i8, i8* %11, align 1
  %13 = load i8, i8* %char, align 1
  %14 = icmp eq i8 %12, %13
  br i1 %14, label %18, label %21

15:                                               ; preds = %6
  %16 = load i32, i32* %builder_len, align 4
  %17 = icmp sgt i32 %16, 0
  br i1 %17, label %32, label %34

18:                                               ; preds = %9
  %19 = load i32, i32* %builder_len, align 4
  %20 = icmp sgt i32 %19, 0
  br i1 %20, label %29, label %31

21:                                               ; preds = %9
  %22 = load i8*, i8** %builder, align 8
  %23 = call i8* @string_concat_char(i8* %22, i8 %12)
  store i8* %23, i8** %builder, align 8
  %24 = load i32, i32* %builder_len, align 4
  %25 = add i32 %24, 1
  store i32 %25, i32* %builder_len, align 4
  br label %26

26:                                               ; preds = %21, %31
  %27 = load i32, i32* %i, align 4
  %28 = add i32 %27, 1
  store i32 %28, i32* %i, align 4
  br label %6

29:                                               ; preds = %18
  %30 = load i8*, i8** %builder, align 8
  call void @".push<str>(str)"({ i8**, i32, i32 }* %result, i8* %30)
  store i8* getelementptr inbounds ([1 x i8], [1 x i8]* @5, i32 0, i32 0), i8** %builder, align 8
  store i32 0, i32* %builder_len, align 4
  br label %31

31:                                               ; preds = %29, %18
  br label %26

32:                                               ; preds = %15
  %33 = load i8*, i8** %builder, align 8
  call void @".push<str>(str)"({ i8**, i32, i32 }* %result, i8* %33)
  br label %34

34:                                               ; preds = %32, %15
  %35 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %result, align 8
  ret { i8**, i32, i32 } %35
}

define { i8**, i32, i32 } @"Stack<str>(i32)"(i32 %0) {
  %capacity = alloca i32, align 4
  store i32 %0, i32* %capacity, align 4
  %2 = alloca { i8**, i32, i32 }, align 8
  %3 = load i32, i32* %capacity, align 4
  %4 = alloca i8**, align 8
  %mallocsize = mul i32 %3, ptrtoint (i1** getelementptr (i1*, i1** null, i32 1) to i32)
  %malloccall = tail call i8* bitcast (i8* (i64)* @malloc to i8* (i32)*)(i32 %mallocsize)
  %5 = bitcast i8* %malloccall to i8**
  store i8** %5, i8*** %4, align 8
  %6 = load i8**, i8*** %4, align 8
  %7 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %2, i32 0, i32 0
  store i8** %6, i8*** %7, align 8
  %8 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %2, i32 0, i32 1
  store i32 0, i32* %8, align 4
  %9 = load i32, i32* %capacity, align 4
  %10 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %2, i32 0, i32 2
  store i32 %9, i32* %10, align 4
  %11 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %2, align 8
  ret { i8**, i32, i32 } %11
}

define void @".push<str>(str)"({ i8**, i32, i32 }* %0, i8* %1) {
  %element = alloca i8*, align 8
  store i8* %1, i8** %element, align 8
  %3 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %4 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 1
  %5 = load i32, i32* %4, align 4
  %6 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %7 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 2
  %8 = load i32, i32* %7, align 4
  %9 = icmp sge i32 %5, %8
  br i1 %9, label %10, label %13

10:                                               ; preds = %2
  %11 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %12 = call i32 @".new_capacity<str>()"({ i8**, i32, i32 } %11)
  call void @".realloc<str>(i32)"({ i8**, i32, i32 }* %0, i32 %12)
  br label %13

13:                                               ; preds = %10, %2
  %14 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %15 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 0
  %16 = load i8**, i8*** %15, align 8
  %17 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %18 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 1
  %19 = load i32, i32* %18, align 4
  %20 = getelementptr i8*, i8** %16, i32 %19
  %21 = load i8*, i8** %element, align 8
  store i8* %21, i8** %20, align 8
  %22 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %23 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 1
  %24 = load i32, i32* %23, align 4
  %25 = add i32 %24, 1
  store i32 %25, i32* %23, align 4
  ret void
}

define i32 @".new_capacity<str>()"({ i8**, i32, i32 } %0) {
  %2 = alloca { i8**, i32, i32 }, align 8
  store { i8**, i32, i32 } %0, { i8**, i32, i32 }* %2, align 8
  %3 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %2, i32 0, i32 2
  %4 = load i32, i32* %3, align 4
  %5 = mul i32 %4, 2
  %6 = icmp sgt i32 %5, 1000000000
  br i1 %6, label %7, label %8

7:                                                ; preds = %1
  call void @"panic(str)"(i8* getelementptr inbounds ([25 x i8], [25 x i8]* @4, i32 0, i32 0))
  br label %8

8:                                                ; preds = %7, %1
  %9 = alloca { i8**, i32, i32 }, align 8
  store { i8**, i32, i32 } %0, { i8**, i32, i32 }* %9, align 8
  %10 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %9, i32 0, i32 2
  %11 = load i32, i32* %10, align 4
  %12 = mul i32 %11, 2
  ret i32 %12
}

define void @"panic(str)"(i8* %0) {
  %error = alloca i8*, align 8
  store i8* %0, i8** %error, align 8
  %2 = load i8*, i8** %error, align 8
  call void @println_string(i8* %2)
  call void @exit(i32 1)
  ret void
}

define void @".realloc<str>(i32)"({ i8**, i32, i32 }* %0, i32 %1) {
  %capacity = alloca i32, align 4
  store i32 %1, i32* %capacity, align 4
  %3 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %4 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 2
  %5 = load i32, i32* %capacity, align 4
  store i32 %5, i32* %4, align 4
  %6 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %7 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 0
  %8 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %9 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 0
  %10 = load i8**, i8*** %9, align 8
  %11 = bitcast i8** %10 to i8*
  %12 = load i32, i32* %capacity, align 4
  %13 = sext i32 %12 to i64
  %14 = call i8* @realloc(i8* %11, i64 %13)
  %15 = bitcast i8* %14 to i8**
  store i8** %15, i8*** %7, align 8
  ret void
}

define void @"get_command_and_args(Stack, *str, *[str], *i32)"({ i8**, i32, i32 } %0, i8** %1, i8*** %2, i32* %3) {
  %splitted = alloca { i8**, i32, i32 }, align 8
  store { i8**, i32, i32 } %0, { i8**, i32, i32 }* %splitted, align 8
  %command = alloca i8**, align 8
  store i8** %1, i8*** %command, align 8
  %args = alloca i8***, align 8
  store i8*** %2, i8**** %args, align 8
  %args_count = alloca i32*, align 8
  store i32* %3, i32** %args_count, align 8
  %5 = load i32*, i32** %args_count, align 8
  %6 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %splitted, i32 0, i32 1
  %7 = load i32, i32* %6, align 4
  %8 = sub i32 %7, 1
  store i32 %8, i32* %5, align 4
  %9 = load i8***, i8**** %args, align 8
  %10 = load i32*, i32** %args_count, align 8
  %11 = load i32, i32* %10, align 4
  %12 = alloca i8**, align 8
  %mallocsize = mul i32 %11, ptrtoint (i1** getelementptr (i1*, i1** null, i32 1) to i32)
  %malloccall = tail call i8* bitcast (i8* (i64)* @malloc to i8* (i32)*)(i32 %mallocsize)
  %13 = bitcast i8* %malloccall to i8**
  store i8** %13, i8*** %12, align 8
  %14 = load i8**, i8*** %12, align 8
  store i8** %14, i8*** %9, align 8
  %i = alloca i32, align 4
  store i32 0, i32* %i, align 4
  br label %15

15:                                               ; preds = %20, %4
  %16 = load i32, i32* %i, align 4
  %17 = load i32*, i32** %args_count, align 8
  %18 = load i32, i32* %17, align 4
  %19 = icmp slt i32 %16, %18
  br i1 %19, label %20, label %32

20:                                               ; preds = %15
  %21 = load i8***, i8**** %args, align 8
  %22 = load i8**, i8*** %21, align 8
  %23 = load i32*, i32** %args_count, align 8
  %24 = load i32, i32* %23, align 4
  %25 = load i32, i32* %i, align 4
  %26 = sub i32 %24, %25
  %27 = sub i32 %26, 1
  %28 = getelementptr i8*, i8** %22, i32 %27
  %29 = call i8* @".pop<str>()"({ i8**, i32, i32 }* %splitted)
  store i8* %29, i8** %28, align 8
  %30 = load i32, i32* %i, align 4
  %31 = add i32 %30, 1
  store i32 %31, i32* %i, align 4
  br label %15

32:                                               ; preds = %15
  %33 = load i8**, i8*** %command, align 8
  %34 = call i8* @".pop<str>()"({ i8**, i32, i32 }* %splitted)
  store i8* %34, i8** %33, align 8
  ret void
}

define i8* @".pop<str>()"({ i8**, i32, i32 }* %0) {
  %2 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %3 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 1
  %4 = load i32, i32* %3, align 4
  %5 = icmp eq i32 %4, 0
  br i1 %5, label %6, label %7

6:                                                ; preds = %1
  call void @"panic(str)"(i8* getelementptr inbounds ([28 x i8], [28 x i8]* @6, i32 0, i32 0))
  br label %7

7:                                                ; preds = %6, %1
  %8 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %9 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 1
  %10 = load i32, i32* %9, align 4
  %11 = sub i32 %10, 1
  store i32 %11, i32* %9, align 4
  %12 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %13 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 0
  %14 = load i8**, i8*** %13, align 8
  %15 = load { i8**, i32, i32 }, { i8**, i32, i32 }* %0, align 8
  %16 = getelementptr { i8**, i32, i32 }, { i8**, i32, i32 }* %0, i32 0, i32 1
  %17 = load i32, i32* %16, align 4
  %18 = getelementptr i8*, i8** %14, i32 %17
  %19 = load i8*, i8** %18, align 8
  ret i8* %19
}

define void @"*CommandLine.interpret_command()"({ i8*, i8**, i32, i1 }* %0) {
  %2 = load { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %0, align 8
  %3 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %0, i32 0, i32 0
  %4 = load i8*, i8** %3, align 8
  %5 = call i1 @"==(str, str)"(i8* %4, i8* getelementptr inbounds ([4 x i8], [4 x i8]* @7, i32 0, i32 0))
  br i1 %5, label %6, label %7

6:                                                ; preds = %1
  call void @clear()
  br label %9

7:                                                ; preds = %1
  %8 = call i1 @"==(str, str)"(i8* %4, i8* getelementptr inbounds ([9 x i8], [9 x i8]* @8, i32 0, i32 0))
  br i1 %8, label %10, label %13

9:                                                ; preds = %26, %24, %20, %15, %10, %6
  ret void

10:                                               ; preds = %7
  %11 = load { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %0, align 8
  %12 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %0, i32 0, i32 3
  store i1 false, i1* %12, align 1
  br label %9

13:                                               ; preds = %7
  %14 = call i1 @"==(str, str)"(i8* %4, i8* getelementptr inbounds ([8 x i8], [8 x i8]* @9, i32 0, i32 0))
  br i1 %14, label %15, label %18

15:                                               ; preds = %13
  %16 = load { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %0, align 8
  %17 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %0, i32 0, i32 3
  store i1 true, i1* %17, align 1
  br label %9

18:                                               ; preds = %13
  %19 = call i1 @"==(str, str)"(i8* %4, i8* getelementptr inbounds ([5 x i8], [5 x i8]* @10, i32 0, i32 0))
  br i1 %19, label %20, label %22

20:                                               ; preds = %18
  %21 = load { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %0, align 8
  call void @"CommandLine.echo()"({ i8*, i8**, i32, i1 } %21)
  br label %9

22:                                               ; preds = %18
  %23 = call i1 @"==(str, str)"(i8* %4, i8* getelementptr inbounds ([3 x i8], [3 x i8]* @11, i32 0, i32 0))
  br i1 %23, label %24, label %26

24:                                               ; preds = %22
  %25 = load { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %0, align 8
  call void @"CommandLine.ls()"({ i8*, i8**, i32, i1 } %25)
  br label %9

26:                                               ; preds = %22
  %27 = call i8* @string_concat(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @13, i32 0, i32 0), i8* %4)
  %28 = call i8* @string_concat(i8* %27, i8* getelementptr inbounds ([2 x i8], [2 x i8]* @14, i32 0, i32 0))
  call void @"trap(str)"(i8* %28)
  br label %9
}

define i1 @"==(str, str)"(i8* %0, i8* %1) {
  %left = alloca i8*, align 8
  store i8* %0, i8** %left, align 8
  %right = alloca i8*, align 8
  store i8* %1, i8** %right, align 8
  %3 = load i8*, i8** %left, align 8
  %4 = load i8*, i8** %right, align 8
  %5 = call i32 @string_compare(i8* %3, i8* %4)
  %6 = trunc i32 %5 to i1
  ret i1 %6
}

define void @"CommandLine.echo()"({ i8*, i8**, i32, i1 } %0) {
  %2 = alloca { i8*, i8**, i32, i1 }, align 8
  store { i8*, i8**, i32, i1 } %0, { i8*, i8**, i32, i1 }* %2, align 8
  %3 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %2, i32 0, i32 3
  %4 = load i1, i1* %3, align 1
  %5 = xor i1 %4, true
  br i1 %5, label %6, label %7

6:                                                ; preds = %1
  ret void

7:                                                ; preds = %1
  %i = alloca i32, align 4
  store i32 0, i32* %i, align 4
  br label %8

8:                                                ; preds = %14, %7
  %9 = load i32, i32* %i, align 4
  %10 = alloca { i8*, i8**, i32, i1 }, align 8
  store { i8*, i8**, i32, i1 } %0, { i8*, i8**, i32, i1 }* %10, align 8
  %11 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %10, i32 0, i32 2
  %12 = load i32, i32* %11, align 4
  %13 = icmp slt i32 %9, %12
  br i1 %13, label %14, label %23

14:                                               ; preds = %8
  %15 = alloca { i8*, i8**, i32, i1 }, align 8
  store { i8*, i8**, i32, i1 } %0, { i8*, i8**, i32, i1 }* %15, align 8
  %16 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %15, i32 0, i32 1
  %17 = load i8**, i8*** %16, align 8
  %18 = load i32, i32* %i, align 4
  %19 = getelementptr i8*, i8** %17, i32 %18
  %20 = load i8*, i8** %19, align 8
  call void @print_string(i8* %20)
  call void @print_char(i8 32)
  %21 = load i32, i32* %i, align 4
  %22 = add i32 %21, 1
  store i32 %22, i32* %i, align 4
  br label %8

23:                                               ; preds = %8
  call void @print_char(i8 10)
  ret void
}

define void @"CommandLine.ls()"({ i8*, i8**, i32, i1 } %0) {
  %dir = alloca i8*, align 8
  store i8* getelementptr inbounds ([2 x i8], [2 x i8]* @12, i32 0, i32 0), i8** %dir, align 8
  %2 = alloca { i8*, i8**, i32, i1 }, align 8
  store { i8*, i8**, i32, i1 } %0, { i8*, i8**, i32, i1 }* %2, align 8
  %3 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %2, i32 0, i32 2
  %4 = load i32, i32* %3, align 4
  %5 = icmp sgt i32 %4, 0
  br i1 %5, label %6, label %12

6:                                                ; preds = %1
  %7 = alloca { i8*, i8**, i32, i1 }, align 8
  store { i8*, i8**, i32, i1 } %0, { i8*, i8**, i32, i1 }* %7, align 8
  %8 = getelementptr { i8*, i8**, i32, i1 }, { i8*, i8**, i32, i1 }* %7, i32 0, i32 1
  %9 = load i8**, i8*** %8, align 8
  %10 = getelementptr i8*, i8** %9, i32 0
  %11 = load i8*, i8** %10, align 8
  store i8* %11, i8** %dir, align 8
  br label %12

12:                                               ; preds = %6, %1
  ret void
}

define void @"trap(str)"(i8* %0) {
  %error = alloca i8*, align 8
  store i8* %0, i8** %error, align 8
  %2 = load i8*, i8** %error, align 8
  %3 = call i8* @string_concat(i8* getelementptr inbounds ([8 x i8], [8 x i8]* @15, i32 0, i32 0), i8* %2)
  call void @println_string(i8* %3)
  ret void
}

attributes #0 = { nounwind uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { inlinehint nobuiltin nounwind uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #2 = { argmemonly nounwind willreturn }
attributes #3 = { nounwind }
attributes #4 = { "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #5 = { noinline nounwind uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #6 = { nofree nounwind "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #7 = { nounwind "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #8 = { nofree nounwind uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #9 = { argmemonly nofree nounwind readonly "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #10 = { nounwind readonly uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #11 = { nofree nounwind readonly "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #12 = { nofree norecurse nounwind uwtable writeonly "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #13 = { nofree "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }

!llvm.ident = !{!0, !0, !0}
!llvm.module.flags = !{!1, !2}

!0 = !{!"clang version 11.0.0"}
!1 = !{i32 1, !"wchar_size", i32 2}
!2 = !{i32 7, !"PIC Level", i32 2}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C/C++ TBAA"}
!7 = !{!8, !8, i64 0}
!8 = !{!"long long", !5, i64 0}
!9 = !{!5, !5, i64 0}
