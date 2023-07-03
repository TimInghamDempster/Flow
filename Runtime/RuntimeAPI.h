#pragma once

#ifdef RUNTIME_API_EXPORTS
#define RUNTIME_API __declspec(dllexport)
#else
#define RUNTIME_API __declspec(dllimport)
#endif

extern "C" RUNTIME_API void Evaluate(int* res);