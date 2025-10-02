// kernels.cu

#include <cuda_runtime.h>
#include <cufft.h>
#include <cstdio>

__global__ void saxpy_kernel(float a, const float* x, const float* y, float* out, int n){
    int i = blockIdx.x * blockDim.x + threadIdx.x;
    if (i < n) out[i] = a * x[i] + y[i];
}

#include <cufft.h>
#include <cstdio>

static inline int checkCuda(cudaError_t st, const char* where){
    if (st != cudaSuccess){ std::fprintf(stderr,"[CUDA]%s: %s\n",where,cudaGetErrorString(st)); return 1; }
    return 0;
}
static inline int checkCufft(cufftResult st, const char* where){
    if (st != CUFFT_SUCCESS){ std::fprintf(stderr,"[cuFFT]%s: %d\n",where,st); return 2; }
    return 0;
}

__global__ void k_power(const float* __restrict__ x, float* __restrict__ p, int n){
    int i = blockIdx.x * blockDim.x + threadIdx.x;   // i is complex index
    if (i >= n) return;
    float re = x[2*i+0];
    float im = x[2*i+1];
    p[i] = re*re + im*im; // power
}

__global__ void k_power_db(const float* __restrict__ x, float* __restrict__ p_db, int n, float floor_db){
    int i = blockIdx.x * blockDim.x + threadIdx.x;
    if (i >= n) return;
    float re = x[2*i+0];
    float im = x[2*i+1];
    float pw = re*re + im*im;
    // 10*log10(power). add tiny epsilon; clamp
    float db = 10.0f * log10f(fmaxf(pw, 1e-30f));
    p_db[i] = fmaxf(db, floor_db);
}

__global__ void k_scale(float* __restrict__ freqs, int N, float Fs)
{
    int k = blockIdx.x * blockDim.x + threadIdx.x;

    if (k < N) {
        freqs[k] = k * (Fs / N);
    }
}

// in-place fftshift for interleaved complex buffer
__global__ void k_fftshift_interleaved(float* x, int n){
    int i = blockIdx.x * blockDim.x + threadIdx.x; // complex index
    int half = n/2;
    if (i >= half) return;
    int j = i + half + (n & 1); // odd-N handling
    if (j >= n) j -= n;         // wrap
    // swap complex pairs
    float r0 = x[2*i], im0 = x[2*i+1];
    float r1 = x[2*j], im1 = x[2*j+1];
    x[2*i] = r1; x[2*i+1] = im1;
    x[2*j] = r0; x[2*j+1] = im0;
}
