
/*
*
*	SYNAPSE X
*	File.:	Iterator.hpp
*	Desc.:	Class for iterating over memory
*
*/

#pragma once

#include "../Exploit/Misc/Static.hpp"

namespace syn
{
	class MemoryScanner
	{
	public:
		static bool Compare(const char* location, const char* aob, const char* mask)
		{
			for (; *mask; ++aob, ++mask, ++location)
			{
				if (*mask == 'x' && *location != *aob)
				{
					return false;
				}
			}

			return true;
		}

		static bool CompareReverse(const char* location, const char* aob, const char* mask)
		{
			auto MaskItr = mask + strlen(mask) - 1;
			for (; MaskItr >= mask; --aob, --MaskItr, --location)
			{
				if (*MaskItr == 'x' && *location != *aob)
				{
					return false;
				}
			}

			return true;
		}

		static BYTE* Scan(const char* aob, const char* mask, DWORD start, DWORD end)
		{
			if (start <= end)
			{
				for (; start <= end; ++start)
				{
					if (Compare((char*) start, (char*) aob, mask))
					{
						return (BYTE*) start;
					}
				}
			}
			else
			{
                size_t maskLen = strlen(mask);

				for (; start >= end; --start)
				{
					if (CompareReverse((char*) start, (char*) aob, mask))
					{
						return (BYTE*) start - maskLen - 1;
					}
				}
			}

			return nullptr;
		};

		static BYTE* Scan(const char* module, const char* aob, const char* mask)
		{
			MODULEINFO Info;
			if (GetModuleInformation(syn::RobloxProcess, GetModuleHandle(module), &Info, sizeof Info))
				return Scan(aob, mask, (DWORD) Info.lpBaseOfDll, (DWORD) Info.lpBaseOfDll + Info.SizeOfImage);

			return nullptr;
		}

		static void* VfScan(DWORD Vftable, size_t Size)
		{
			void* Return = NULL;

			const auto Heap = GetProcessHeap();

			PROCESS_HEAP_ENTRY HeapEntry;

			ZeroMemory(&HeapEntry, sizeof HeapEntry);
			HeapLock(Heap);

			while (HeapWalk(Heap, &HeapEntry))
			{
				if (HeapEntry.wFlags & PROCESS_HEAP_ENTRY_BUSY)
				{
					if (HeapEntry.cbData < Size)
						continue;

					const auto Data = (DWORD) HeapEntry.lpData + 4;

					if (*(DWORD*)Data == Vftable)
					{
						Return = (void*)Data;
						break;
					}
				}
			}

			HeapUnlock(Heap);

			return Return;
		}
	};
}
