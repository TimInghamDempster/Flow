#include "RuntimeAPI.h"

#include <windows.h>

typedef int(__stdcall* f_funci)();

int Evaluate()
{
    HINSTANCE hGetProcIDDLL = LoadLibrary(L"Content\\test.dll");

    if (!hGetProcIDDLL) {
        return -1;
    }

    f_funci funci = (f_funci)GetProcAddress(hGetProcIDDLL, "test_func");
    if (!funci) {
        return EXIT_FAILURE;
    }

    auto val = funci();

    FreeLibrary(hGetProcIDDLL);

    return val;
}