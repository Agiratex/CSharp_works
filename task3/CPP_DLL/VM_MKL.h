#pragma once
#include "mkl.h"

extern "C"  _declspec(dllexport)
void call_vmdLn(unsigned int n, double* a, double* y, bool mode, int& ret);

extern "C"  _declspec(dllexport)
void call_vmsLn(unsigned int n, float* a, float* y, bool mode, int& ret);
