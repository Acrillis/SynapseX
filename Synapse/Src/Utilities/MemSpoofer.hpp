#pragma once
#include <functional>

#include "../Exploit/Misc/Static.hpp"
#include "Obfuscation/ObfuscatedNumber.hpp"

namespace syn
{
	class MemSpoofer
	{
	public:
		__forceinline static std::function<void()> Spoof()
		{
			auto Idx = 0;
			uint64_t* ScriptMemPtr = nullptr;

            VM_TIGER_WHITE_START;

			do
			{
				/* 5 is Enum.Stats.DeveloperMemoryTag.Script. */
				if (((int(__cdecl*)(int))syn::RobloxBase(OBFUSCATED_NUM(syn::Offsets::MemUsage::GetIdx)))(Idx) == 5)
				{
					ScriptMemPtr = (uint64_t*) (syn::RobloxBase(OBFUSCATED_NUM(syn::Offsets::MemUsage::UsageTable)) + 104 * Idx);
					break;
				}

				Idx++;
			} while (Idx < ((int(__cdecl*)())syn::RobloxBase(OBFUSCATED_NUM(syn::Offsets::MemUsage::GetUsageMax)))());

			if (ScriptMemPtr == nullptr)
			{
				syn::Profiler::GetSingleton()->AddProfile(OBFUSCATE_STR("ScriptMemPtr is null for MemSpoofer."));

				return []() {};
			}

			/* Create a backup of it. */
			auto Backup = *ScriptMemPtr;

            VM_TIGER_WHITE_END;

			return [ScriptMemPtr, Backup]()
			{
				/* Then set it back after conversion is complete. */
				*ScriptMemPtr = Backup;
			};
		}
	};
}
