#include <cuda_runtime.h>
#include <cufft.h>
#include <cstdio>
//chat gpt generated shit...

inline const char* cufft_err(cufftResult r) {
    switch (r) {
    case CUFFT_SUCCESS: return "CUFFT_SUCCESS";
    case CUFFT_INVALID_PLAN: return "CUFFT_INVALID_PLAN";
    case CUFFT_ALLOC_FAILED: return "CUFFT_ALLOC_FAILED";
    case CUFFT_INVALID_TYPE: return "CUFFT_INVALID_TYPE";
    case CUFFT_INVALID_VALUE: return "CUFFT_INVALID_VALUE";
    case CUFFT_INTERNAL_ERROR: return "CUFFT_INTERNAL_ERROR";
    case CUFFT_EXEC_FAILED: return "CUFFT_EXEC_FAILED";
    case CUFFT_SETUP_FAILED: return "CUFFT_SETUP_FAILED";
    case CUFFT_INVALID_SIZE: return "CUFFT_INVALID_SIZE";
    case CUFFT_UNALIGNED_DATA: return "CUFFT_UNALIGNED_DATA";
    default: return "CUFFT_UNKNOWN_ERROR";
    }
}

inline int checkCuda(cudaError_t st, const char* where) {
    if (st != cudaSuccess) {
        std::fprintf(stderr, "[CUDA] %s: %s\n", where, cudaGetErrorString(st));
        return 1;
    }
    return 0;
}

// i saw this in the book...
inline int checkCufft(cufftResult st, const char* where) {
    if (st != CUFFT_SUCCESS) {
        std::fprintf(stderr, "[cuFFT] %s: %s\n", where, cufft_err(st));
        return 2;
    }
    return 0;
}
