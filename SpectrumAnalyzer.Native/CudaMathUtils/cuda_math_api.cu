// api.cpp (host code)
#include "cuda_math.h"
#include <cuda_runtime.h>
#include <cufft.h>
#include <cstdio>

//chat gpt generated shit... need to test...

__global__ void saxpy_kernel(float a, const float* x, const float* y, float* out, int n);

__global__ void k_fftshift_interleaved(float* x, int n);
__global__ void k_power(const float* __restrict__ x, float* __restrict__ p, int n);
__global__ void k_power_db(const float* __restrict__ x, float* __restrict__ p_db, int n, float floor_db);
__global__ void k_fftshift_interleaved(float* x, int n);
__global__ void k_scale(float* __restrict__ freqs, int N, float Fs);
__global__ void k_power_db_real(const float* __restrict__ x,
                                float* __restrict__ p_db,
                                int n,
                                float floor_db);

static inline int checkCuda(cudaError_t st, const char* where)
{
    if (st != cudaSuccess)
    {
        std::fprintf(stderr, "[CUDA]%s: %s\n", where, cudaGetErrorString(st));
        return 1;
    }
    return 0;
}

static inline int checkCufft(cufftResult st, const char* where)
{
    if (st != CUFFT_SUCCESS)
    {
        std::fprintf(stderr, "[cuFFT]%s: %d\n", where, st);
        return 2;
    }
    return 0;
}

void saxpy(float a, const float* x_h, const float* y_h, float* out_h, int n)
{
    float *x_d, *y_d, *out_d;
    size_t bytes = n * sizeof(float);
    cudaMalloc(&x_d, bytes);
    cudaMalloc(&y_d, bytes);
    cudaMalloc(&out_d, bytes);
    cudaMemcpy(x_d, x_h, bytes, cudaMemcpyHostToDevice);
    cudaMemcpy(y_d, y_h, bytes, cudaMemcpyHostToDevice);

    dim3 blk(256), grd((n + blk.x - 1) / blk.x);
    saxpy_kernel<<<grd, blk>>>(a, x_d, y_d, out_d, n);
    cudaDeviceSynchronize();

    cudaMemcpy(out_h, out_d, bytes, cudaMemcpyDeviceToHost);
    cudaFree(x_d);
    cudaFree(y_d);
    cudaFree(out_d);
}

int iq_fft_c2c_forward(const float* in_host, float* out_host, int n)
{
    if (!in_host || !out_host || n <= 0) return 3;
    size_t bytes = sizeof(float) * 2 * n;
    cufftComplex *d_in = nullptr, *d_out = nullptr;
    if (int e = checkCuda(cudaMalloc(&d_in, bytes), "cudaMalloc d_in")) return e;
    if (int e = checkCuda(cudaMalloc(&d_out, bytes), "cudaMalloc d_out")) return e;
    if (int e = checkCuda(cudaMemcpy(d_in, in_host, bytes, cudaMemcpyHostToDevice), "H2D")) return e;

    cufftHandle plan;
    if (int e = checkCufft(cufftPlan1d(&plan, n, CUFFT_C2C, 1), "plan C2C")) return e;
    if (int e = checkCufft(cufftExecC2C(plan, d_in, d_out, CUFFT_FORWARD), "exec C2C FWD")) return e;
    if (int e = checkCuda(cudaDeviceSynchronize(), "sync")) return e;

    if (int e = checkCuda(cudaMemcpy(out_host, d_out, bytes, cudaMemcpyDeviceToHost), "D2H")) return e;
    cufftDestroy(plan);
    cudaFree(d_in);
    cudaFree(d_out);
    return 0;
}

int iq_fft_c2r_forward(const float* in_host, float* out_host, int n)
{
    if (!in_host || !out_host || n <= 0) return 3;

    const int n_complex = n / 2 + 1; // Hermitian packed length
    const size_t bytes_in = sizeof(float) * 2 * n_complex; // interleaved complex
    const size_t bytes_out = sizeof(float) * n; // real

    cufftComplex* d_in = nullptr;
    float* d_out = nullptr;

    if (int e = checkCuda(cudaMalloc(&d_in, bytes_in), "cudaMalloc d_in")) return e;
    if (int e = checkCuda(cudaMalloc(&d_out, bytes_out), "cudaMalloc d_out")) return e;
    if (int e = checkCuda(cudaMemcpy(d_in, in_host, bytes_in, cudaMemcpyHostToDevice), "H2D in")) return e;

    cufftHandle plan;
    if (int e = checkCufft(cufftPlan1d(&plan, n, CUFFT_C2R, 1), "cufftPlan1d C2R")) return e;

    if (int e = checkCufft(cufftExecC2R(plan, d_in, d_out), "cufftExecC2R")) return e;
    if (int e = checkCuda(cudaDeviceSynchronize(), "cudaDeviceSynchronize")) return e;

    // NOTE: cuFFT C2R is unnormalized. If you want unitary IFFT, divide by n.
    if (int e = checkCuda(cudaMemcpy(out_host, d_out, bytes_out, cudaMemcpyDeviceToHost), "D2H out")) return e;

    cufftDestroy(plan);
    cudaFree(d_in);
    cudaFree(d_out);
    return 0;
}

int iq_power_spectrum(const float* in_host, float* out_host, int n)
{
    if (!in_host || !out_host || n <= 0) return 3;
    size_t bytes = sizeof(float) * 2 * n;
    float* d_in = nullptr;
    float* d_pow = nullptr;

    if (int e = checkCuda(cudaMalloc(&d_in, bytes), "malloc d_in")) return e;
    if (int e = checkCuda(cudaMalloc(&d_pow, sizeof(float) * n), "malloc d_pow")) return e;
    if (int e = checkCuda(cudaMemcpy(d_in, in_host, bytes, cudaMemcpyHostToDevice), "H2D")) return e;


    dim3 blk(256), grd((n + 255) / 256);
    k_power<<<grd,blk>>>(d_in, d_pow, n);
    if (int e = checkCuda(cudaDeviceSynchronize(), "sync")) return e;

    if (int e = checkCuda(cudaMemcpy(out_host, d_pow, sizeof(float) * n, cudaMemcpyDeviceToHost), "D2H")) return e;
    cudaFree(d_in);
    cudaFree(d_pow);
    return 0;
}

int k_scale_r(float* out_host, float N, float Fs)
{
    float* d_freqs;
    cudaMalloc(&d_freqs, N * sizeof(float));

    dim3 block(256);
    dim3 grid((N + block.x - 1) / block.x);

    k_scale<<<grid, block>>>(d_freqs, N, Fs);

    if (int e = checkCuda(cudaDeviceSynchronize(), "sync")) return e;
    if (int e = checkCuda(cudaMemcpy(out_host, d_freqs, sizeof(float) * N, cudaMemcpyDeviceToHost), "D2H")) return e;
    cudaFree(d_freqs);
}


// power db for complex numbers
int iq_power_db(const float* in_host, float* out_host, int n, float floor_db)
{
    if (!in_host || !out_host || n <= 0) return 3;
    size_t bytes = sizeof(float) * 2 * n;
    float* d_in = nullptr;
    float* d_db = nullptr;
    if (int e = checkCuda(cudaMalloc(&d_in, bytes), "malloc d_in")) return e;
    if (int e = checkCuda(cudaMalloc(&d_db, sizeof(float) * n), "malloc d_db")) return e;
    if (int e = checkCuda(cudaMemcpy(d_in, in_host, bytes, cudaMemcpyHostToDevice), "H2D")) return e;

    dim3 blk(256), grd((n + 255) / 256);
    k_power_db<<<grd,blk>>>(d_in, d_db, n, floor_db);
    if (int e = checkCuda(cudaDeviceSynchronize(), "sync")) return e;

    if (int e = checkCuda(cudaMemcpy(out_host, d_db, sizeof(float) * n, cudaMemcpyDeviceToHost), "D2H")) return e;
    cudaFree(d_in);
    cudaFree(d_db);
    return 0;
}

//power db for real numbers
int iq_power_db_real(const float* in_host, float* out_host, int n, float floor_db)
{
    if (!in_host || !out_host || n <= 0)
        return 3;

    size_t bytes = sizeof(float) * n;
    float* d_in = nullptr;
    float* d_db = nullptr;
    if (int e = checkCuda(cudaMalloc(&d_in, bytes), "malloc d_in")) return e;
    if (int e = checkCuda(cudaMalloc(&d_db, bytes), "malloc d_db")) return e;
    if (int e = checkCuda(cudaMemcpy(d_in, in_host, bytes, cudaMemcpyHostToDevice), "H2D")) return e;

    dim3 blk(256), grd((n + 255) / 256);
    k_power_db_real<<<grd,blk>>>(d_in, d_db, n, floor_db);
    if (int e = checkCuda(cudaDeviceSynchronize(), "sync")) return e;

    if (int e = checkCuda(cudaMemcpy(out_host, d_db, sizeof(float) * n, cudaMemcpyDeviceToHost), "D2H")) return e;
    cudaFree(d_in);
    cudaFree(d_db);
    return 0;
}

int iq_fftshift_inplace(float* io_host, int n)
{
    if (!io_host || n <= 0) return 3;
    size_t bytes = sizeof(float) * 2 * n;
    float* d = nullptr;
    if (int e = checkCuda(cudaMalloc(&d, bytes), "malloc d")) return e;
    if (int e = checkCuda(cudaMemcpy(d, io_host, bytes, cudaMemcpyHostToDevice), "H2D")) return e;

    dim3 blk(256), grd(((n / 2) + 255) / 256);
    k_fftshift_interleaved<<<grd,blk>>>(d, n);
    if (int e = checkCuda(cudaDeviceSynchronize(), "sync")) return e;

    if (int e = checkCuda(cudaMemcpy(io_host, d, bytes, cudaMemcpyDeviceToHost), "D2H")) return e;
    cudaFree(d);
    return 0;
}
