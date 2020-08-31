; ModuleID = 'test5.cpp'
source_filename = "test5.cpp"
target datalayout = "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-pc-windows-msvc19.26.28806"

%class.Data = type { i32, float }

$"??0Data@@QEAA@HM@Z" = comdat any

; Function Attrs: noinline norecurse optnone uwtable
define dso_local i32 @main() #0 personality i8* bitcast (i32 (...)* @__CxxFrameHandler3 to i8*) {
  %1 = alloca i32, align 4
  %2 = alloca %class.Data*, align 8
  %3 = alloca i32, align 4
  store i32 0, i32* %1, align 4
  %4 = call i8* @"??2@YAPEAX_K@Z"(i64 8) #4
  %5 = bitcast i8* %4 to %class.Data*
  %6 = invoke %class.Data* @"??0Data@@QEAA@HM@Z"(%class.Data* %5, i32 10, float 2.000000e+00)
          to label %7 unwind label %23

7:                                                ; preds = %0
  store %class.Data* %5, %class.Data** %2, align 8
  %8 = load %class.Data*, %class.Data** %2, align 8
  %9 = getelementptr inbounds %class.Data, %class.Data* %8, i32 0, i32 0
  %10 = load i32, i32* %9, align 4
  %11 = sitofp i32 %10 to float
  %12 = load %class.Data*, %class.Data** %2, align 8
  %13 = getelementptr inbounds %class.Data, %class.Data* %12, i32 0, i32 1
  %14 = load float, float* %13, align 4
  %15 = fmul float %11, %14
  %16 = fptosi float %15 to i32
  store i32 %16, i32* %3, align 4
  %17 = load %class.Data*, %class.Data** %2, align 8
  %18 = icmp eq %class.Data* %17, null
  br i1 %18, label %21, label %19

19:                                               ; preds = %7
  %20 = bitcast %class.Data* %17 to i8*
  call void @"??3@YAXPEAX@Z"(i8* %20) #5
  br label %21

21:                                               ; preds = %19, %7
  %22 = load i32, i32* %3, align 4
  ret i32 %22

23:                                               ; preds = %0
  %24 = cleanuppad within none []
  call void @"??3@YAXPEAX@Z"(i8* %4) #5 [ "funclet"(token %24) ]
  cleanupret from %24 unwind to caller
}

; Function Attrs: nobuiltin
declare dso_local noalias i8* @"??2@YAPEAX_K@Z"(i64) #1

; Function Attrs: noinline nounwind optnone uwtable
define linkonce_odr dso_local %class.Data* @"??0Data@@QEAA@HM@Z"(%class.Data* returned %0, i32 %1, float %2) unnamed_addr #2 comdat align 2 {
  %4 = alloca float, align 4
  %5 = alloca i32, align 4
  %6 = alloca %class.Data*, align 8
  store float %2, float* %4, align 4
  store i32 %1, i32* %5, align 4
  store %class.Data* %0, %class.Data** %6, align 8
  %7 = load %class.Data*, %class.Data** %6, align 8
  %8 = load i32, i32* %5, align 4
  %9 = getelementptr inbounds %class.Data, %class.Data* %7, i32 0, i32 0
  store i32 %8, i32* %9, align 4
  %10 = load float, float* %4, align 4
  %11 = getelementptr inbounds %class.Data, %class.Data* %7, i32 0, i32 1
  store float %10, float* %11, align 4
  ret %class.Data* %7
}

declare dso_local i32 @__CxxFrameHandler3(...)

; Function Attrs: nobuiltin nounwind
declare dso_local void @"??3@YAXPEAX@Z"(i8*) #3

attributes #0 = { noinline norecurse optnone uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { nobuiltin "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #2 = { noinline nounwind optnone uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #3 = { nobuiltin nounwind "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #4 = { builtin }
attributes #5 = { builtin nounwind }

!llvm.module.flags = !{!0, !1}
!llvm.ident = !{!2}

!0 = !{i32 1, !"wchar_size", i32 2}
!1 = !{i32 7, !"PIC Level", i32 2}
!2 = !{!"clang version 10.0.0 "}
