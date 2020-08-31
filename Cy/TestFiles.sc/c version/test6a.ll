; ModuleID = 'test5a.cpp'
source_filename = "test5a.cpp"
target datalayout = "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-pc-windows-msvc19.26.28806"

%class.Data = type { i32, float }

$"??0Data@@QEAA@HM@Z" = comdat any

; Function Attrs: noinline norecurse optnone uwtable
define dso_local i32 @main() #0 {
  %1 = alloca i32, align 4
  %2 = alloca %class.Data, align 4
  store i32 0, i32* %1, align 4
  %3 = call %class.Data* @"??0Data@@QEAA@HM@Z"(%class.Data* %2, i32 10, float 2.000000e+00)
  %4 = getelementptr inbounds %class.Data, %class.Data* %2, i32 0, i32 0
  %5 = load i32, i32* %4, align 4
  %6 = sitofp i32 %5 to float
  %7 = getelementptr inbounds %class.Data, %class.Data* %2, i32 0, i32 1
  %8 = load float, float* %7, align 4
  %9 = fmul float %6, %8
  %10 = fptosi float %9 to i32
  ret i32 %10
}

; Function Attrs: noinline nounwind optnone uwtable
define linkonce_odr dso_local %class.Data* @"??0Data@@QEAA@HM@Z"(%class.Data* returned %0, i32 %1, float %2) unnamed_addr #1 comdat align 2 {
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

attributes #0 = { noinline norecurse optnone uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { noinline nounwind optnone uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }

!llvm.module.flags = !{!0, !1}
!llvm.ident = !{!2}

!0 = !{i32 1, !"wchar_size", i32 2}
!1 = !{i32 7, !"PIC Level", i32 2}
!2 = !{!"clang version 10.0.0 "}
