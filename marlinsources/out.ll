; ModuleID = 'Program'
source_filename = "Program"

%"std::Void" = type {}
%"std::Character" = type { i32 }
%"std::String" = type { %"std::Character"* }
%"std::Boolean" = type { i1 }
%"std::Console" = type { void }
%"std::Integer" = type { i32 }

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
  %x = alloca %"std::String"
  %boxPtr = alloca %"std::Void"
  ret %"std::Void"* %boxPtr
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

define void @"std::Console.ctor"(%"std::Console"* %0) {
entry:
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
  %boxPtr = alloca %"std::Character"
  %2 = getelementptr inbounds %"std::Character", %"std::Character"* %boxPtr, i32 0, i32 0
  store i32 0, i32* %2
  store %"std::Character"* %boxPtr, %"std::Character"** %1
  ret void
}

define void @"std::Void.ctor"(%"std::Void"* %0) {
entry:
  ret void
}
