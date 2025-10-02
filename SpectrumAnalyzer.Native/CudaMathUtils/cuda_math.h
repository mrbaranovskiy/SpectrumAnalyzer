#pragma once

// api.hpp
#pragma once
#ifdef _WIN32
#define API __declspec(dllexport)
#else
#define API __attribute__((visibility("default")))
#endif

#ifdef _WIN32
  #ifdef BUILDING_CUDAFFT
    #define CUDAFFT_API __declspec(dllexport)
  #else
    #define CUDAFFT_API __declspec(dllimport)
  #endif
#else
  #define CUDAFFT_API __attribute__((visibility("default")))
#endif


extern "C" {
    API void saxpy(float a, const float* x_h, const float* y_h, float* out_h, int n);
    // in/out: interleaved [re0, im0, re1, im1, ...], length = 2*N floats
    int iq_fft_c2c_forward(const float* in_host, float* out_host, int n);
    int iq_fft_c2c_forward2(const float* in_host, float* out_host, int n);
    // power spectrum (|X[k]|^2) from interleaved complex input
    int iq_power_spectrum(const float* in_host, float* out_host, int n);
    // center DC (fftshift) for interleaved complex buffer in place
    int iq_fftshift_inplace(float* io_host, int n);
    // power-in-dB with clamp to avoid log(0)
    int iq_power_db(const float* in_host, float* out_host, int n, float floor_db);
    // scale to frequncy (this is real)
    int k_scale_r(float* out_host, float N, float Fs);
}