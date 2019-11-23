#pragma once 

#include <cstdint>

template <uint32_t Hash> constexpr uint32_t fnv1a_assured() { return Hash; }
constexpr uint32_t fnv1a(const char* Str, const uint32_t Hash = 0x2288A23E)
{
    return Str[0] == '\0' ? Hash : fnv1a(&Str[1], (Hash ^ Str[0]) * 0x811c9dc5);
}

#define FNV1A(str) (fnv1a(str))
#define FNVA1_CONSTEXPR(str) (fnv1a_assured<fnv1a(str)>())