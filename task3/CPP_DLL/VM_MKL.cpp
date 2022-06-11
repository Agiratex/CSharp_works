#include "pch.h"
#include "mkl.h"

extern "C"  _declspec(dllexport)
void call_vmsLn(unsigned int n, float* a, float* y, bool mode, int& ret)
{
	try
	{
		if (mode)
		{
			vmsLn(n, a, y, VML_HA);
		}
		else
		{
			vmsLn(n, a, y, VML_EP);
		}
		ret = 0;
	}
	catch (...)
	{
		ret = -1;
	}
}

extern "C"  _declspec(dllexport)
void call_vmdLn(unsigned int n, double* a, double* y, bool mode, int& ret)
{
	try
	{
		if (mode)
		{
			vmdLn(n, a, y, VML_HA);
		}
		else
		{
			vmdLn(n, a, y, VML_EP);
		}
		ret = 0;
	}
	catch (...)
	{
		ret = -1;
	}
}
