#pragma once

#include <cstdint>
#include "../Exploit/Misc/CallingConvention.hpp"

#undef CDECL

namespace syn
{
	extern std::uintptr_t RetAddr;
	static __forceinline __declspec(naked) void CallStub() 
	{
		__asm ret
	}

	#pragma warning(disable: 4035 4731 4191)
	template<typename ReturnType, typename... Arguments>
	ReturnType SpoofCall(CallingConvention callType, void *Addr, Arguments... arguments) noexcept
	{
        UNUSED(Addr); /* Will produce a warning when only used in inline asm */

		__asm {
			//Register saving.	
			push edi
			push esi
			mov esi, esp

			mov eax, fs:[0]
			mov eax, [eax]
			mov fs:[0], eax
		}

		if (callType == CallingConvention::CDECL || callType == CallingConvention::STDCALL)
			reinterpret_cast<void(__fastcall*)(Arguments...)>(&CallStub)(arguments...);
		else if (callType == CallingConvention::FASTCALL)
			reinterpret_cast<void(__stdcall*)(Arguments...)>(&CallStub)(arguments...);

		//Push the return address, and call our callee.
		__asm {
			mov edi, finish
			push RetAddr
			jmp Addr

			finish:
			mov esp, esi	
			pop edi
			pop esi

			mov esp, ebp
			pop ebp
			ret
		}
	}
    #pragma warning(default: 4035 4733 4731 4191)
}
