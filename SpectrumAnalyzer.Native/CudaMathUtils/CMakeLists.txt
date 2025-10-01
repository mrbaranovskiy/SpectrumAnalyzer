
cmake_minimum_required(VERSION 3.18)

set(PROJECT_NAME cuda_math)

project(${PROJECT_NAME} LANGUAGES CXX CUDA)

find_package(CUDAToolkit REQUIRED)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if("${CMAKE_BUILD_TYPE}" STREQUAL "")
  set(CMAKE_BUILD_TYPE Release)
endif()

if(CMAKE_CUDA_ARCHITECTURES LESS 75)
  set(CMAKE_CUDA_ARCHITECTURES 75 80 86 90 100 120)
endif()


set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set_source_files_properties(${PROJECT_NAME} PROPERTIES LANGUAGE CUDA)

add_executable(${PROJECT_NAME})

set_target_properties( ${PROJECT_NAME}
                       PROPERTIES CUDA_SEPARABLE_COMPILATION ON
                      )

target_include_directories(${PROJECT_NAME}
                           PRIVATE ${CMAKE_CUDA_TOOLKIT_INCLUDE_DIRECTORIES}
                           ${CMAKE_SOURCE_DIR}/utils)

target_sources(${PROJECT_NAME}
               PRIVATE ${PROJECT_SOURCE_DIR}/cuda_math_api.cu)

target_link_libraries(${PROJECT_NAME} PRIVATE CUDA::cufft CUDA::cudart)