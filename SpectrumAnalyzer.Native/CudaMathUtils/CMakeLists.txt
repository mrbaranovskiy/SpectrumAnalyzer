
cmake_minimum_required(VERSION 3.18 FATAL_ERROR)  # 3.18+ recommended for CUDA language support

project(MyCudaApp LANGUAGES CXX CUDA)
find_package(CUDAToolkit REQUIRED)

set(cuLib "cuda_lib")

add_library(${cuLib} SHARED cuda_math.h cuda_math.cu cuda_math_api.cu)
target_link_libraries(${cuLib} PRIVATE CUDA::cufft CUDA::cudart )

set_target_properties(${cuLib} PROPERTIES
  CUDA_SEPARABLE_COMPILATION ON     # enables relocatable device code & device link
  POSITION_INDEPENDENT_CODE ON
  CXX_STANDARD 17
  CUDA_ARCHITECTURES 75       # pick what you need
)

set_target_properties(${cuLib} PROPERTIES CUDA_RUNTIME_LIBRARY Shared)
# Optional: set GPU architectures (compute capabilities)
# Example: build for sm_60 (Pascal) and sm_75 (Turing)
if(CMAKE_CUDA_ARCHITECTURES LESS 75)
  set(CMAKE_CUDA_ARCHITECTURES 75 80 86 90 100 120)
endif()



