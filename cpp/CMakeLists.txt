cmake_minimum_required(VERSION 3.11)

project(
  ImageProcessing
  LANGUAGES CXX)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -pthread -fPIC")

# 라이브러리 파일은 빌드 디렉토리 안에 lib 폴더에 출력.
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)

# 실행 파일은 빌드 디렉토리 안에 bin 폴더에 출력.
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

file(GLOB SRC_FILES CONFIGURE_DEPENDS
  ${CMAKE_CURRENT_SOURCE_DIR}/*.cpp
  ${CMAKE_CURRENT_SOURCE_DIR}/*.hpp
)

if (APPLE AND ${CMAKE_SYSTEM_NAME} STREQUAL iOS)
    add_library(Improc STATIC ${SRC_FILES})
    target_include_directories(Improc PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/opencv2.framework)
else ()
    add_executable(Improc ${SRC_FILES})
    find_package(OpenCV 4.5.3 REQUIRED)
    target_link_libraries(Improc PUBLIC ${OpenCV_LIBS})
endif ()

target_compile_features(Improc PUBLIC cxx_std_17)
