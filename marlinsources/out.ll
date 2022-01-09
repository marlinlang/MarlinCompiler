; ModuleID = 'Program'
source_filename = "Program"

%"std::Void" = type {}
%"std::Character" = type { i32 }
%"std::Integer" = type { i32 }
%"std::String" = type { %"std::Character"* }
%"std::Boolean" = type { i1 }

declare i32 @malloc(i32)

declare i32 @getchar()

declare i32 @putchar(i32)

define %"std::Void"* @"std::ConsoleInterfaces.PutCharacter"() {
entry:
  %boxPtr = alloca %"std::Character"
  %0 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr, i32 0, i32 0
  store i32 97, i32* %0
  %1 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr, i32 0, i32 0
  %2 = load i32, i32* %1
  %3 = call i32 @putchar(i32 %2)
  %boxPtr1 = alloca %"std::Void"
  ret %"std::Void"* %boxPtr1
}

define %"std::Character"* @"std::ConsoleInterfaces.GetCharacter"() {
entry:
  %0 = call i32 @getchar()
  %boxPtr = alloca %"std::Character"
  %1 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr, i32 0, i32 0
  store i32 %0, i32* %1
  ret %"std::Character"* %boxPtr
}

define %"std::Void"* @"app::Program.Main"() {
entry:
  %boxPtr = alloca %"std::Integer"
  %0 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr, i32 0, i32 0
  store i32 12, i32* %0
  %1 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr, i32 0, i32 0
  %2 = load i32, i32* %1
  %3 = mul i32 ptrtoint (i32* getelementptr (i32, i32* null, i32 1) to i32), %2
  %4 = call i32 @malloc(i32 %3)
  %5 = inttoptr i32 %4 to %"std::Character"*
  %boxPtr1 = alloca %"std::Character"
  %6 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr1, i32 0, i32 0
  store i32 72, i32* %6
  %7 = load %"std::Character", %"std::Character"* %boxPtr1
  %boxPtr2 = alloca %"std::Integer"
  %8 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr2, i32 0, i32 0
  store i32 0, i32* %8
  %9 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr2, i32 0, i32 0
  %array_index = load i32, i32* %9
  %array_element_gep = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index
  store %"std::Character" %7, %"std::Character"* %array_element_gep
  %boxPtr3 = alloca %"std::Character"
  %10 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr3, i32 0, i32 0
  store i32 101, i32* %10
  %11 = load %"std::Character", %"std::Character"* %boxPtr3
  %boxPtr4 = alloca %"std::Integer"
  %12 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr4, i32 0, i32 0
  store i32 1, i32* %12
  %13 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr4, i32 0, i32 0
  %array_index5 = load i32, i32* %13
  %array_element_gep6 = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index5
  store %"std::Character" %11, %"std::Character"* %array_element_gep6
  %boxPtr7 = alloca %"std::Character"
  %14 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr7, i32 0, i32 0
  store i32 108, i32* %14
  %15 = load %"std::Character", %"std::Character"* %boxPtr7
  %boxPtr8 = alloca %"std::Integer"
  %16 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr8, i32 0, i32 0
  store i32 2, i32* %16
  %17 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr8, i32 0, i32 0
  %array_index9 = load i32, i32* %17
  %array_element_gep10 = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index9
  store %"std::Character" %15, %"std::Character"* %array_element_gep10
  %boxPtr11 = alloca %"std::Character"
  %18 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr11, i32 0, i32 0
  store i32 108, i32* %18
  %19 = load %"std::Character", %"std::Character"* %boxPtr11
  %boxPtr12 = alloca %"std::Integer"
  %20 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr12, i32 0, i32 0
  store i32 3, i32* %20
  %21 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr12, i32 0, i32 0
  %array_index13 = load i32, i32* %21
  %array_element_gep14 = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index13
  store %"std::Character" %19, %"std::Character"* %array_element_gep14
  %boxPtr15 = alloca %"std::Character"
  %22 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr15, i32 0, i32 0
  store i32 111, i32* %22
  %23 = load %"std::Character", %"std::Character"* %boxPtr15
  %boxPtr16 = alloca %"std::Integer"
  %24 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr16, i32 0, i32 0
  store i32 4, i32* %24
  %25 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr16, i32 0, i32 0
  %array_index17 = load i32, i32* %25
  %array_element_gep18 = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index17
  store %"std::Character" %23, %"std::Character"* %array_element_gep18
  %boxPtr19 = alloca %"std::Character"
  %26 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr19, i32 0, i32 0
  store i32 32, i32* %26
  %27 = load %"std::Character", %"std::Character"* %boxPtr19
  %boxPtr20 = alloca %"std::Integer"
  %28 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr20, i32 0, i32 0
  store i32 5, i32* %28
  %29 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr20, i32 0, i32 0
  %array_index21 = load i32, i32* %29
  %array_element_gep22 = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index21
  store %"std::Character" %27, %"std::Character"* %array_element_gep22
  %boxPtr23 = alloca %"std::Character"
  %30 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr23, i32 0, i32 0
  store i32 119, i32* %30
  %31 = load %"std::Character", %"std::Character"* %boxPtr23
  %boxPtr24 = alloca %"std::Integer"
  %32 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr24, i32 0, i32 0
  store i32 6, i32* %32
  %33 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr24, i32 0, i32 0
  %array_index25 = load i32, i32* %33
  %array_element_gep26 = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index25
  store %"std::Character" %31, %"std::Character"* %array_element_gep26
  %boxPtr27 = alloca %"std::Character"
  %34 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr27, i32 0, i32 0
  store i32 111, i32* %34
  %35 = load %"std::Character", %"std::Character"* %boxPtr27
  %boxPtr28 = alloca %"std::Integer"
  %36 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr28, i32 0, i32 0
  store i32 7, i32* %36
  %37 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr28, i32 0, i32 0
  %array_index29 = load i32, i32* %37
  %array_element_gep30 = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index29
  store %"std::Character" %35, %"std::Character"* %array_element_gep30
  %boxPtr31 = alloca %"std::Character"
  %38 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr31, i32 0, i32 0
  store i32 114, i32* %38
  %39 = load %"std::Character", %"std::Character"* %boxPtr31
  %boxPtr32 = alloca %"std::Integer"
  %40 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr32, i32 0, i32 0
  store i32 8, i32* %40
  %41 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr32, i32 0, i32 0
  %array_index33 = load i32, i32* %41
  %array_element_gep34 = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index33
  store %"std::Character" %39, %"std::Character"* %array_element_gep34
  %boxPtr35 = alloca %"std::Character"
  %42 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr35, i32 0, i32 0
  store i32 108, i32* %42
  %43 = load %"std::Character", %"std::Character"* %boxPtr35
  %boxPtr36 = alloca %"std::Integer"
  %44 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr36, i32 0, i32 0
  store i32 9, i32* %44
  %45 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr36, i32 0, i32 0
  %array_index37 = load i32, i32* %45
  %array_element_gep38 = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index37
  store %"std::Character" %43, %"std::Character"* %array_element_gep38
  %boxPtr39 = alloca %"std::Character"
  %46 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr39, i32 0, i32 0
  store i32 100, i32* %46
  %47 = load %"std::Character", %"std::Character"* %boxPtr39
  %boxPtr40 = alloca %"std::Integer"
  %48 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr40, i32 0, i32 0
  store i32 10, i32* %48
  %49 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr40, i32 0, i32 0
  %array_index41 = load i32, i32* %49
  %array_element_gep42 = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index41
  store %"std::Character" %47, %"std::Character"* %array_element_gep42
  %boxPtr43 = alloca %"std::Character"
  %50 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr43, i32 0, i32 0
  store i32 33, i32* %50
  %51 = load %"std::Character", %"std::Character"* %boxPtr43
  %boxPtr44 = alloca %"std::Integer"
  %52 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr44, i32 0, i32 0
  store i32 11, i32* %52
  %53 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr44, i32 0, i32 0
  %array_index45 = load i32, i32* %53
  %array_element_gep46 = getelementptr inbounds %"std::Character", %"std::Character"* %5, i32 %array_index45
  store %"std::Character" %51, %"std::Character"* %array_element_gep46
  %x = alloca %"std::String"
  %boxPtr47 = alloca %"std::String"
  %54 = getelementptr inbounds %"std::String", %"std::String"* %boxPtr47, i32 0, i32 0
  store %"std::Character"* %5, %"std::Character"** %54
  %55 = load %"std::String", %"std::String"* %boxPtr47
  store %"std::String" %55, %"std::String"* %x
  %E = getelementptr inbounds %"std::String", %"std::Character"* %5, i32 0, i32 0
  %56 = load %"std::Character"*, %"std::Character"** %E
  %57 = getelementptr inbounds %"std::Character", %"std::Character"* %56, i32 0
  %58 = getelementptr inbounds %"std::Character", %"std::Character"* %57, i32 0, i32 0
  %59 = load i32, i32* %58
  %60 = call i32 @putchar(i32 %59)
  %boxPtr48 = alloca %"std::Void"
  ret %"std::Void"* %boxPtr48
}

define void @"std::Boolean.ctor"(%"std::Boolean"* %0) {
entry:
  %1 = getelementptr inbounds %"std::Boolean", %"std::Boolean"* %0, i32 0, i32 0
  store i1 false, i1* %1
  ret void
}

define void @"std::Character.ctor"(%"std::Character"* %0) {
entry:
  %1 = getelementptr inbounds %"std::Character", %"std::Character"* %0, i32 0, i32 0
  store i32 0, i32* %1
  ret void
}

define void @"std::Integer.ctor"(%"std::Integer"* %0) {
entry:
  %1 = getelementptr inbounds %"std::Integer", %"std::Integer"* %0, i32 0, i32 0
  store i32 0, i32* %1
  ret void
}

define void @"std::String.ctor"(%"std::String"* %0) {
entry:
  %1 = getelementptr inbounds %"std::String", %"std::String"* %0, i32 0, i32 0
  %boxPtr = alloca %"std::Integer"
  %2 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr, i32 0, i32 0
  store i32 0, i32* %2
  %3 = getelementptr inbounds %"std::Integer", %"std::Integer"* %boxPtr, i32 0, i32 0
  %4 = load i32, i32* %3
  %5 = mul i32 ptrtoint (i32* getelementptr (i32, i32* null, i32 1) to i32), %4
  %6 = call i32 @malloc(i32 %5)
  %7 = inttoptr i32 %6 to %"std::Character"*
  store %"std::Character"* null, %"std::Character"** %1
  ret void
}

define void @"std::Void.ctor"(%"std::Void"* %0) {
entry:
  ret void
}

define i32 @main() {
entry:
  %0 = call %"std::Void"* @"app::Program.Main"()
  ret i32 90
}
