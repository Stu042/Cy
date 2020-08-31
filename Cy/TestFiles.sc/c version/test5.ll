; ModuleID = 'test4.cpp'
source_filename = "test4.cpp"
target datalayout = "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-pc-windows-msvc19.26.28806"

%class.Data = type { i32, float }

; Function Attrs: noinline norecurse optnone uwtable
define dso_local i32 @main() #0 {
  %1 = alloca i32, align 4
  %2 = alloca %class.Data*, align 8
  %3 = alloca i32, align 4
  store i32 0, i32* %1, align 4
  %4 = call i8* @"??2@YAPEAX_K@Z"(i64 8) #4
  %5 = bitcast i8* %4 to %class.Data*
  %6 = bitcast %class.Data* %5 to i8*
  call void @llvm.memset.p0i8.i64(i8* align 8 %6, i8 0, i64 8, i1 false)
  store %class.Data* %5, %class.Data** %2, align 8
  %7 = load %class.Data*, %class.Data** %2, align 8
  %8 = getelementptr inbounds %class.Data, %class.Data* %7, i32 0, i32 0
  store i32 5, i32* %8, align 4
  %9 = load %class.Data*, %class.Data** %2, align 8
  %10 = getelementptr inbounds %class.Data, %class.Data* %9, i32 0, i32 1
  store float 0x3FC99999A0000000, float* %10, align 4
  %11 = load %class.Data*, %class.Data** %2, align 8
  %12 = getelementptr inbounds %class.Data, %class.Data* %11, i32 0, i32 0
  %13 = load i32, i32* %12, align 4
  %14 = sitofp i32 %13 to float
  %15 = load %class.Data*, %class.Data** %2, align 8
  %16 = getelementptr inbounds %class.Data, %class.Data* %15, i32 0, i32 1
  %17 = load float, float* %16, align 4
  %18 = fmul float %14, %17
  %19 = fptosi float %18 to i32
  store i32 %19, i32* %3, align 4
  %20 = load %class.Data*, %class.Data** %2, align 8
  %21 = icmp eq %class.Data* %20, null
  br i1 %21, label %24, label %22

22:                                               ; preds = %0
  %23 = bitcast %class.Data* %20 to i8*
  call void @"??3@YAXPEAX@Z"(i8* %23) #5
  br label %24

24:                                               ; preds = %22, %0
  %25 = load i32, i32* %3, align 4
  ret i32 %25
}

; Function Attrs: nobuiltin
declare dso_local noalias i8* @"??2@YAPEAX_K@Z"(i64) #1

; Function Attrs: argmemonly nounwind willreturn
declare void @llvm.memset.p0i8.i64(i8* nocapture writeonly, i8, i64, i1 immarg) #2

; Function Attrs: nobuiltin nounwind
declare dso_local void @"??3@YAXPEAX@Z"(i8*) #3

attributes #0 = { noinline norecurse optnone uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { nobuiltin "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #2 = { argmemonly nounwind willreturn }
attributes #3 = { nobuiltin nounwind "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #4 = { builtin }
attributes #5 = { builtin nounwind }

!llvm.module.flags = !{!0, !1}
!llvm.ident = !{!2}

!0 = !{i32 1, !"wchar_size", i32 2}
!1 = !{i32 7, !"PIC Level", i32 2}
!2 = !{!"clang version 10.0.0 "}
