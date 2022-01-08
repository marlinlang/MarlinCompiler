; ModuleID = 'Program'
source_filename = "Program"

%"std::Void" = type {}
%"std::Character" = type { i32 }
%"std::Integer" = type { i32 }

declare i32 @getchar()

declare i32 @putchar(i32)

define %"std::Void"* @"std::ConsoleInterfaces.PutCharacter"() {
entry:
  %0 = alloca %"std::Character"
  %1 = getelementptr inbounds %"std::Character", %"std::Character"* %0, i32 0, i32 0
  store i32 97, i32* %1
  %2 = getelementptr inbounds %"std::Character", %"std::Character"* %0, i32 0, i32 0
  %3 = load i32, i32* %2
  %4 = call i32 @putchar(i32 %3)
  ret %"std::Void"* null
}

define %"std::Character"* @"std::ConsoleInterfaces.GetCharacter"() {
entry:
  %0 = call i32 @getchar()
  %1 = alloca %"std::Character"
  %2 = getelementptr inbounds %"std::Character", %"std::Character"* %1, i32 0, i32 0
  store i32 %0, i32* %2
  ret %"std::Character"* %1
  ret %"std::Character"* null
}

define %"std::Void"* @"app::Program.Main"() {
entry:
  %x = alloca %"std::Integer"
  %0 = alloca %"std::Integer"
  %1 = getelementptr inbounds %"std::Integer", %"std::Integer"* %0, i32 0, i32 0
  store i32 0, i32* %1
  %2 = load %"std::Integer", %"std::Integer"* %0
  store %"std::Integer" %2, %"std::Integer"* %x
  ret %"std::Void"* null
}

define void @"std::Boolean.ctor"() {
entry:
  ret void
}

define void @"std::Character.ctor"() {
entry:
  ret void
}

define void @"std::Console.ctor"() {
entry:
  ret void
}

define void @"std::Integer.ctor"() {
entry:
  ret void
}

define void @"std::Void.ctor"() {
entry:
  ret void
}
