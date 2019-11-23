/*
 * This is a second memory hasher (seperate from Synapse's) that aims to prevent users
 * and snoopy competitors from determining whether our bypass is enabled.
 */

#include "./FakeMemoryHasher.hpp"

#define XXH_STATIC_LINKING_ONLY
#define XXH_INLINE_ALL
#include "./Hashing/XXHash/xxhash.h"

#include "../Exploit/Security/MemCheck.hpp"
#include "../Exploit/Execution/Virtual Machine/VMBase.hpp"

#include <DbgHelp.h>

#include <themida/ThemidaSDK.h>
#include <themida/SecureEngineCustomVM_TIGER_LONDON.h>


namespace syn::FakeMemcheck
{
    enum LockContext
    {
        LOCK_INTERNAL_REHASH,
        LOCK_NEEDS_REHASH,
        LOCK_OPEN,
    };

    std::atomic<int> Lock = LOCK_OPEN;

    uintptr_t hook1, hook2; /* TODO: Figure out if this is the issue!!!! */

    uintptr_t text; size_t textSz;
    uintptr_t rdata; size_t rdataSz;

    uint32_t MasterHash = 0;

    __declspec(noinline) void GatherSegmentInfo()
    {
        VM_TIGER_LONDON_START;

        HMODULE vModule = syn::RobloxModule;
        PIMAGE_NT_HEADERS NTHeader = ImageNtHeader(vModule);
        PIMAGE_SECTION_HEADER section = reinterpret_cast<PIMAGE_SECTION_HEADER>(NTHeader + 1);

        for (int i = 0; i < NTHeader->FileHeader.NumberOfSections; i++, section++)
        {
            char* name = reinterpret_cast<char*>(section->Name);
            uintptr_t start = reinterpret_cast<uintptr_t>(vModule) + section->VirtualAddress;
            size_t size = section->Misc.VirtualSize;

            if (!strcmp(name, OBFUSCATE_STR(".text"))) {
                text = start;
                textSz = size;
            }
            else if (!strcmp(name, OBFUSCATE_STR(".rdata"))) {
                rdata = start;
                rdataSz = size;
            }
        }

        VM_TIGER_LONDON_END;
    }

    /* XX3 has an alignment issue, does not work at the moment */
    __declspec(noinline) uint32_t HashSegment(uintptr_t start, uintptr_t size)
    {
        XXH32_state_t* const state = XXH32_createState();
        if (!state)
            return 0;

        size_t position = start;
        size_t totalBlocks = size / 16; /* This should be fine for now */
        size_t remainingSize = size - (totalBlocks * 16);

        /* Use diffrence between .text and .rdata as seed */
        XXH_errorcode const resetResult = XXH32_reset(state, text - rdata);
        if (resetResult == XXH_ERROR)
            return 0;

        uintptr_t bockEnd = position + 16;
        while (totalBlocks--)
        {
            /* Temporary check for known hook locations */
            if (!(hook1 > position && hook1 < bockEnd) || !(hook2 > position && hook2 < bockEnd))
            {
                XXH32_update(state, (void*)position, 16);
            }

            position += 16;
        }

        /* Hash end of region if needed */
        if (remainingSize != 0)
            XXH32_update(state, (void*)position, remainingSize);

        XXH32_hash_t hash = XXH32_digest(state);

        XXH32_freeState(state);

        return hash;
    }

    FORCEINLINE uint32_t GetMixed()
    {
        VM_MUTATE_ONLY_START;

        uint32_t hash = HashSegment(text, textSz);
        hash ^= HashSegment(rdata, rdataSz);

        volatile int LockVar = Lock.load(std::memory_order_acquire);
        if (LockVar != LOCK_OPEN)
        {
            if (LockVar == LOCK_INTERNAL_REHASH)
                return hash;

            /* Wait for until we need to rehash, should always be on a seperate thread */
            while (Lock.load(std::memory_order_acquire) != LOCK_NEEDS_REHASH) _mm_pause();

            Lock.store(LOCK_OPEN, std::memory_order_release);

            return GetMixed();
        }

        VM_MUTATE_ONLY_END;

        return hash;
    }

    __declspec(noinline) void UpdateMasterHash()
    {
        Lock.store(LOCK_INTERNAL_REHASH, std::memory_order_release);

        MasterHash = GetMixed();

        Lock.store(LOCK_NEEDS_REHASH, std::memory_order_release);
    }

	__declspec(noinline) void Initialize()
    {
        GatherSegmentInfo();

        hook1 = syn::RobloxBase(VMLuaUCall);
        hook2 = syn::RobloxBase(VMLuaUOpCallCall); 

        /* Early attach, before real memcheck, cannot be locked */
        MasterHash = GetMixed();
    }

	__declspec(noinline) bool MemChanged()
    {
        uint32_t CurrentHash = GetMixed();

        MemoryChanged = MasterHash != CurrentHash;

        return MemoryChanged;
    }
}
