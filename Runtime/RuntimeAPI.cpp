#include "RuntimeAPI.h"

#include <windows.h>

typedef int(__stdcall* f_funci)();

void Evaluate(int* res)
{
    HINSTANCE hGetProcIDDLL = LoadLibrary(L"Content\\flowprogram.dll");

    if (!hGetProcIDDLL) {
        *res = -1;
        return;
    }

    f_funci funci = (f_funci)GetProcAddress(hGetProcIDDLL, "test_func");
    if (!funci) {
        *res = -1;
        return;
    }

    auto val = funci();
    *res = val;

    FreeLibrary(hGetProcIDDLL);

    return;
}