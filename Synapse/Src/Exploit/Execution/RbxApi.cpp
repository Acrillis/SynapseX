#include "./RbxApi.hpp"

#include "../Misc/Profiler.hpp"
#include "../Misc/HttpStatus.hpp"

#include "./Conversion/ObfusDumper.hpp"
#include "./Conversion/RbxConversion.hpp"
#include "./Conversion/RbxLuauConversion.hpp"

#include "../../Utilities/MemSpoofer.hpp"
#include "../../Utilities/Hashing/fnv.hpp"
#include "../../Utilities/Hashing/sha512.h"
#include "../../Utilities/Obfuscation/ObfuscatedMember.hpp"

#include "../Misc/Resource.hpp"
#include "../Misc/PointerObfuscation.hpp"

#include "../Security/MemCheck.hpp"
#include "../Security/AntiProxy.hpp"

#include <Shlwapi.h>
#include <winhttp.h>

#include <cryptopp/blowfish.h>
#include <cryptopp/modes.h>

namespace syn 
{
	DWORD InitRL = 0;
	DWORD RobloxGlobalRL = 0;
	BOOL WSocketEnabled = 0;
	BOOL IngameChatEnabled = 0;
	BOOL WSocketCreated = 0;
	BOOL WSocketChatCreated = 0;
	DWORD LastDataModel = 0;
	DWORD DataModel = 0;
	HANDLE PipeHandle = 0;
	std::unordered_map<DWORD, DWORD> CClosureHandlers;
	syn::Obfuscation::Member::ObfuscatedMemberString* InitScript = nullptr;
    std::wstring WorkspaceDirectory;
	std::string HWID;
	std::string WSocketString;
	std::string WSocketChatString;
	DWORD ModuleSize;
	DWORD NtDllSize;
	bool SynAutoExec;
	bool Teleported;

	bool RbxApi::checkinstance(RbxLua RL, int Index)
    {
        RL.GetGlobal("typeof");
        RL.PushValue(Index);
        RL.PCall(1, 1, 0);
        const auto Check = RL.ToString(-1);
        RL.Pop(1);
        return !strcmp(Check, "Instance");
    }

    bool RbxApi::checksignal(RbxLua RL, int Index)
    {
        RL.GetGlobal("typeof");
        RL.PushValue(Index);
        RL.PCall(1, 1, 0);
        const auto Check = RL.ToString(-1);
        RL.Pop(1);
        return !strcmp(Check, "RBXScriptSignal");
    }

    bool RbxApi::checkcframe(RbxLua RL, int Index)
    {
        RL.GetGlobal("typeof");
        RL.PushValue(Index);
        RL.PCall(1, 1, 0);
        const auto Check = RL.ToString(-1);
        RL.Pop(1);
        return !strcmp(Check, "CFrame");
    }

	int RbxApi::loadstring(DWORD rL)
	{
		syn::RbxLua RL(rL);

        size_t ScriptLength;
        const char* Source = RL.CheckLString(1, &ScriptLength);
        std::string ChunkName = RL.OptString(2, ("@" + RandomString(16)).c_str());

		const auto MSpoofCallback = syn::MemSpoofer::Spoof();

		try
		{
			VM_TIGER_WHITE_START;

			auto Translator = syn::LuaTranslator::GetSingleton();
			Translator->ConvertInCurrentThread(RL, std::string(Source, ScriptLength), 0, &ChunkName);

			MSpoofCallback();

			VM_TIGER_WHITE_END;

			return 1;
		}
		catch (const std::exception& ex)
		{
			RL.PushNil();
			RL.PushString(ex.what());

			MSpoofCallback();

			return 2;
		}
	}

	int RbxApi::getrawmetatable(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.CheckAny(1);

        if (!RL.GetMetaTable(1))
            RL.PushNil();

		return 1;
	}

    int RbxApi::setrawmetatable(DWORD rL)
    {
        syn::RbxLua RL(rL);

        int t = RL.Type(2);
        RL.ArgCheck(t == R_LUA_TNIL || t == R_LUA_TTABLE, 2, "nil or table expected");

        RL.SetTop(2);
        RL.PushBoolean(RL.SetMetaTable(1));

        return 1;
    }

	int RbxApi::setreadonly(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.CheckType(1, R_LUA_TTABLE);
        RL.CheckType(2, R_LUA_TBOOLEAN);

		const auto Table = RL.ToPointer(1);
		*(BYTE*)(Table + RT_LOCKED) = RL.ToBoolean(2);

		return 0;
	}

	int RbxApi::make_writeable(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.CheckType(1, R_LUA_TTABLE);

		const auto Table = RL.ToPointer(1);
		*(BYTE*)(Table + RT_LOCKED) = FALSE;

		return 0;
	}

	int RbxApi::make_readonly(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.CheckType(1, R_LUA_TTABLE);

		const auto Table = RL.ToPointer(1);
		*(BYTE*)(Table + RT_LOCKED) = TRUE;

		return 0;
	}

	int RbxApi::isreadonly(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.CheckType(1, R_LUA_TTABLE);

		const auto Table = RL.ToPointer(1);
		RL.PushBoolean(*(BYTE*)(Table + RT_LOCKED));

		return 1;
	}

	int RbxApi::checkcaller(DWORD rL)
	{
		syn::RbxLua RL(rL);

		RBXExtraSpace* ExtraSpace = (RBXExtraSpace*)(RL - sizeof(RBXExtraSpace));
		RL.PushBoolean(!ExtraSpace->ScriptPtr);

		return 1;
	}

	int RbxApi::setidentity(DWORD rL)
	{
		syn::RbxLua RL(rL);
		BYTE NewIdentity = (BYTE)RL.CheckInteger(1);

		//Call constructor to immediately set identity as well.

		static DWORD ScriptImpPtr = 0;
		if (!ScriptImpPtr)
			ScriptImpPtr = syn::RobloxBase(syn::Lua::RbxImpersonatorConstruct);

		DWORD _this{};
		((void(__thiscall*)(DWORD*, DWORD))ScriptImpPtr)(&_this, NewIdentity);

		//Then call actual identity setting code so it preserves across yields.
		RL.SetIdentity((BYTE)RL.CheckInteger(1));

		return 0;
	}

	int RbxApi::getidentity(DWORD rL)
	{
		syn::RbxLua RL(rL);

		RL.PushInteger(RL.GetIdentity());

		return 1;
	}

	int RbxApi::getreg(DWORD rL)
	{
		syn::RbxLua RL(rL);

		RL.PushValue(LUA_REGISTRYINDEX);

		return 1;
	}

	int RbxApi::getgenv(DWORD rL)
	{
		syn::RbxLua RL(rL);
		syn::RbxLua GRL(InitRL);

		GRL.PushValue(LUA_GLOBALSINDEX);
		GRL.XMove(RL, 1);

		return 1;
	}

	int RbxApi::getrenv(DWORD rL)
	{
		syn::RbxLua RL(rL);
		syn::RbxLua LRL(RobloxGlobalRL);

		LRL.PushValue(LUA_GLOBALSINDEX);
		LRL.XMove(RL, 1);

		return 1;
	}

	int RbxApi::getinstancelist(DWORD rL)
	{
		syn::RbxLua RL(rL);

		static DWORD PushF = NULL;
		if (!PushF) PushF = RbxLua::GetBinValue(FNVA1_CONSTEXPR("pushinstance"));

		RL.PushValue(LUA_REGISTRYINDEX);
		RL.PushLightUserData((void*)PushF);
		RL.GetTable(-2);

		return 1;
	}

    int RbxApi::setclipboard(DWORD rL)
	{
        syn::RbxLua RL(rL);
        std::size_t Length;

        const char* String = RL.CheckLString(1, &Length);

        /* Open and empty clipboard */
        if (!OpenClipboard(syn::RobloxWindow) || !EmptyClipboard())
        {
            return RL.LError("failed to open clipboard");
        }

        HGLOBAL HG = GlobalAlloc(GMEM_FIXED, Length + 1);
        if (HG == NULL)
        {
            CloseClipboard();
            return RL.LError("failed to allocate space for clipboard");
        }

        memcpy(HG, String, Length + 1);

        if (!SetClipboardData(CF_TEXT, HG))
        {
            CloseClipboard();
            GlobalFree(HG);

            return RL.LError("failed to set clipboard");
        }

        CloseClipboard();

        return 0;
	}

    void settabss(RbxLua RL, const char* i, const char* v)
    {
        RL.PushString(v);
        RL.SetField(-2, i);
    }

    void settabsi(RbxLua RL, const char* i, int v) 
    {
        RL.PushInteger(v);
        RL.SetField(-2, i);
    }

    void treatstackoption(RbxLua RL, const char* fname)
    {
        RL.PushValue(-2);
        RL.Remove(-3);
        RL.SetField(-2, fname);
    }

	int RbxApi::getinfo(DWORD rL)
	{
		syn::RbxLua RL(rL);

        lua_Debug ar;
		std::string options;

		if (IsLuaU)
			options = RL.OptString(2, "flSu");
		else
			options = RL.OptString(2, "flnSu");

		if (IsLuaU && options.find('n') != std::string::npos)
			return RL.ArgError(2, "'name' fields are not supported on Lua U.");

        if (RL.IsNumber(1)) 
        {
            if (!RL.GetStack((int)RL.ToInteger(1), &ar)) 
            {
                RL.PushNil();  /* level out of range */
                return 1;
            }
        }
        else if (RL.IsFunction(1)) 
            options.insert(0, ">");
        else
            return RL.ArgError(1, "function or level expected");

        if (!RL.GetInfo(options.c_str(), &ar))
            return RL.ArgError(2, "invalid option");

        RL.CreateTable(0, 2);

        if (options.find('S') != std::string::npos) {
            settabss(RL, "source", ar.source);
            settabss(RL, "short_src", ar.short_src);
            settabsi(RL, "linedefined", ar.linedefined);
            settabsi(RL, "lastlinedefined", ar.lastlinedefined);
            settabss(RL, "what", ar.what);
        }
        if (options.find('l') != std::string::npos)
            settabsi(RL, "currentline", ar.currentline);
        if (options.find('u') != std::string::npos)
            settabsi(RL, "nups", ar.nups);
        if (options.find('n') != std::string::npos) {
            settabss(RL, "name", (ar.name == NULL) ? "" : ar.name);
            settabss(RL, "namewhat", ar.namewhat);
        }
        if (options.find('L') != std::string::npos)
            treatstackoption(RL, "activelines");
        if (options.find('f') != std::string::npos)
            treatstackoption(RL, "func");

        return 1;  /* return table */
	}

    int RbxApi::getstack(DWORD rL)
	{
        syn::RbxLua RL(rL);

        lua_Debug ar;
        if (!RL.GetStack(RL.CheckInteger(1), &ar))
            return RL.ArgError(1, "level out of range");

        RL.GetInfo("f", &ar);

        if (!RL.IsFunction(-1))
            return RL.LError("stack does not point to a function");

        if (RL.IsCFunction(-1))
            return RL.LError("stack points to a C closure, Lua function expected");

        DWORD CI = (DWORD)(*(CallInfo**)(rL + L_BCI) + ar.i_ci); /* L->base_ci + func */

        if (RL.IsNumber(2))
        {
            StkId Val = *(StkId*)(CI + CI_BASE) + RL.ToInteger(2) - 1; /* Lua-based indexing */
            r_setobj(rL, *(TValue**)(rL + L_TOP), Val);
            r_incr_top(rL);
        }
        else
        {
            RL.NewTable();

            int Iter = 0;
            for (StkId Val = *(StkId*)((DWORD)CI + CI_BASE); Val < *(StkId*)(CI + CI_TOP); ++Val)
            {
                RL.PushInteger(Iter++ + 1); /* Lua-based indexing */
                r_setobj(rL, *(TValue**)(rL + L_TOP), Val);
                r_incr_top(rL);
                RL.SetTable(-3);
            }
        }

        return 1;
	}

    int RbxApi::setstack(DWORD rL)
	{
        syn::RbxLua RL(rL);

        lua_Debug ar;
        if (!RL.GetStack(RL.CheckInteger(1), &ar))
            return RL.ArgError(1, "level out of range");

        lua_Integer Idx = RL.CheckInteger(2);
        RL.CheckAny(3);

        RL.GetInfo("f", &ar);

        if (!RL.IsFunction(-1))
            return RL.LError("stack does not point to a function");

        if (RL.IsCFunction(-1))
            return RL.LError("stack points to a C closure, Lua function expected");

        DWORD CI = (DWORD)(*(CallInfo**)(rL + L_BCI) + ar.i_ci); /* L->base_ci + func */

        StkId Val = *(StkId*)(CI + CI_BASE) + Idx - 1; /* Lua-based indexing */
        r_setobj(rL, Val, RL.Index2Adr(3));

        return 0;
	}

	int RbxApi::getupvalues(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.ArgCheck(RL.IsFunction(1) || RL.IsNumber(1), 1, "function or number expected");

		if (RL.IsNumber(1))
		{
			lua_Debug ar;
			if (!RL.GetStack(RL.ToInteger(1), &ar)) 
                 return RL.ArgError(1, "level out of range");

			RL.GetInfo("f", &ar);
		}

		RL.NewTable();

		const auto HSettings = syn::LuaTranslator::GetHsvmSettings(RL, -2);
		if (HSettings != nullptr && HSettings->VM != SECURELUA_VM_NONE)
		{
			return 1;
		}

		if (IsLuaU && !RL.IsCFunction(-2) && !syn::LuaTranslator::GetSynapseMarked(RL, -2))
		{
			const auto LC = RL.ToPointer(-2);
			const auto Upvalues = (UpVal**)(LC + LCL_UPVALS);

			for (auto i = 0; i < *(BYTE*)(LC + LCL_NUPVALS); i++)
			{
				const auto Current = Upvalues[i]->v;

				RL.PushNumber(i + 1);
				RL.PushObject(Current);
				RL.SetTable(-3);
			}
		}

		int n = 1;
		while (const char* name = RL.GetUpvalue(-2, n))
		{
			RL.PushString(name);
			RL.PushValue(-2);
			RL.SetTable(-4);
			RL.Pop(1);
			n++;
		}

		return 1;
	}

	int RbxApi::getupvalue(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.ArgCheck(RL.IsFunction(1) || RL.IsNumber(1), 1, "function or number expected");
        RL.ArgCheck(RL.IsString(2) || RL.IsNumber(2), 2, "string or number expected");

		if (RL.IsNumber(1))
		{
			lua_Debug ar;
			if (!RL.GetStack(RL.ToInteger(1), &ar))
				return RL.ArgError(1, "level out of range");

			RL.GetInfo("f", &ar);
		}
		else RL.PushValue(1);

		const auto HSettings = syn::LuaTranslator::GetHsvmSettings(RL, -1);
		if (HSettings != nullptr && HSettings->VM != SECURELUA_VM_NONE)
		{
			return 0;
		}

        if (RL.Type(2) == R_LUA_TSTRING)
        {
			if (IsLuaU && !RL.IsCFunction(-1) && !syn::LuaTranslator::GetSynapseMarked(RL, -1))
				return RL.LError("String based upvalue names are not supported in Lua U functions. Please use numeric indices instead.");

            const char* UName = RL.CheckString(2);

            int n = 1;
            while (const char* name = RL.GetUpvalue(-1, n))
            {
                if (!strcmp(name, UName)) return 1;
                RL.Pop(1);
                n++;
            }

        }
        else
        {
			if (IsLuaU && !RL.IsCFunction(-1) && !syn::LuaTranslator::GetSynapseMarked(RL, -1))
			{
				const auto LC = RL.ToPointer(-1);
				
				if (!RL.ToInteger(2))
					return RL.LError("upvalue index starts at 1");
				if (RL.ToInteger(2) >= *(BYTE*)(LC + LCL_NUPVALS))
					return RL.LError("upvalue index is out of range");

				const auto Upvalues = (UpVal**)(LC + LCL_UPVALS);
				const auto Upvalue = Upvalues[RL.ToInteger(2) - 1]->v;
				RL.PushObject(Upvalue);
				return 1;
			}

            if (RL.GetUpvalue(-1, RL.ToInteger(2)))
                return 1;
        }

		RL.PushNil();
		return 1;
	}

	int RbxApi::setupvalue(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.ArgCheck(RL.IsFunction(1) || RL.IsNumber(1), 1, "function or number expected");
        RL.ArgCheck(RL.IsString(2) || RL.IsNumber(2), 2, "string or number expected");

        RL.CheckAny(3);

		if (RL.IsNumber(1))
		{
			lua_Debug ar;
			if (!RL.GetStack(RL.ToInteger(1), &ar)) 
                return RL.ArgError(1, "level out of range");

            RL.GetInfo("f", &ar);
		}
		else RL.PushValue(1);

		const auto HSettings = syn::LuaTranslator::GetHsvmSettings(RL, -1);
		if (HSettings != nullptr && HSettings->VM != SECURELUA_VM_NONE)
		{
			return 0;
		}

        if (RL.Type(2) == R_LUA_TSTRING)
        {
			if (IsLuaU && !RL.IsCFunction(-1) && !syn::LuaTranslator::GetSynapseMarked(RL, -1))
				return RL.LError("String based upvalue names are not supported in Lua U functions. Please use numeric indices instead.");

            const char* UName = RL.CheckString(2);

            int n = 1;
            while (const char* name = RL.GetUpvalue(-1, n))
            {
                RL.Pop(1);
                if (!strcmp(name, UName))
                {
                    RL.PushValue(3);
                    RL.SetUpvalue(-2, n);
                    RL.PushBoolean(true);
                    return 1;
                }
                n++;
            }
        }
        else
		{
			const auto LC = RL.ToPointer(-1);

			if (!RL.ToInteger(2))
				return RL.LError("upvalue index starts at 1");
			if (RL.ToInteger(2) > *(BYTE*)(LC + LCL_NUPVALS))
				return RL.LError("upvalue index is out of range");

			const auto Upvalues = (UpVal**)(LC + LCL_UPVALS);
			auto Upvalue = Upvalues[RL.ToInteger(2) - 1]->v;

			const auto SetVal = (TValue*) RL.Index2Adr(3);
			Upvalue->value = SetVal->value;
			Upvalue->tt = SetVal->tt;

			RL.CBarrier(LC, (DWORD) SetVal);
			RL.PushBoolean(true);

			return 1;
		}

		RL.PushBoolean(false);
		return 1;
	}

	int RbxApi::getlocals(DWORD rL)
	{
		syn::RbxLua RL(rL);

		if (IsLuaU)
			return RL.LError("debug.getlocals is not supported on Lua U enabled games. Please use debug.getstack instead.");

		lua_Debug ar;
		if (!RL.GetStack(RL.CheckInteger(1), &ar)) 
            return RL.ArgError(1, "level out of range");

		RL.NewTable();

		int n = 1;
		while (const char* name = RL.GetLocal(&ar, n))
		{
			if (strcmp("(*temporary)", name) != 0)
			{
				RL.PushString(name);
				RL.PushValue(-2);
				RL.SetTable(-4);
			}
			RL.Pop(1);
			n++;
		}

		return 1;
	}

	int RbxApi::getlocal(DWORD rL)
	{
		syn::RbxLua RL(rL);

		if (IsLuaU)
			return RL.LError("debug.getlocal is not supported on Lua U enabled games. Please use debug.getstack instead.");

        RL.CheckType(1, R_LUA_TNUMBER);
        RL.CheckType(2, R_LUA_TSTRING);

		const char* Name = RL.ToString(2);

		lua_Debug ar;
		if (!RL.GetStack(RL.ToInteger(1), &ar))
            return RL.ArgError(1, "level out of range");

		int n = 1;
		while (const char* name = RL.GetLocal(&ar, n))
		{
			if (!strcmp(name, Name)) return 1;
			RL.Pop(1);
			n++;
		}

		RL.PushNil();
		return 1;
	}

	int RbxApi::setlocal(DWORD rL)
	{
		syn::RbxLua RL(rL);

		if (IsLuaU)
			return RL.LError("debug.setlocal is not supported on Lua U enabled games. Please use debug.setstack instead.");

        RL.CheckType(1, R_LUA_TNUMBER);
        RL.CheckType(2, R_LUA_TSTRING);
        RL.CheckAny(3);

		const char* Name = RL.ToString(2);

		lua_Debug ar;
		if (!RL.GetStack(RL.ToInteger(1), &ar)) 
            return RL.ArgError(1, "level out of range");

		int n = 1;
		while (const char* name = RL.GetLocal(&ar, n))
		{
			RL.Pop(1);
			if (!strcmp(name, Name))
			{
				RL.PushValue(3);
				RL.SetLocal(&ar, n);
				RL.PushBoolean(true);
				return 1;
			}
			n++;
		}

		RL.PushBoolean(false);
		return 1;
	}

	int RbxApi::getconstants(DWORD rL)
	{
		syn::RbxLua RL(rL);

	    RL.ArgCheck(RL.IsFunction(1) || RL.IsNumber(1), 1, "function or number expected");

	    if (RL.IsNumber(1))
        {
			lua_Debug ar;
	        if (!RL.GetStack(RL.ToInteger(1), &ar)) 
                return RL.ArgError(1, "level out of range");

            RL.GetInfo("f", &ar);

            if (RL.IsCFunction(-1))
                return RL.ArgError(1, "stack points to a C closure, Lua function expected");
        }
        else
        {
            RL.PushValue(1);

            if (RL.IsCFunction(-1))
                return RL.ArgError(1, "Lua function expected");
        }

		const auto HSettings = syn::LuaTranslator::GetHsvmSettings(RL, -1);
		if (HSettings != nullptr && HSettings->VM != SECURELUA_VM_NONE)
		{
			RL.NewTable();
			return 1;
		}

		int SizeK;
		TValue* Constants = syn::LuaTranslator::GetConstantsPointer(RL, -1, SizeK);

		RL.NewTable();

		for (int i = 0; i < SizeK; i++)
		{
			RL.PushInteger(i + 1); /* Lua-based indexing */
			syn::LuaTranslator::CloneConstant(RL, Constants, i);
			RL.SetTable(-3);
		}

		return 1;
	}

	int RbxApi::getconstant(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.ArgCheck(RL.IsFunction(1) || RL.IsNumber(1), 1, "function or number expected");

		const int Index = RL.CheckInteger(2);

	    RL.Pop(1);

	    if (RL.IsNumber(1))
        {
			lua_Debug ar;
			if (!RL.GetStack(RL.ToInteger(1), &ar)) 
                return RL.ArgError(1, "level out of range");

            RL.GetInfo("f", &ar);

            if (RL.IsCFunction(-1))
                return RL.ArgError(1, "stack points to a C closure, Lua function expected");
		}
        else
        {
            RL.PushValue(1);

            if (RL.IsCFunction(-1))
                return RL.ArgError(1, "Lua function expected");
        }

		const auto HSettings = syn::LuaTranslator::GetHsvmSettings(RL, -1);
		if (HSettings != nullptr && HSettings->VM != SECURELUA_VM_NONE)
		{
			return 0;
		}

		int SizeK;
		TValue* Constants = syn::LuaTranslator::GetConstantsPointer(RL, -1, SizeK);

		if (!Index)
			return RL.ArgError(2, "constant index starts at 1");
		if (Index > SizeK) 
            return RL.ArgError(2, "constant index is out of range");

		syn::LuaTranslator::CloneConstant(RL, Constants, Index - 1); /* Lua-based indexing */

		return 1;
	}

	int RbxApi::setconstant(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.ArgCheck(RL.IsFunction(1) || RL.IsNumber(1), 1, "function or number expected");

		const lua_Integer Index = RL.CheckInteger(2);

        RL.CheckAny(3);

        if (RL.IsNumber(1))
        {
            lua_Debug ar;
            if (!RL.GetStack(RL.ToInteger(1), &ar))
                return RL.ArgError(1, "level out of range");

            RL.GetInfo("f", &ar);

            if (RL.IsCFunction(-1))
                return RL.ArgError(1, "stack points to a C closure, Lua function expected");
        }
        else
        {
            RL.PushValue(1);

            if (RL.IsCFunction(-1))
                return RL.ArgError(1, "Lua function expected");
        }

		const auto HSettings = syn::LuaTranslator::GetHsvmSettings(RL, -1);
		if (HSettings != nullptr && HSettings->VM != SECURELUA_VM_NONE)
		{
			return 0;
		}

        int SizeK;
		TValue* Constants = syn::LuaTranslator::GetConstantsPointer(RL, -1, SizeK);

		if (!Index)
			return RL.ArgError(2, "constant index starts at 1");
		if (Index > SizeK) 
            return RL.ArgError(2, "constant index is out of range");

		auto Old = &Constants[Index - 1]; /* Lua-based indexing */
		const auto New = (TValue*) RL.Index2Adr(3);
		Old->tt = New->tt;
		Old->value = New->value;

		return 0;
	}

    /* This is a perfect case where CheckArgs wont work, some tuple system is needed */
	int RbxApi::setupvaluename(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.ArgCheck(RL.IsFunction(1) || RL.IsNumber(1), 1, "function or number expected");

		const int Index = RL.CheckInteger(2);

        RL.CheckString(3);

		if (RL.IsNumber(1))
		{
			lua_Debug ar;
			if (!RL.GetStack(RL.ToInteger(1), &ar)) 
                return RL.ArgError(1, "stack index is out of range");

            RL.GetInfo("f", &ar);
		}
		else RL.PushValue(1);		
		
		const auto HSettings = syn::LuaTranslator::GetHsvmSettings(RL, -1);
		if (HSettings != nullptr && HSettings->VM != SECURELUA_VM_NONE)
		{
			return 0;
		}

		int SizeUpvalues;
		TString** Upvalues = syn::LuaTranslator::GetUpvaluesPointer(RL, -1, SizeUpvalues);

		if (Index > SizeUpvalues)
            return RL.ArgError(2, "upvalue index is out of range");

		size_t UpvalueLength;
		const auto UpvaluePointer = RL.ToLString(3, &UpvalueLength);
		Upvalues[Index - 1] = (TString*) RL.NewLString(UpvaluePointer, UpvalueLength);

		return 0;
	}

	int RbxApi::checkrbxlocked(DWORD rL)
	{
		syn::RbxLua RL(rL);
        std::string got = RL.TypeName(1);
        RL.ArgCheck(checkinstance(RL, 1), 1, got.c_str());

		syn::Instance Inst = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(1));

		if (!Inst.GetParent())
		{
			RL.PushBoolean(false);
			return 1;
		}

		static std::unordered_set<std::string> ProtectedServices =
		{
			"CoreGui",
			"CorePackages"
		};

		while (Inst != DataModel && (DWORD) Inst.GetParent())
		{
			if (ProtectedServices.count(Inst.GetInstanceClassName()))
			{
				RL.PushBoolean(true);
				return 1;
			}

			Inst = Inst.GetParent();
		}

		RL.PushBoolean(false);

		return 1;
	}

	int RbxApi::checkinst(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.ArgCheck(checkinstance(RL, 1), 1, "userdata<Instance> expected");
        RL.ArgCheck(checkinstance(RL, 2), 2, "userdata<Instance> expected");

		const syn::Instance Inst = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(1));
		const syn::Instance Inst2 = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(2));

		RL.PushBoolean((DWORD)Inst == (DWORD)Inst2);

		return 1;
	}

	int RbxApi::checkparentchain(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.ArgCheck(checkinstance(RL, 1), 1, "userdata<Instance> expected");
        RL.ArgCheck(checkinstance(RL, 2), 2, "userdata<Instance> expected");

		syn::Instance Inst = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(1));
		const syn::Instance Inst2 = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(2));

		while (Inst != DataModel && (DWORD)Inst.GetParent())
		{
			if (Inst == Inst2)
			{
				RL.PushBoolean(true);
				return 1;
			}

			Inst = Inst.GetParent();
		}

		RL.PushBoolean(false);
		return 1;
	}

	int RbxApi::getclassname(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.ArgCheck(checkinstance(RL, 1), 1, "userdata<Instance> expected");

		const syn::Instance Inst = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(1));

		RL.PushString(Inst.GetInstanceClassName().c_str());

		return 1;
	}

	int RbxApi::getstates(DWORD rL)
	{
		syn::RbxLua RL(rL);
		
		RBXExtraSpace *ES = (RBXExtraSpace*)(rL - sizeof(RBXExtraSpace));
		RBXExtraSpace *Iter = ES->All->Head;

		RL.NewTable();

		unsigned Count = 0;
		while (Iter)
		{
			RL.PushNumber(++Count);
			RL.PushLightUserData((void*)((DWORD)Iter + sizeof(RBXExtraSpace)));
			RL.SetTable(-3);

			Iter = Iter->Next;
		}

		return 1;
	}

	int RbxApi::getpointerfromstate(DWORD rL)
	{
		RbxLua RL(rL);
		if (RL.Type(1) != R_LUA_TLIGHTUSERDATA)
			throw std::exception("expected thread as argument #1");

		DWORD UD = (DWORD)RL.ToUserData(1);
		if (*(BYTE*)(UD + GCO_TT) != R_LUA_TTHREAD)
			throw std::exception("expected thread as argument #1");

		RBXExtraSpace* ES = (RBXExtraSpace*)((DWORD)UD - sizeof(RBXExtraSpace));
		RL.PushLightUserData((void*)ES->ScriptPtr);

		return 1;
	}

    /* TODO: please learn to document things you literal tards */
	int RbxApi::getinstancefromstate(DWORD rL)
	{
		RbxLua RL(rL);
		if (RL.Type(1) != R_LUA_TLIGHTUSERDATA)
            return RL.ArgError(1, "expected thread as argument #1");

		DWORD UD = (DWORD)RL.ToUserData(1);
		if (*(BYTE*)(UD + GCO_TT) != R_LUA_TTHREAD)
			return RL.ArgError(1, "expected thread as argument #1");

		static DWORD PushF = NULL;
		if (!PushF) PushF = RbxLua::GetBinValue(FNVA1_CONSTEXPR("pushinstance"));

		RBXExtraSpace* ES = (RBXExtraSpace*)((DWORD)UD - sizeof(RBXExtraSpace));
		DWORD Scr = ES->ScriptPtr;
		if (IsBadReadPtr((DWORD*)Scr, 4) || IsBadReadPtr(*(DWORD**)Scr, 40))
			RL.PushNil();
		else
			((int(__cdecl*)(DWORD, DWORD))PushF)(rL, (DWORD)&ES->ScriptPtr);

		return 1;
	}

	int RbxApi::getstateenv(DWORD rL)
	{
		RbxLua RL(rL);
		if (RL.Type(1) != R_LUA_TLIGHTUSERDATA)
			throw std::exception("expected thread as argument #1");

		DWORD UD = (DWORD)RL.ToUserData(1);
		if (*(BYTE*)(UD + GCO_TT) != R_LUA_TTHREAD)
			throw std::exception("expected thread as argument #1");

		RbxLua NRL(UD);

		NRL.PushValue(LUA_GLOBALSINDEX);
		NRL.XMove(RL, 1);

		return 1;
	}

	int RbxApi::getloadedmodules(DWORD rL)
	{
		syn::RbxLua RL(rL);

		std::vector<std::shared_ptr<uintptr_t>> Instances;

		static DWORD PushF = NULL;
		if (!PushF) PushF = RbxLua::GetBinValue(FNVA1_CONSTEXPR("pushinstance"));

		//will NOT work in debug mode due to iterator proxy - "Exception thrown while cleaning up Lua"
#ifndef _DEBUG
		const auto LoadedModules = *(std::set<std::weak_ptr<uintptr_t>>*) (syn::Instance(syn::DataModel).GetChildFromClassName("ScriptContext") + 0x124);

		for (auto Mod : LoadedModules)
			if (!Mod.expired())
				Instances.push_back(Mod.lock());

		RL.NewTable();

		for (size_t i = 0; i < Instances.size(); i++)
		{
			RL.PushInteger(i + 1);
			((int(__cdecl*)(DWORD, std::shared_ptr<uintptr_t>&))PushF)(rL, Instances[i]);

			RL.SetTable(-3);
		}
#else
		RL.NewTable();
#endif

		return 1;
	}

	int RbxApi::getgc(DWORD rL)
	{
		const syn::RbxLua RL(rL);

		auto IncludeTables = false;
		if (RL.IsBoolean(1))
			IncludeTables = RL.ToBoolean(1);

		const auto GlobalState = (DWORD) syn::PointerObfuscation::DeObfuscateGlobalState(RL + L_GS);
		const auto DeadMask = *(BYTE*)(GlobalState + G_WMASK) ^ 3;
		auto Object = *(GCObject**)(GlobalState + G_ROOTGC);

		RL.NewTable();
		
		auto n = 1;
		while (Object != nullptr)
		{
			const auto TT = *(BYTE*)((DWORD) Object + GCO_TT);

			/* Make sure the object is not dead and is a 'safe' type (without including tables) */
			if ((TT == R_LUA_TFUNCTION || (IncludeTables ? TT == R_LUA_TTABLE : TT == R_LUA_TFUNCTION) || (IncludeTables ? TT == R_LUA_TUSERDATA : TT == R_LUA_TFUNCTION)) && (*(BYTE*)((DWORD) Object + GCO_MARKED) ^ 3) & DeadMask)
			{
				RL.PushInteger(n++);
				RL.PushRawObject((DWORD) Object, TT);

				RL.SetTable(-3);
			}

			Object = Object->gch.next;
		}

		return 1;
	}

	int RbxApi::getsenv(DWORD rL)
	{
		syn::RbxLua RL(rL);

        if (!RL.IsUserData(1) && !checkinstance(RL, 1))
            return RL.TypeError(1, "Variant<userdata[LocalScript, ModuleScript]>");

		const syn::Instance Inst = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(1));
        
        std::string classname = Inst.GetInstanceClassName();
		if (classname != "LocalScript" && classname != "ModuleScript")
			return RL.TypeError(1, "Variant<userdata[LocalScript, ModuleScript]> expected");

		DWORD LS = 0;

        VM_TIGER_WHITE_START;

		if (classname == "ModuleScript")
		{
			const auto GlobalState = (DWORD)syn::PointerObfuscation::DeObfuscateGlobalState(RL + L_GS);
			auto Object = *(GCObject**)(GlobalState + G_ROOTGC);

			while (Object != 0)
			{
				if (Object->gch.tt == R_LUA_TTHREAD)
				{
					RBXExtraSpace* Es = (RBXExtraSpace*)((DWORD)Object - sizeof(RBXExtraSpace));
					DWORD Scr = Es->ScriptPtr;
					if (Scr == Inst)
					{
						LS = (DWORD) Object;
						break;
					}
				}

				Object = Object->gch.next;
			}
		}
		else
		{
			//todo: WTF IS THIS DEFCON
			if (!*(DWORD*)((DWORD)Inst + 0xEC))
				return RL.LError("could not get script environment - localscript not running");

			DWORD N = *(DWORD*)((DWORD)Inst + 0xEC);
			DWORD WTR = *(DWORD*)(N + 4);
			DWORD LIS = *(DWORD*)(WTR + 20);
			LS = *(DWORD*)(LIS + 8);
		}

        VM_TIGER_WHITE_END;

		if (!LS)
			return RL.LError("could not get script environment");

		syn::RbxLua NRL(LS);

		NRL.PushValue(LUA_GLOBALSINDEX);
		NRL.XMove(RL, 1);

		return 1;
	}

	int RbxApi::islclosure(DWORD rL)
	{
		syn::RbxLua RL(rL);

		RL.CheckType(1, R_LUA_TFUNCTION);

		RL.PushBoolean(!RL.IsCFunction(1));

		return 1;
	}

	int RbxApi::issynfunc(DWORD rL)
	{
		syn::RbxLua RL(rL);

		RL.CheckType(1, R_LUA_TFUNCTION);
        RL.CheckType(1, R_LUA_TFUNCTION);

		if (RL.IsCFunction(1))
		{
			static DWORD CraInvoke = NULL;
			if (!CraInvoke) CraInvoke = syn::RobloxBase(OBFUSCATED_NUM(syn::CallCheck::CallcheckAddress));

			const auto CheckClosure = (CClosure*)RL.ToPointer(1);
			const auto CheckCFunction = syn::PointerObfuscation::DeObfuscateCClosure((DWORD)CheckClosure + 20);
			RL.PushBoolean(CheckCFunction == CraInvoke);
			return 1;
		}

		RL.PushBoolean(syn::LuaTranslator::GetSynapseMarked(RL, 1));
		return 1;
	}

	int RbxApi::isrbxactive(DWORD rL)
	{
		syn::RbxLua RL(rL);

		RL.PushBoolean(GetForegroundWindow() == syn::RobloxWindow);

		return 1;
	}

    /* TODO: Please document this function */
	int RbxApi::getcallingscript(DWORD rL)
	{
		syn::RbxLua RL(rL);

		if (*(DWORD*)((DWORD)rL - 32) && ((DWORD)rL - 28) > 0) //scriptptr && script ptr ref count > 0
		{
			static DWORD PushF = NULL;
			if (!PushF) PushF = RbxLua::GetBinValue(FNVA1_CONSTEXPR("pushinstance"));

			((int(__cdecl*)(DWORD, DWORD))PushF)(rL, ((DWORD)rL - 32));
		}
		else RL.PushNil();

		return 1;
	}

#ifdef EnableLuaUTranslator
	void LuaUConvert(lua_State* L, Proto* P)
	{
		const auto Conv = syn::OneWayLuauTranslator(P).Convert(P->code, P->sizecode);

		P->sizecode = Conv.size();
		P->code = luaM_newvector(L, Conv.size(), Instruction);

		for (auto i = 0; i < Conv.size(); i++)
			P->code[i] = (Instruction) Conv[i];

		for (auto i = 0; i < P->sizep; i++)
			LuaUConvert(L, P->p[i]);
	}

	int RbxApi::luaudump(DWORD rL)
	{
		const syn::RbxLua RL(rL);

		size_t ScriptLength;
		const auto Source = RL.CheckLString(1, &ScriptLength);

		const auto NState = luaL_newstate();

		if (luaL_loadbuffer(NState, Source, ScriptLength, BS_LUA, "testing"))
		{
			std::string Err = lua_tostring(NState, -1);
			lua_close(NState);
			throw std::exception(Err.c_str());
		}

		const auto C = (Closure*) lua_topointer(NState, -1);
		const auto P = C->l.p;

		LuaUConvert(NState, P);

		luaL_Buffer B;
		luaL_buffinit(NState, &B);
		if (lua_dump(NState, syn::LuaTranslator::DumpWriter, &B) != 0) throw std::exception("failed to dump bytecode");
		luaL_pushresult(&B);
		size_t Len;
		const auto BC = lua_tolstring(NState, -1, &Len);

		const std::string Res(BC, Len);

		lua_close(NState);

		RL.PushLString(Res.c_str(), Res.size());

		return 1;
	}
#endif

	int RbxApi::mouse1press(DWORD rL)
	{
		if (GetForegroundWindow() == syn::RobloxWindow)
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);

		return 0;
	}

	int RbxApi::mouse1release(DWORD rL)
	{
		if (GetForegroundWindow() == syn::RobloxWindow)
			mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

		return 0;
	}

	int RbxApi::mouse1click(DWORD rL)
	{
		if (GetForegroundWindow() == syn::RobloxWindow)
			mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

		return 0;
	}

	int RbxApi::mouse2press(DWORD rL)
	{
		if (GetForegroundWindow() == syn::RobloxWindow)
			mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);

		return 0;
	}

	int RbxApi::mouse2release(DWORD rL)
	{
		if (GetForegroundWindow() == syn::RobloxWindow)
			mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);

		return 0;
	}

	int RbxApi::mouse2click(DWORD rL)
	{
		if (GetForegroundWindow() == syn::RobloxWindow)
			mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);

		return 0;
	}

	int RbxApi::keypress(DWORD rL)
	{
        syn::RbxLua RL(rL);
        
        UINT key = RL.CheckInteger(1);

		if (GetForegroundWindow() == syn::RobloxWindow)
            keybd_event(0, (BYTE)MapVirtualKeyA(key, MAPVK_VK_TO_VSC), KEYEVENTF_SCANCODE, 0);

		return 0;
	}

	int RbxApi::keyrelease(DWORD rL)
	{
        syn::RbxLua RL(rL);

        UINT key = RL.CheckInteger(1);

		if (GetForegroundWindow() == syn::RobloxWindow)
            keybd_event(0, (BYTE)MapVirtualKeyA(key, MAPVK_VK_TO_VSC), KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP, 0);

		return 0;
	}

	int RbxApi::mousemoverel(DWORD rL)
	{
        syn::RbxLua RL(rL);
        
        DWORD x = (DWORD)RL.CheckLong(1);
        DWORD y = (DWORD)RL.CheckLong(2);

		if (GetForegroundWindow() == syn::RobloxWindow)
            mouse_event(MOUSEEVENTF_MOVE, x, y, 0, 0);

		return 0;
	}

	int RbxApi::mousemoveabs(DWORD rL)
	{
		syn::RbxLua RL(rL);

        DWORD x = (DWORD)RL.CheckLong(1);
        DWORD y = (DWORD)RL.CheckLong(2);

		if (GetForegroundWindow() != syn::RobloxWindow) return 0;

		int width = GetSystemMetrics(SM_CXSCREEN) - 1;
		int height = GetSystemMetrics(SM_CYSCREEN) - 1;

		RECT CRect;
		GetClientRect(GetForegroundWindow(), &CRect);

		POINT Point{ CRect.left, CRect.top };
		ClientToScreen(GetForegroundWindow(), &Point);

		x = (x + (DWORD)Point.x) * (65535 / width);
		y = (y + (DWORD)Point.y) * (65535 / height);

		mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x, y, 0, 0);
		return 0;
	}

	int RbxApi::mousescroll(DWORD rL)
	{
		syn::RbxLua RL(rL);

        DWORD scrollAmount = (DWORD)RL.CheckLong(1);

		if (GetForegroundWindow() == syn::RobloxWindow)
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, scrollAmount, 0);

		return 0;
	}

	int RbxApi::newcclosurehandler(DWORD rL)
	{
		syn::RbxLua RL(rL);

		/* Stack:
		 * 1 - newcclosurehandler
		 * 2 - synapse lua function (user)
		 * 3 - newcclosurestub
		 * 4 - caller function (what we want)
		 */

		/* Lazy coding inbound, we need to create a fake exception trace for our error */
		size_t ErrSize;
		const auto ErrPtr = RL.ToLString(-1, &ErrSize);
		const auto Err = std::string(ErrPtr, ErrSize);

		/* Check for yield */
		if (Err.find("attempt to yield across") != std::string::npos && Err.find("is not") == std::string::npos)
		{
			RL.Pop(1);
			RL.PushBoolean(true);
			return 1;
		}

		/* If not, start our secondary exception handling */
		lua_Debug ar;
		if (!RL.GetStack(4, &ar)) return RL.GetTop();
		RL.GetInfo("Sl", &ar);

		lua_Debug ar_syn;
		if (!RL.GetStack(2, &ar_syn)) return RL.GetTop();
		RL.GetInfo("Sl", &ar_syn);

		/* Check for non-newcclosure error */
		if (Err.find(std::string(ar_syn.source).erase(0, 1)) == std::string::npos)
			return RL.GetTop();

		std::vector<std::string> Tokenized;
		SplitString(Err, ":", Tokenized);

		/* Use the source from the real function */
		std::stringstream Stream;
		Stream << std::string(ar.source).erase(0, 1) << ":" << ar.currentline << ":";

		/* Put actual error */
		for (size_t i = 0; i < Tokenized.size(); i++)
		{
			if (i == 0 || i == 1) continue;
			Stream << Tokenized.at(i) << ":";
		}

		/* Erase final ':' from string */
		auto Final = Stream.str();
		Final.erase(Final.size() - 1);

		RL.Pop(1);
		RL.PushLString(Final.c_str(), Final.size());

		return RL.GetTop();
	}

	int RbxApi::newcclosurestub(DWORD rL)
	{
		syn::RbxLua RL(rL);

		const auto Func = (*(SynCClosure*)RL.ToUserData(lua_upvalueindex(1))).ExtraSpace;
		RL.PushRawObject(Func, R_LUA_TFUNCTION);
		RL.Insert(1);
		RL.PushRawObject(CClosureHandlers[syn::PointerObfuscation::DeObfuscateGlobalState(RL + L_GS)], R_LUA_TFUNCTION);
		RL.Insert(1);

		const auto Res = RL.PCall(RL.GetTop() - 2, LUA_MULTRET, 1);

		RL.Remove(1);
		
		if (Res && Res != LUA_YIELD)
		{
			if (RL.IsBoolean(-1))
				return RL.RYield(0);

			return RL.Error();
		}

		return RL.GetTop();
	}

	int RbxApi::newcclosurestub_noupvals(DWORD rL, SynCClosure cL)
	{
		syn::RbxLua RL(rL);

		const auto Func = cL.ExtraSpace;
		RL.PushRawObject(Func, R_LUA_TFUNCTION);
		RL.Insert(1);
		RL.PushRawObject(CClosureHandlers[syn::PointerObfuscation::DeObfuscateGlobalState(RL + L_GS)], R_LUA_TFUNCTION);
		RL.Insert(1);

		const auto Res = RL.PCall(RL.GetTop() - 2, LUA_MULTRET, 1);

		RL.Remove(1);

		if (Res && Res != LUA_YIELD)
		{
			if (RL.IsBoolean(-1))
				return RL.RYield(0);

			return RL.Error();
		}

		return RL.GetTop();
	}

	int RbxApi::newcclosure(DWORD rL)
	{
		syn::RbxLua RL(rL);

        RL.CheckType(1, R_LUA_TFUNCTION);

        if (RL.IsCFunction(1))
            return RL.ArgError(1, "lua function expected");

		/* lazy hack so we don't get GC'd */
		RL.PushValue(1);
		RL.SetField(LUA_REGISTRYINDEX, RandomString(16).c_str());

		DWORD& CCH = CClosureHandlers[syn::PointerObfuscation::DeObfuscateGlobalState(RL + L_GS)];

		if (!CCH)
		{
			/* prevent CClosure handler from being GC'd */
			RL.PushCFunction(syn::RbxApi::newcclosurehandler);
			CCH = RL.ToPointer(-1);
			RL.SetField(LUA_REGISTRYINDEX, RandomString(16).c_str());
		}

		const auto CC = new SynCClosure();
		CC->Address = (DWORD) newcclosurestub;
		CC->ExtraSpace = RL.ToPointer(1);

		RL.PushLightUserData(CC);
		RL.CreateCClosure(1);

		return 1;
	}

    int RbxApi::hookfunction(DWORD rL)
    {
        syn::RbxLua RL(rL);

        RL.CheckType(1, R_LUA_TFUNCTION);
        RL.CheckType(2, R_LUA_TFUNCTION);

        if (RL.IsCFunction(1))
        {
            if (!RL.IsCFunction(2))
                return RL.ArgError(2, "C function expected");

            const auto OldClosure = (CClosure*) RL.ToPointer(1);
            const auto HookClosure = (CClosure*) RL.ToPointer(2);

			/* Make sure we don't fuck ourselves over with GC */
			RL.PushValue(2);
			RL.SetField(LUA_REGISTRYINDEX, RandomString(16).c_str());

            const auto OldCFunction = syn::PointerObfuscation::DeObfuscateCClosure((DWORD) OldClosure + 20);
            const auto HookCFunction = syn::PointerObfuscation::DeObfuscateCClosure((DWORD) HookClosure + 20);

            /* Backup old function */

            for (auto i = 0; i < OldClosure->nupvalues; i++)
                RL.PushObject(&OldClosure->upvalue[i]);

            RL.PushCClosure((r_lua_CFunction) OldCFunction, OldClosure->nupvalues);

			if (!OldClosure->nupvalues && HookClosure->nupvalues == 1)
			{
				/* Check if we can use 0 upvalue hookfunction */
				const auto CC = (SynCClosure*) HookClosure->upvalue[0].value.p;

				if (CC != nullptr && CC->Address == (DWORD) newcclosurestub)
				{
					/* We can. */
					SynCClosure SCC = { (DWORD) newcclosurestub_noupvals, CC->ExtraSpace };

					HookedFunctionsMap[(std::uintptr_t) OldClosure] = SCC;
					syn::PointerObfuscation::ObfuscateCClosure((DWORD) OldClosure + 20, HookCFunction);

					return 1;
				}
			}

            /* Hook */
            syn::PointerObfuscation::ObfuscateCClosure((DWORD) OldClosure + 20, HookCFunction);
            OldClosure->nupvalues = HookClosure->nupvalues;

            for (auto i = 0; i < HookClosure->nupvalues; i++)
            {
                auto OT = &OldClosure->upvalue[i];
                const auto HT = &HookClosure->upvalue[i];

                OT->value = HT->value;
                OT->tt = HT->tt;
            }

            /* Return */
            return 1;
        }
        else
        {
            if (RL.IsCFunction(2))
                return RL.ArgError(2, "lua function expected");

            const auto OldClosure = (DWORD) RL.ToPointer(1);
            const auto HookClosure = (DWORD) RL.ToPointer(2);

			/* Make sure we don't fuck ourselves over with GC */
			RL.PushValue(2);
			RL.SetField(LUA_REGISTRYINDEX, RandomString(16).c_str());

            const auto OldLFunction = syn::PointerObfuscation::DeObfuscateLClosure((DWORD) OldClosure + 20);
            const auto HookLFunction = syn::PointerObfuscation::DeObfuscateLClosure((DWORD) HookClosure + 20);

            if (*(BYTE*)(HookClosure + LCL_NUPVALS) > *(BYTE*)(OldClosure + LCL_NUPVALS) && *(BYTE*)(HookClosure + LCL_NUPVALS) > 1)
                return RL.ArgError(2, "hook function has too many upvalues");

            RL.PushString(RandomString(16).c_str());
            RL.PushValue(2);
            RL.SetTable(LUA_REGISTRYINDEX);

            /* Backup old function */
            DWORD LC = RL.NewLClosure(*(lu_byte*)(OldClosure + LCL_NUPVALS), *(lu_byte*)(OldClosure + LCL_MAXSTACKSIZE), NULL);

            memcpy((void*)(DWORD*)(LC + 4), (void*)(DWORD*)(OldClosure + 4), sizeLclosure(*(lu_byte*)(OldClosure + LCL_NUPVALS) + 1));

            syn::PointerObfuscation::ObfuscateLClosure(LC + 20, OldLFunction);

            /* Hook */
            memcpy((void*)(DWORD*)(OldClosure + 4), (void*)(DWORD*)(HookClosure + 4), sizeLclosure(*(lu_byte*)(HookClosure + LCL_NUPVALS) + 1));

            syn::PointerObfuscation::ObfuscateLClosure(OldClosure + 20, HookLFunction);

            /* Push LClosure to stack */
            r_setclvalue(*(TValue**)(RL + L_TOP), LC);
            r_incr_top(RL);

            /* Return */
            return 1;
        }
    }

	bool RbxApi::decompile_sanity_check(syn::Instance Inst)
	{
		try
		{
			if (Inst.GetInstanceClassName() == "LocalScript")
			{
				if (!*(DWORD*)(Inst + 0xF8)) return false;
				auto Bytecode = *(std::string*)(*(DWORD*)(Inst + 0xF8) + 0x18);
				if (Bytecode.size() <= 8)
				{
					Bytecode = *(std::string*)(Inst + 0xBC); /* CachedRemoteSource + 0x18 */
					if (Bytecode.size() <= 8) return false;
				}
				return true;
			}

			auto RProt = new RbxProtectedString();
			static DWORD ModScrDump = 0;
			if (!ModScrDump) ModScrDump = RbxLua::GetBinValue(FNVA1_CONSTEXPR("modscriptdump"));
			const auto ProtectedString = ((int(__thiscall*)(int, int))ModScrDump)(Inst, (DWORD)RProt);
			auto Bytecode = *(std::string*)(ProtectedString + 0x18);
			if (Bytecode.size() <= 8)
			{
				Bytecode = *(std::string*)(Inst + 0xBC); /* CachedRemoteSource + 0x18 */
				if (Bytecode.size() <= 8) return false;
			}

			return true;
		}
		catch (std::exception&)
		{
			return false;
		}
	}

	void KillProcessTree(const DWORD ProcId)
	{
		PROCESSENTRY32 PE;
		memset(&PE, 0, sizeof(PROCESSENTRY32));
		PE.dwSize = sizeof(PROCESSENTRY32);

		const auto HSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

		if (Process32First(HSnap, &PE))
		{
			do 
			{
				if (PE.th32ParentProcessID == ProcId)
					KillProcessTree(PE.th32ProcessID);
			} while (Process32Next(HSnap, &PE));
		}

		const auto HProc = OpenProcess(PROCESS_TERMINATE, FALSE, ProcId);

		if (HProc)
		{
			TerminateProcess(HProc, 1);
			CloseHandle(HProc);
		}
	}

    int RbxApi::decompile(DWORD rL)
    {
        syn::RbxLua RL(rL);

        RbxYield RYield(rL);

#ifndef EnableLuaUDecompiler
		if (IsLuaU)
		{
			RL.PushString(
				"--SynapseX Decompiler\n--The Synapse X decompiler is currently unsupported on LuaU enabled games.");
			return 1;
		}
#endif

        RL.ArgCheck(RL.IsFunction(1) || RL.IsProto(1) || RL.IsString(1) || RL.IsUserData(1), 1,
            "Variant<userdata[LocalScript, ModuleScript], function, string, proto> expected");

        enum DecModes
        {
            DEC_DUMP,
            DEC_LEGACY,
            DEC_NEW,
        };

        auto DecMode = DEC_LEGACY;
        if (RL.Type(2) == R_LUA_TBOOLEAN)
        {
            DecMode = RL.ToBoolean(2) ? DEC_DUMP : DEC_LEGACY;
            RL.Pop(1);
        }

		auto Timeout = INFINITE;
		if (RL.Type(3) == R_LUA_TNUMBER)
		{
			Timeout = RL.ToNumber(3) * 1000;
		}

        if (RL.Type(2) == R_LUA_TSTRING)
        {
            std::string DmpStr = RL.ToString(2);
            std::transform(DmpStr.begin(), DmpStr.end(), DmpStr.begin(), tolower);

            if (DmpStr == "dump")
            {
                DecMode = DEC_DUMP;
            }
            else if (DmpStr == "regular" || DmpStr == "reg" || DmpStr == "legacy" || DmpStr == "old")
            {
                DecMode = DEC_LEGACY;
            }
            else if (DmpStr == "new" || DmpStr == "beta")
            {
                DecMode = DEC_NEW;
            }
            else
            {
                return RL.ArgError(2, "invalid decompilation mode");
            }

            RL.Pop(1);
        }

        std::string BC, DecKey;

        switch (RL.Type(1))
        {
		    case R_LUA_TUSERDATA:
		    case R_LUA_TLIGHTUSERDATA:
		    {
		        if (!checkinstance(RL, 1))
		            return RL.ArgError(1, "Variant<userdata[LocalScript, ModuleScript], function, string, proto> expected");

		        syn::Instance Inst = DereferenceSmartPointerInstance((DWORD)RL.ToUserData(1));
		        const std::string ClassName = Inst.GetInstanceClassName();

		        if (ClassName != "LocalScript" && ClassName != "ModuleScript")
		            return RL.ArgError(1, "Variant<userdata[LocalScript, ModuleScript], function, string, proto> expected");

		        if (!decompile_sanity_check(Inst))
		        {
		            RL.PushString(
		                "--SynapseX Decompiler\n--This script could not be decompiled due to it having no bytecode.\n--This is usually caused by trying to decompile a Synapse X generated script.");
		            return 1;
		        }

		        DWORD ProtectedString;
		        if (ClassName == "LocalScript")
		            ProtectedString = *(DWORD*)(Inst + 0xF8);
		        else
		        {
		            auto RProt = new RbxProtectedString();
		            static DWORD ModScrDump = 0;
		            if (!ModScrDump) ModScrDump = RbxLua::GetBinValue(FNVA1_CONSTEXPR("modscriptdump"));
		            ProtectedString = ((int(__thiscall*)(int, int))ModScrDump)(Inst, (DWORD)RProt);
		        }

		        if ((*(std::string*)(ProtectedString + 0x18)).size() <= 8)
		            ProtectedString = Inst + 0xA4;

		        static DWORD HateFlag = 0;
		        if (!HateFlag) HateFlag = RbxLua::GetBinValue(FNVA1_CONSTEXPR("hateflag"));
		        static DWORD Deserialize = 0;
		        if (!Deserialize) Deserialize = RbxLua::GetBinValue(FNVA1_CONSTEXPR("deserialize"));

		        const auto Bytecode = *(std::string*)(ProtectedString + 0x18);
		        const auto HateBackup = *(DWORD*)HateFlag;
		        ((int(__cdecl*)(DWORD, const std::string&, const char*, DWORD))Deserialize)(
		            rL, Bytecode, OBFUSCATE_STR("SXDecompiler"), 1);
		        *(DWORD*)HateFlag = HateBackup;

		        if (RL.Type(-1) != R_LUA_TFUNCTION || !RL.ToPointer(-1))
		        {
		            RL.PushString(
		                "--SynapseX Decompiler\n--This script could not be decompiled due to it having invalid bytecode.\n--This is usually caused by trying to decompile a Synapse X generated script.");

		            return 1;
		        }
		    }
		    case R_LUA_TFUNCTION:
		    {
		        if (RL.IsCFunction(1))
		            return RL.ArgError(1, "Variant<userdata[LocalScript, ModuleScript], function, string, proto> expected");

		        if (syn::LuaTranslator::GetSynapseMarked(RL, -1))
		        {
		            RL.PushString(
		                "--SynapseX Decompiler\n--This function can not be decompiled due to it being created by Synapse X.\n--You can use `dumpstring(\"<your code here>\")` to convert to Roblox format bytecode though.");

		            return 1;
		        }

		        try
		        {
		            const auto Translator = syn::LuaTranslator::GetSingleton();
		            if (DecMode == DEC_NEW)
		            {
                        VM_TIGER_WHITE_START;

		                auto LS = luaL_newstate();
		                auto LC = Translator->DumpToFunc(LS, RL);
		                auto ObfDumped = syn::ObfuscatedDumper(LC->l).Dump();
		                BC = std::get<0>(ObfDumped);
		                DecKey = std::get<1>(ObfDumped);
		                lua_close(LS);
                        
                        VM_TIGER_WHITE_END;
		            }
		            else
		            {
		                BC = Translator->Dump(RL);
		            }
		        }
		        catch (const std::exception& ex)
		        {
		            RL.PushString(ex.what());

		            return 2;
		        }
		        break;
		    }
		    case R_LUA_TPROTO:
		    {
		        if (syn::LuaTranslator::GetSynapseMarked(RL, -1))
		        {
		            RL.PushString(
		                "--SynapseX Decompiler\n--This function can not be decompiled due to it being created by Synapse X.\n--You can use `dumpstring(\"<your code here>\")` to convert to Roblox format bytecode though.");

		            return 1;
		        }

		        try
		        {
		            const auto Translator = syn::LuaTranslator::GetSingleton();
		            if (DecMode == DEC_NEW)
		            {
                        VM_TIGER_WHITE_START;

		                auto LS = luaL_newstate();
		                auto LP = Translator->DumpToProto(LS, RL);
		                LClosure lc{};
		                lc.p = LP;
		                auto ObfDumped = syn::ObfuscatedDumper(lc).Dump();
		                BC = std::get<0>(ObfDumped);
		                DecKey = std::get<1>(ObfDumped);
		                lua_close(LS);

                        VM_TIGER_WHITE_END;
		            }
		            else
		            {
		                BC = Translator->Dump(RL);
		            }
		        }
		        catch (const std::exception& ex)
		        {
		            RL.PushString(ex.what());

		            return 2;
		        }
		        break;
		    }
		    case R_LUA_TSTRING:
		    {
		        std::size_t BCLength;
		        const auto BCPointer = RL.ToLString(1, &BCLength);

		        BC = std::string(BCPointer, BCLength);

		        if (DecMode == DEC_NEW)
		        {
                    VM_TIGER_WHITE_START;

		            DecKey = RandomString(16);
		            const auto SKeyGrab = (DecKey.at(0) * OBFUSCATED_NUM_UNCACHE(16777216) + DecKey.at(1) * OBFUSCATED_NUM_UNCACHE(65536) + DecKey.at(2) * OBFUSCATED_NUM_UNCACHE(256) + DecKey.at(3)) % 128;

		            for (char& i : BC)
			            i ^= SKeyGrab;

		            auto FKey = OBFUSCATE_STR("F8ixT9H4z8moGusU") + DecKey;
		            size_t FLen;
		            auto FRet = xxtea_encrypt(BC.c_str(), BC.length(), FKey.c_str(), &FLen);

		            BC = std::string(Base64Encode((byte*)FRet, FLen));

                    VM_TIGER_WHITE_END;
		        }

		        break;
		    }
            default: break;
        }

        if (DecMode == DEC_DUMP)
        {
			RL.PushLString(BC.c_str(), BC.size());
			return 1;
        }

        return RYield.Execute([BC, DecMode, DecKey, Timeout]()
        {
	        auto BinDir = syn::D3D::GetWorkingPath() + L"\\bin\\";

            auto DmpStr = ConvertToWStr(RandomString(16));
            auto DecompStr = ConvertToWStr(RandomString(16));
            auto DecKeyW = ConvertToWStr(DecKey);

            auto DmpBytecodeName = BinDir + DmpStr + L".out";
            auto DecompName = BinDir + DecompStr + L".txt";

            /* Write bytecode file */
            std::ofstream OutFile;
            OutFile.open(DmpBytecodeName, std::ios::out | std::ios::binary);
            OutFile.write(BC.c_str(), BC.size());
            OutFile.close();

            /* Decompile with unluac */
            STARTUPINFOW si; ZeroMemory(&si, sizeof(si));
            PROCESS_INFORMATION pi; ZeroMemory(&pi, sizeof(pi));
            si.cb = sizeof(si);
            
            constexpr size_t PathSz = (MAX_PATH + 1) * sizeof(wchar_t);
            wchar_t CMDDir[PathSz];
            UINT Result = GetSystemDirectoryW(CMDDir, PathSz);

            /* Potential miscalulation on my part, look into size conflicts */
            if (Result > PathSz || Result == 0)
            {
                std::wstring DefaultCmdDir = L"C:\\Windows\\System32\\cmd.exe";
                wcsncpy_s(CMDDir, DefaultCmdDir.c_str(), DefaultCmdDir.size());
            }
            else
            {
                wcscat_s(CMDDir, L"\\cmd.exe");
            }

            if (DecMode == DEC_LEGACY)
            {
                if (Is64BitOS())
                {
                    syn::Resource* unluac_exe = new syn::Resource("unluac-native", BinDir + L"unluac.exe",
                        OBFUSCATE_STR("https://cdn.synapse.to/synapsedistro/unluac/unluac_native_may1819.exe"));
                    unluac_exe->get();

                    if (!CreateProcessW(CMDDir,
                        (LPWSTR)(L"/C unluac.exe \"" + DmpBytecodeName + L"\" > \"" + DecompName + L"\"").c_str(), NULL, NULL,
                        FALSE, CREATE_NO_WINDOW, NULL, BinDir.c_str(), &si, &pi))
                    {
                        std::filesystem::remove(DmpBytecodeName);
                        throw std::exception("failed to decompile: 0x01");
                    }
                }
                else
                {
                    syn::Resource* unluac_jar = new syn::Resource("unluac", BinDir + L"unluac.jar",
                        OBFUSCATE_STR("https://cdn.synapse.to/synapsedistro/unluac/unluac_oct1118.jar"));
                    unluac_jar->get();

                    if (!CreateProcessW(CMDDir,
                        (LPWSTR) ((std::wstring(L"/C java -Xss32M -jar unluac.jar \"") + DmpBytecodeName + L"\" > \"" + DecompName + L"\"").c_str()), NULL, NULL,
                        FALSE, CREATE_NO_WINDOW, NULL, BinDir.c_str(), &si, &pi))
                    {
                        std::filesystem::remove(DmpBytecodeName);
                        throw std::exception("failed to decompile: 0x02");
                    }
                }
            }
            else
            {
                if (Is64BitOS())
                {
                    syn::Resource* unluac_exe = new syn::Resource("unluac-native-new", BinDir + L"unluac-new.exe",
                        OBFUSCATE_STR("https://cdn.synapse.to/synapsedistro/unluac/unluac_native_new_may1919.exe"));
                    unluac_exe->get();

                    if (!CreateProcessW(CMDDir,
                        (LPWSTR) (std::wstring(L"/C unluac-new.exe ") + DecKeyW + L" \"" + DmpBytecodeName + L"\" > \"" + DecompName + L"\"").c_str(), NULL, NULL,
                        FALSE, CREATE_NO_WINDOW, NULL, BinDir.c_str(), &si, &pi))
                    {
                        std::filesystem::remove(DmpBytecodeName);
                        throw std::exception("failed to decompile: 0x03");
                    }
                }
                else
                {
                    syn::Resource* unluac_new_jar = new syn::Resource("unluac-new", BinDir + L"unluac-new.jar",
                        OBFUSCATE_STR("https://cdn.synapse.to/synapsedistro/unluac/unluac_new_may1919.jar"));
                    unluac_new_jar->get();

                    if (!CreateProcessW(CMDDir,
                        (LPWSTR) (std::wstring(L"/C java -Xss32M -XX:+DisableAttachMechanism -jar unluac-new.jar ") +
                            DecKeyW + L" \"" + DmpBytecodeName + L"\" > \"" + DecompName + L"\"").c_str(), NULL, NULL, FALSE,
                        CREATE_NO_WINDOW, NULL, NULL, &si, &pi))
                    {
                        std::filesystem::remove(DmpBytecodeName);
                        throw std::exception("failed to decompile: 0x04");
                    }
                }
            }

            /* Wait for decompilation to complete */
            auto WaitRes = WaitForSingleObject(pi.hProcess, Timeout);

			/* Check for timeout */
			if (WaitRes == WAIT_TIMEOUT)
			{
				/* Kill process tree (timed out) */
				KillProcessTree(pi.dwProcessId);

				/* Close handles */
				CloseHandle(pi.hProcess);
				CloseHandle(pi.hThread);

				/* Remove left over files */
				for (auto i = 0; i < 5; i++)
				{
					try
					{
						/* We have to wait for the file handles to close. */
						std::filesystem::remove(DecompName);
						std::filesystem::remove(DmpBytecodeName);
						break;
					}
					catch (std::exception&)
					{
						Sleep(1000);
					}
				}

				std::function<int(RbxLua)> ret([](RbxLua NRL)
					{
						/* Push to stack. */
						NRL.PushString(OBFUSCATE_STR("--SynapseX Decompiler\n--Decompiler timeout."));
						return 1;
					});
				return ret;
			}

            CloseHandle(pi.hProcess);
            CloseHandle(pi.hThread);

            /* Grab file contents */
            std::ifstream t(DecompName);
            std::string str((std::istreambuf_iterator<char>(t)),
                std::istreambuf_iterator<char>());
            t.close();

            /* Add watermark */
            str.insert(0, OBFUSCATE_STR("--SynapseX Decompiler\n\n"));

            /* Remove files */
            std::filesystem::remove(DecompName);
            std::filesystem::remove(DmpBytecodeName);

			std::function<int(RbxLua)> ret([str](RbxLua NRL)
				{
					/* Push to stack */
					NRL.PushLString(str.c_str(), str.size());

					return 1;
				});
			return ret;
        });
    }

	int RbxApi::dumpstring(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t ScriptLength;

        const auto ScriptPointer = RL.CheckLString(1, &ScriptLength);
		const auto Script = std::string(ScriptPointer, ScriptLength);

        if (Script.find("\033Lua") == 0)
            return RL.LError("bytecode is not supported");

		std::string BC;
		try
		{
#ifndef EnableHSVMOnlyLuaU
#ifdef EnableHSVM
			const auto NState = luaL_newstate();

			if (luaL_loadbuffer(NState, Script.c_str(), Script.size(), BS_LUA, ""))
			{
				/* Error while compiling, report back to translation caller */
				const std::string Err = lua_tostring(NState, -1);
				lua_close(NState);
				throw std::exception(Err.c_str());
			}

			luaL_Buffer B;
			luaL_buffinit(NState, &B);
			if (lua_dump(NState, syn::LuaTranslator::DumpWriter, &B) != 0) throw std::exception("failed to dump bytecode");
			luaL_pushresult(&B);
			size_t Len;
			const auto BCC = lua_tolstring(NState, -1, &Len);

			BC = std::string(BCC, Len);

			lua_close(NState);
#else
			auto Translator = syn::LuaTranslator::GetSingleton();
			Translator->ConvertInCurrentThread(RL, Script, 0);
			BC = Translator->Dump(RL);
#endif
#else
			if (IsLuaU)
			{
				const auto NState = luaL_newstate();

				if (luaL_loadbuffer(NState, Script.c_str(), Script.size(), BS_HSVM, ""))
				{
					/* Error while compiling, report back to translation caller */
					const std::string Err = lua_tostring(NState, -1);
					lua_close(NState);
					throw std::exception(Err.c_str());
				}

				luaL_Buffer B;
				luaL_buffinit(NState, &B);
				if (lua_dump(NState, syn::LuaTranslator::DumpWriter, &B) != 0) throw std::exception("failed to dump bytecode");
				luaL_pushresult(&B);
				size_t Len;
				const auto BCC = lua_tolstring(NState, -1, &Len);

				BC = std::string(BCC, Len);

				lua_close(NState);
			}
			else
			{
				auto Translator = syn::LuaTranslator::GetSingleton();
				Translator->ConvertInCurrentThread(RL, Script, 0);
				BC = Translator->Dump(RL);
			}
#endif
		}
		catch (const std::exception& ex)
		{
			throw std::exception(ex.what());
		}

		RL.PushLString(BC.c_str(), BC.size());

		return 1;
	}

	int RbxApi::httpget(DWORD rL)
	{
		syn::RbxLua RL(rL);
		RbxYield RYield(RL);

		size_t CSize;
		const auto CStr = RL.CheckLString(1, &CSize);
		const auto Url = std::string(CStr, CSize);

		if (Url.find("http"))
			return RL.ArgError(1, "Invalid protocol specified (expected 'http://' or 'https://')");

		return RYield.Execute([Url]()
		{
			auto result = cpr::Get(cpr::Url{ Url },
				cpr::Header{ {"User-Agent", "Roblox/WinInet"} });

			if (HttpStatus::isError(result.status_code))
			{
				auto Err = "Http Error " + std::to_string(result.status_code) + " - " + HttpStatus::reasonPhrase(
					result.status_code);

				throw std::exception(Err.c_str());
			}

			return [result](RbxLua NRL)
			{
				NRL.PushLString(result.text.c_str(), result.text.size());
				return 1;
			};
		});
	}

	int RbxApi::httppost(DWORD rL)
	{
		syn::RbxLua RL(rL);

		std::string Url = RL.CheckString(1);

		size_t DCSize;
		const auto DCStr = RL.CheckLString(2, &DCSize);
		const auto Data = std::string(DCStr, DCSize);

		auto ContentType = RL.CheckString(3);

		if (Url.find("http") != 0) throw std::exception("Invalid protocol specified (expected 'http://' or 'https://')");

		auto result = Post(cpr::Url{ Url },
			cpr::Header{ {"User-Agent", "Roblox/WinInet"} },
			cpr::Body{ Data },
			cpr::Header{ {"Content-Type", ContentType} });

		if (HttpStatus::isError(result.status_code))
		{
			auto Err = "Http Error " + std::to_string(result.status_code) + " - " + HttpStatus::reasonPhrase(
				result.status_code);

			throw std::exception(Err.c_str());
		}

		RL.PushLString(result.text.c_str(), result.text.size());

		return 1;
	}

	int RbxApi::httprequest(DWORD rL)
	{
		syn::RbxLua RL(rL);
		RbxYield RYield(RL);

		RL.CheckType(1, R_LUA_TTABLE);

		RL.GetField(1, "Url");
		if (RL.Type(-1) != R_LUA_TSTRING)
			return RL.LError("Invalid or no 'Url' field specified in request table");

		size_t UrlSize;
		const auto UrlCStr = RL.CheckLString(-1, &UrlSize);
		std::string Url(UrlCStr, UrlSize);

		if (Url.find("http://") != 0 && Url.find("https://") != 0)
			return RL.LError("Only 'http' and 'https' protocols are supported for the 'Url' field.");

		RL.Pop(1);

		enum RequestMethods
		{
			H_GET,
			H_HEAD,
			H_POST,
			H_PUT,
			H_DELETE,
			H_OPTIONS
		};

		std::map<std::string, RequestMethods> RequestMethodMap =
		{
			{ "get", H_GET },
			{ "head", H_HEAD },
			{ "post", H_POST },
			{ "put", H_PUT },
			{ "delete", H_DELETE },
			{ "options", H_OPTIONS }
		};

		auto Method = H_GET;

		RL.GetField(1, "Method");
		if (RL.Type(-1) == R_LUA_TSTRING)
		{
			std::string MethodS = RL.CheckString(-1);
			std::transform(MethodS.begin(), MethodS.end(), MethodS.begin(), tolower);

			if (!RequestMethodMap.count(MethodS))
				return RL.LError("Request type '%s' is not a valid http request type.", MethodS.c_str());

			Method = RequestMethodMap[MethodS];
		}

		RL.Pop(1);

		cpr::Header Headers;

		RL.GetField(1, "Headers");
		if (RL.Type(-1) == R_LUA_TTABLE)
		{
			RL.PushNil();

			while (RL.Next(-2))
			{
				if (RL.Type(-2) != R_LUA_TSTRING || RL.Type(-1) != R_LUA_TSTRING)
					return RL.LError("'Headers' table must contain string keys/values.");

				size_t HeaderKeySize;
				const auto HeaderKeyCStr = RL.CheckLString(-2, &HeaderKeySize);
				std::string HeaderKey(HeaderKeyCStr, HeaderKeySize);

				auto HeaderKeyCopy = std::string(HeaderKey);
				std::transform(HeaderKeyCopy.begin(), HeaderKeyCopy.end(), HeaderKeyCopy.begin(), tolower);

				if (HeaderKeyCopy == "content-length")
					return RL.LError("Headers: 'Content-Length' header cannot be overwritten.");

				size_t HeaderValueSize;
				const auto HeaderValueCStr = RL.CheckLString(-1, &HeaderValueSize);
				std::string HeaderValue(HeaderValueCStr, HeaderValueSize);

				Headers.insert({ HeaderKey, HeaderValue });

				RL.Pop(1);
			}
		}

		RL.Pop(1);

		cpr::Cookies Cookies;

		RL.GetField(1, "Cookies");
		if (RL.Type(-1) == R_LUA_TTABLE)
		{
			std::map<std::string, std::string> RCookies;

			RL.PushNil();

			while (RL.Next(-2))
			{
				if (RL.Type(-2) != R_LUA_TSTRING || RL.Type(-1) != R_LUA_TSTRING)
					return RL.LError("'Cookies' table must contain string keys/values.");

				size_t CookieKeySize;
				const auto CookieKeyCStr = RL.CheckLString(-2, &CookieKeySize);
				std::string CookieKey(CookieKeyCStr, CookieKeySize);

				size_t CookieValueSize;
				const auto CookieValueCStr = RL.CheckLString(-1, &CookieValueSize);
				std::string CookieValue(CookieValueCStr, CookieValueSize);

				RCookies[CookieKey] = CookieValue;

				RL.Pop(1);
			}

			Cookies = RCookies;
		}

		RL.Pop(1);

		auto HasUserAgent = false;
		for (auto& Header : Headers)
		{
			auto HeaderName = Header.first;
			std::transform(HeaderName.begin(), HeaderName.end(), HeaderName.begin(), tolower);

			if (HeaderName == "user-agent")
				HasUserAgent = true;
		}

		if (!HasUserAgent)
		{
			Headers.insert({ "User-Agent", std::string("synx/") + OBFUSCATE_STR(SYNAPSE_VERSION) });
		}

		std::string Body;
		RL.GetField(1, "Body");
		if (RL.Type(-1) == R_LUA_TSTRING)
		{
			if (Method == H_GET || Method == H_HEAD)
				return RL.LError("'Body' cannot be present in GET or HEAD requests.");

			size_t BodySize;
			const auto BodyCStr = RL.CheckLString(-1, &BodySize);
			Body = std::string(BodyCStr, BodySize);
		}

		RL.Pop(1);

		return RYield.Execute([Method, Url, Headers, Cookies, Body]()
		{
			cpr::Response Response;

			switch (Method)
			{
			case H_GET:
			{
				Response = cpr::Get(
					cpr::Url{ Url },
					Cookies,
					Headers
				);

				break;
			}

			case H_HEAD:
			{
				Response = cpr::Head(
					cpr::Url{ Url },
					Cookies,
					Headers
				);

				break;
			}

			case H_POST:
			{
				Response = cpr::Post(
					cpr::Url{ Url },
					cpr::Body{ Body },
					Cookies,
					Headers
				);

				break;
			}

			case H_PUT:
			{
				Response = cpr::Put(
					cpr::Url{ Url },
					cpr::Body{ Body },
					Cookies,
					Headers
				);

				break;
			}

			case H_DELETE:
			{
				Response = cpr::Delete(
					cpr::Url{ Url },
					cpr::Body{ Body },
					Cookies,
					Headers
				);

				break;
			}

			case H_OPTIONS:
			{
				Response = cpr::Options(
					cpr::Url{ Url },
					cpr::Body{ Body },
					Cookies,
					Headers
				);

				break;
			}

			default:
			{
				throw std::exception("invalid request type");
			}
			}

			return [Response](RbxLua NRL) 
			{
				NRL.NewTable();

				NRL.PushBoolean(HttpStatus::isSuccessful(Response.status_code));
				NRL.SetField(-2, "Success");

				NRL.PushInteger(Response.status_code);
				NRL.SetField(-2, "StatusCode");

				NRL.PushString(HttpStatus::reasonPhrase(Response.status_code).c_str());
				NRL.SetField(-2, "StatusMessage");

				NRL.NewTable();

				for (auto& Header : Response.header)
				{
					NRL.PushLString(Header.first.c_str(), Header.first.size());
					NRL.PushLString(Header.second.c_str(), Header.second.size());

					NRL.SetTable(-3);
				}

				NRL.SetField(-2, "Headers");

				NRL.NewTable();

				for (auto& Cookie : Response.cookies.map_)
				{
					NRL.PushLString(Cookie.first.c_str(), Cookie.first.size());
					NRL.PushLString(Cookie.second.c_str(), Cookie.second.size());

					NRL.SetTable(-3);
				}

				NRL.SetField(-2, "Cookies");

				NRL.PushLString(Response.text.c_str(), Response.text.size());
				NRL.SetField(-2, "Body");
				return 1;
			};
		});
	}

	int RbxApi::readfile(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t PathSize;
		const auto PathCStr = RL.CheckLString(1, &PathSize);
		auto Path = std::string(PathCStr, PathSize);

		std::replace(Path.begin(), Path.end(), '/', '\\');

		const std::string Extention = PathFindExtensionA(Path.c_str());

		if (Path.find("..") != std::string::npos)
            return RL.LError("attempt to escape directory");

		std::wstring WPath = WorkspaceDirectory + L"\\" + ConvertToWStr(Path);

		if (!std::filesystem::exists(WPath.c_str()))
            return RL.LError("file does not exist");

		std::ifstream Stream(WPath, std::ios_base::binary);
		std::string Final((std::istreambuf_iterator<char>(Stream)),
		                  std::istreambuf_iterator<char>());

		RL.PushLString(Final.c_str(), Final.size());

		return 1;
	}

	int RbxApi::loadfile(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t PathSize;
		const auto PathCStr = RL.CheckLString(1, &PathSize);
		auto Path = std::string(PathCStr, PathSize);

		std::replace(Path.begin(), Path.end(), '/', '\\');

		if (Path.find("..") != std::string::npos)
			return RL.LError("attempt to escape directory");

		const std::string Extention = PathFindExtensionA(Path.c_str());
		std::wstring WPath = WorkspaceDirectory + L"\\" + ConvertToWStr(Path);

		if (!std::filesystem::exists(WPath.c_str()))
            return RL.LError("file does not exist");

		std::ifstream Stream(WPath, std::ios_base::binary);
		std::string Final((std::istreambuf_iterator<char>(Stream)),
		                  std::istreambuf_iterator<char>());

		RL.GetGlobal("loadstring");
		RL.PushLString(Final.c_str(), Final.size());
		RL.PCall(1, 1, 0);

		return 1;
	}

    /* TODO: Abstract out base */
	int RbxApi::writefile(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t PathSize;
		const auto PathCStr = RL.CheckLString(1, &PathSize);
		auto Path = std::string(PathCStr, PathSize);

		size_t ContentsSize;
		const auto ContentsCStr = RL.CheckLString(2, &ContentsSize);

		std::replace(Path.begin(), Path.end(), '/', '\\');

		const std::string Extention = PathFindExtensionA(Path.c_str());

		std::vector<std::string> DisallowedExtensions =
		{
			".exe", ".scr", ".bat", ".com", ".csh", ".msi", ".vb", ".vbs", ".vbe", ".ws", ".wsf", ".wsh", ".ps1"
		};

		for (std::string& Test : DisallowedExtensions)
			if (equals_ignore_case(Extention, Test)) 
                return RL.LError("forbidden extension");

		if (Path.find("..") != std::string::npos)
            return RL.LError("attempt to escape directory");

		std::wstring WPath = WorkspaceDirectory + L"\\" + ConvertToWStr(Path);

		std::ofstream Out;
		Out.open(WPath, std::ios::out | std::ios::binary);
		Out.write(ContentsCStr, ContentsSize);
		Out.close();

		return 0;
	}

	int RbxApi::isfile(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t PathSize;
		const auto PathCStr = RL.CheckLString(1, &PathSize);
		auto Path = std::string(PathCStr, PathSize);

		if (Path.find("..") != std::string::npos)
            return RL.LError("attempt to escape directory");

		std::wstring WPath = WorkspaceDirectory + L"\\" + ConvertToWStr(Path);
		RL.PushBoolean(std::filesystem::is_regular_file(WPath));
		return 1;
	}

	int RbxApi::isfolder(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t PathSize;
		const auto PathCStr = RL.CheckLString(1, &PathSize);
		auto Path = std::string(PathCStr, PathSize);

		if (Path.find("..") != std::string::npos)
            return RL.LError("attempt to escape directory");

		std::wstring WPath = WorkspaceDirectory + L"\\" + ConvertToWStr(Path);
		RL.PushBoolean(std::filesystem::is_directory(WPath));
		return 1;
	}

	int RbxApi::listfiles(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t PathSize;
		const auto PathCStr = RL.CheckLString(1, &PathSize);
		auto Path = std::string(PathCStr, PathSize);

		if (Path.find("..") != std::string::npos)
            return RL.LError("attempt to escape directory");

		std::wstring WPath = WorkspaceDirectory + L"\\" + ConvertToWStr(Path);
		RL.NewTable();

		size_t c = 0;
		for (auto& p : std::filesystem::directory_iterator(WPath))
		{
			auto path = p.path().string().substr(WorkspaceDirectory.length() + 1);

			RL.PushInteger(++c);
			RL.PushString(path.c_str());
			RL.SetTable(-3);
		}

		return 1;
	}

	int RbxApi::makefolder(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t PathSize;
		const auto PathCStr = RL.CheckLString(1, &PathSize);
		auto Path = std::string(PathCStr, PathSize);

		if (Path.find("..") != std::string::npos)
            return RL.LError("attempt to escape directory");

		std::wstring WPath = WorkspaceDirectory + L"\\" + ConvertToWStr(Path);
		std::filesystem::create_directories(WPath);

		return 0;
	}

	int RbxApi::delfolder(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t PathSize;
		const auto PathCStr = RL.CheckLString(1, &PathSize);
		auto Path = std::string(PathCStr, PathSize);

		if (Path.find("..") != std::string::npos)
            return RL.LError("attempt to escape directory");

		std::wstring WPath = WorkspaceDirectory + L"\\" + ConvertToWStr(Path);

		if (!std::filesystem::remove_all(WPath))
            return RL.LError("folder does not exist");
		
		return 0;
	}

	int RbxApi::delfile(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t PathSize;
		const auto PathCStr = RL.CheckLString(1, &PathSize);
		auto Path = std::string(PathCStr, PathSize);

		if (Path.find("..") != std::string::npos)
            return RL.LError("attempt to escape directory");

		std::wstring WPath = WorkspaceDirectory + L"\\" + ConvertToWStr(Path);

		if (!std::filesystem::remove(WPath))
            return RL.LError("file does not exist");

		return 0;
	}

	int RbxApi::appendfile(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t PathSize;
		const auto PathCStr = RL.CheckLString(1, &PathSize);
		auto Path = std::string(PathCStr, PathSize);

		size_t ContentsSize;
		const auto ContentsCStr = RL.CheckLString(2, &ContentsSize);

		std::replace(Path.begin(), Path.end(), '/', '\\');

		const std::string Extention = PathFindExtensionA(Path.c_str());

		if (Path.find("..") != std::string::npos)
            return RL.LError("attempt to escape directory");

		std::wstring WPath = WorkspaceDirectory + L"\\" + ConvertToWStr(Path);

		std::vector<std::string> DisallowedExtensions = {
			".exe", ".bat", ".com", ".csh", ".msi", ".vb", ".vbs", ".vbe", ".ws", ".wsf", ".wsh", ".ps1"
		};
		for (std::string& Test : DisallowedExtensions)
		{
			if (equals_ignore_case(Extention, Test)) 
                return RL.LError("file does not exist");
		}

		if (!std::filesystem::exists(WPath.c_str()))
            return RL.LError("file does not exist");

		std::ofstream Out;
		Out.open(WPath, std::ios_base::app | std::ios_base::binary);
		Out.write(ContentsCStr, ContentsSize);
		Out.close();

		return 0;
	}

	int RbxApi::isbeta(DWORD rL)
	{
		syn::RbxLua RL(rL);

#ifdef EnableBetaRelease
		RL.PushBoolean(true);
#else
		RL.PushBoolean(false);
#endif

		return 1;
	}

	int RbxApi::isluau(DWORD rL)
	{
		syn::RbxLua RL(rL);

		RL.PushBoolean(IsLuaU);

		return 1;
	}

	int RbxApi::getnamecallmethod(DWORD rL)
	{
		const syn::RbxLua RL(rL);

		if (!IsLuaU)
			return RL.LError("'getnamecallmethod' is only supported on LuaU enabled games.");

		RL.PushRawObject(*(uintptr_t*) (rL + L_NAMECALLMETHOD), R_LUA_TSTRING);

		return 1;
	}

	int RbxApi::setnamecallmethod(DWORD rL)
	{
		const syn::RbxLua RL(rL);

		if (!IsLuaU)
			return RL.LError("'setnamecallmethod' is only supported on LuaU enabled games.");

		if (!RL.IsString(1))
			RL.TagError(1, R_LUA_TSTRING);

		*(uintptr_t*)(rL + L_NAMECALLMETHOD) = (uintptr_t) RL.Index2Adr(1)->value.p;

		return 0;
	}

	//Used in explorer.
	int RbxApi::getpropvalue(DWORD rL)
	{
		const syn::RbxLua RL(rL);

		RL.GetGlobal("tostring");
		RL.GetField(1, RL.ToString(2));
		RL.PCall(1, 1, 0);

		size_t CSize;
		const auto CStr = RL.ToLString(3, &CSize);

		RL.Pop(2);

		RL.PushLString(CStr, CSize);

		return 1;
	}	
	
	int RbxApi::setpropvalue(DWORD rL)
	{
		const syn::RbxLua RL(rL);

		RL.SetField(1, RL.ToString(2));

		return 0;
	}

	int RbxApi::getconnections(DWORD rL)
	{
		syn::RbxLua RL(rL);

		if (!RL.IsUserData(1) || !checksignal(RL, 1))
			return RL.ArgError(1, "signal expected");

		const auto EventInstance = *reinterpret_cast<std::uintptr_t*>(RL.ToUserData(1));
		const auto Source = reinterpret_cast<std::weak_ptr<uintptr_t>*>(EventInstance + 4)->lock();
		const auto Signal = reinterpret_cast<uintptr_t(__thiscall*)(uintptr_t*, bool)>(*reinterpret_cast<std::uintptr_t*>(EventInstance + 48))(Source.get(), true);
		//auto Current = *reinterpret_cast<std::uintptr_t*>(Signal);
		//
		//RL.NewTable();
		//
		//int Count = 1;
		//while (Current != 0)
		//{
		//	RL.PushNumber(Count++);
		//	RL.PushLightUserData(reinterpret_cast<void*>(Current));
		//	RL.SetTable(-3);
		//
		//	Current = *reinterpret_cast<std::uintptr_t*>(Current + 0x8);
		//}

		return 1;
	}

	int RbxApi::disableconnection(DWORD rL)
	{
		syn::RbxLua RL(rL);

        /* TODO: Check whether it is an actual syn signal */
		if (!RL.IsUserData(1))
			return RL.ArgError(1, "synapse signal expected");

		const auto Connection = reinterpret_cast<std::uintptr_t>(RL.ToUserData(-1));
		const auto Signal = *reinterpret_cast<std::uintptr_t*>(Connection + 0xC);

		auto Previous = 0;
		auto Current = *reinterpret_cast<std::uintptr_t*>(Signal);

		while (Current && Current != Connection)
		{
			Previous = Current;
			Current = *reinterpret_cast<std::uintptr_t*>(Current + 0x8);
		}

        if (!Current)
            return RL.LError("synapse signal is not connected");

		if (!Previous)
			*reinterpret_cast<std::uintptr_t*>(Signal) = 0;
		else
			*reinterpret_cast<std::uintptr_t*>(Previous + 0x8) = *reinterpret_cast<std::uintptr_t*>(Current + 0x8);

		RL.PushLightUserData(reinterpret_cast<void*>(Current));

		return 1;
	}

	int RbxApi::enableconnection(DWORD rL)
	{
		syn::RbxLua RL(rL);

        /* TODO: Check whether it is an actual syn signal */
        if (!RL.IsUserData(1))
            return RL.ArgError(1, "synapse signal expected");

		const auto Connection = reinterpret_cast<std::uintptr_t>(RL.ToUserData(-1));
		const auto Signal = *reinterpret_cast<std::uintptr_t*>(Connection + 0xC);

		auto Previous = 0;
		auto Current = *reinterpret_cast<std::uintptr_t*>(Signal);

		while (Current != 0)
		{
			Previous = Current;
			Current = *reinterpret_cast<std::uintptr_t*>(Current + 0x8);
		}

		if (!Previous)
			*reinterpret_cast<std::uintptr_t*>(Signal) = Connection;
		else
			*reinterpret_cast<std::uintptr_t*>(Previous + 0x8) = Connection;

		RL.PushLightUserData(reinterpret_cast<void*>(Current));

		return 1;
	}

	static DWORD GetConnectionTR(RbxLua RL, DWORD Conn)
	{
		DWORD Ret = 0;

		if (*(DWORD*)(Conn + 0x1C) == 0)
			Ret = *(DWORD*)(Conn + 0x20);

		else if (*(DWORD*)(Conn + 0x14) == 0)
			Ret = *(DWORD*)(Conn + 0x18);

		if (!Ret)
			return RL.LError("internal error [0x01]");

		return Ret;
	}

	int RbxApi::getconnectionfunc(DWORD rL)
	{
		RbxLua RL(rL);

        /* TODO: Check whether it is an actual syn signal */
        if (!RL.IsUserData(1))
            return RL.ArgError(1, "synapse signal expected");

        VM_FISH_LITE_START;
		DWORD Conn = (DWORD)RL.ToUserData(1);
		DWORD TR = GetConnectionTR(RL, Conn);

		DWORD NRL = *(DWORD*)(*(DWORD*)(TR + 0x38) + 0x8);
		DWORD Idx = *(DWORD*)(TR + 0x40);
        VM_FISH_LITE_END;

		RbxLua RLX(NRL);

		RLX.PushInteger(Idx);
		RLX.GetTable(LUA_REGISTRYINDEX);
		RLX.XMove(RL, 1);

		return 1;
	}

	int RbxApi::getconnectionstate(DWORD rL)
	{
		/*
			weakthreadref = signal + 0x18]
			node = signal + 0x18] + 0x38]
			luaState = signal + 0x18] + 0x38] + 0x8]
		*/

		RbxLua RL(rL);

        /* TODO: Check whether it is an actual syn signal */
        if (!RL.IsUserData(1))
            return RL.ArgError(1, "synapse signal expected");
        
        VM_FISH_LITE_START;
		DWORD Conn = (DWORD)RL.ToUserData(1);
		DWORD TR = GetConnectionTR(RL, Conn);
		DWORD NRL = *(DWORD*)(*(DWORD*)(TR + 0x38) + 0x8);
        VM_FISH_LITE_END;

		RL.PushLightUserData((void*)NRL);

		return 1;
	}

	int RbxApi::firesignal(DWORD rL)
	{
		syn::RbxLua RL(rL);

		if (!RL.IsUserData(1) || !checksignal(RL, 1))
			return RL.ArgError(1, "signal expected");

		VM_FISH_LITE_START;

		RL.GetField(1, OBFUSCATE_STR("Connect"));
		RL.PushValue(1);
		RL.PushCFunction(getconnectionshandler);
		RL.PCall(2, 1, 0);

		RL.Remove(1);

		const auto Connection = *reinterpret_cast<std::uintptr_t*>(reinterpret_cast<std::uintptr_t>(RL.ToUserData(-1)) + 4);
		const auto Signal = *reinterpret_cast<std::uintptr_t*>(Connection + 0xC);

		RL.GetField(-1, OBFUSCATE_STR("Disconnect"));
		RL.Insert(-2);
		RL.Insert(-2);
		RL.PCall(1, 0, 0);

		int ArgumentCount = RL.GetTop();

		auto Next = *reinterpret_cast<uintptr_t*>(Signal);
		while (Next)
		{
			DWORD TR = GetConnectionTR(RL, Next);
			DWORD NRL = *(DWORD*)(*(DWORD*)(TR + 0x38) + 0x8);
			DWORD Idx = *(DWORD*)(TR + 0x40);

			Next = *reinterpret_cast<uintptr_t*>(Next + 0x8);
			if (syn::PointerObfuscation::DeObfuscateGlobalState(NRL + L_GS) != syn::PointerObfuscation::DeObfuscateGlobalState(rL + L_GS))
				continue;

			RbxLua RLX(NRL);

			RLX.PushNumber(Idx);
			RLX.GetTable(LUA_REGISTRYINDEX);
			RLX.XMove(RL, 1);

			//Stack:
			//Arg1
			//Arg2
			//Arg3
			//[Function]

			for (int i = 1; i <= ArgumentCount; i++)
				RL.PushValue(i);

			RL.PCall(ArgumentCount, 0, 0);
		}

		VM_FISH_LITE_END;

		return 0;
	}

	template<typename T>
	__forceinline std::string EncryptWithAlgo(const syn::RbxLua RL, const std::string& Plaintext, const std::string& Key, const std::string& IV)
    {
		try
		{
			std::string Encrypted;

			T Encryptor;
			Encryptor.SetKeyWithIV((byte*) Key.c_str(), Key.size(), (byte*) IV.c_str(), IV.length());

			CryptoPP::StringSource ss(Plaintext, true,
				new CryptoPP::StreamTransformationFilter(Encryptor,
					new CryptoPP::StringSink(Encrypted)
				)
			);

			return Base64Encode((unsigned char*) Encrypted.c_str(), Encrypted.size());
		}
		catch (CryptoPP::Exception& e)
		{
			UNUSED(RL.LError(e.what()));
			return "";
		}
    }

	template<typename T>
	__forceinline std::string EncryptAuthenticatedWithAlgo(const syn::RbxLua RL, const std::string& Plaintext, const std::string& Key, const std::string& IV)
	{
		try
		{
			std::string Encrypted;

			T Encryptor;
			Encryptor.SetKeyWithIV((byte*) Key.c_str(), Key.size(), (byte*) IV.c_str(), IV.size());

			CryptoPP::AuthenticatedEncryptionFilter Aef(Encryptor,
				new CryptoPP::StringSink(Encrypted)
			);

			Aef.Put((const byte*)Plaintext.data(), Plaintext.size());
			Aef.MessageEnd();

			return Base64Encode((unsigned char*) Encrypted.c_str(), Encrypted.size());
		}
		catch (CryptoPP::Exception& e)
		{
			UNUSED(RL.LError(e.what()));
			return "";
		}
	}
	
	int RbxApi::encryptstringcustom(DWORD rL)
    {
		syn::RbxLua RL(rL);
		
        VM_TIGER_LONDON_START;

		std::string Algo = RL.CheckString(1);

		size_t DataSize;
		const auto DataCStr = RL.CheckLString(2, &DataSize);
		const std::string Plaintext(DataCStr, DataSize);

		size_t KeySize;
		const auto KeyCStr = RL.CheckLString(3, &KeySize);
		const std::string Key(KeyCStr, KeySize);
    	
    	size_t IvSize;
		const auto IvCStr = RL.CheckLString(4, &IvSize);
		const std::string Iv(IvCStr, IvSize);

		enum CryptModes
		{
			//AES
			AES_CBC,
			AES_CFB,
			AES_CTR,
			AES_OFB,
			AES_GCM,
			AES_EAX,

			//Blowfish
			BF_CBC,
			BF_CFB,
			BF_OFB
		};

		std::map<std::string, CryptModes> CryptTranslationMap =
		{
			//AES
			{ "aes-cbc", AES_CBC },
			{ "aes_cbc", AES_CBC },

			{ "aes-cfb", AES_CFB },
			{ "aes_cfb", AES_CFB },

			{ "aes-ctr", AES_CTR },
			{ "aes_ctr", AES_CTR },

			{ "aes-ofb", AES_OFB },
			{ "aes_ofb", AES_OFB },

			{ "aes-gcm", AES_GCM },
			{ "aes_gcm", AES_GCM },

			{ "aes-eax", AES_EAX },
			{ "aes_eax", AES_EAX },
			
			//Blowfish
			{ "blowfish-cbc", BF_CBC },
			{ "blowfish_cbc", BF_CBC },
			{ "bf-cbc", BF_CBC },
			{ "bf_cbc", BF_CBC },

			{ "blowfish-cfb", BF_CFB },
			{ "blowfish_cfb", BF_CFB },
			{ "bf-cfb", BF_CFB },
			{ "bf_cfb", BF_CFB },

			{ "blowfish-ofb", BF_OFB },
			{ "blowfish_ofb", BF_OFB },
			{ "bf-ofb", BF_OFB },
			{ "bf_ofb", BF_OFB },
		};

		std::transform(Algo.begin(), Algo.end(), Algo.begin(), tolower);

		if (!CryptTranslationMap.count(Algo))
			return RL.ArgError(1, "non-existant algorithm");

		const auto RAlgo = CryptTranslationMap[Algo];

		std::string Result;

		//This is intentional - blame Themida not supporting jump tables.
		if (RAlgo == AES_CBC)
		{
			Result = EncryptWithAlgo<CryptoPP::CBC_Mode<CryptoPP::AES>::Encryption>(RL, Plaintext, Key, Iv);
		}
		else if (RAlgo == AES_CFB)
		{
			Result = EncryptWithAlgo<CryptoPP::CBC_Mode<CryptoPP::AES>::Encryption>(RL, Plaintext, Key, Iv);
		}
		else if (RAlgo == AES_CTR)
		{
			Result = EncryptWithAlgo<CryptoPP::CTR_Mode<CryptoPP::AES>::Encryption>(RL, Plaintext, Key, Iv);
		}
		else if (RAlgo == AES_OFB)
		{
			Result = EncryptWithAlgo<CryptoPP::OFB_Mode<CryptoPP::AES>::Encryption>(RL, Plaintext, Key, Iv);
		}
		else if (RAlgo == AES_GCM)
		{
			Result = EncryptAuthenticatedWithAlgo<CryptoPP::GCM<CryptoPP::AES>::Encryption>(RL, Plaintext, Key, Iv);
		}
		else if (RAlgo == AES_EAX)
		{
			Result = EncryptAuthenticatedWithAlgo<CryptoPP::EAX<CryptoPP::AES>::Encryption>(RL, Plaintext, Key, Iv);
		}
		else if (RAlgo == BF_CBC)
		{
			Result = EncryptWithAlgo<CryptoPP::CBC_Mode<CryptoPP::Blowfish>::Encryption>(RL, Plaintext, Key, Iv);
		}
		else if (RAlgo == BF_CFB)
		{
			Result = EncryptWithAlgo<CryptoPP::CFB_Mode<CryptoPP::Blowfish>::Encryption>(RL, Plaintext, Key, Iv);
		}
		else if (RAlgo == BF_OFB)
		{
			Result = EncryptWithAlgo<CryptoPP::OFB_Mode<CryptoPP::Blowfish>::Encryption>(RL, Plaintext, Key, Iv);
		}
		else
		{
			return RL.ArgError(1, "non-existant algorithm");
		}

        VM_TIGER_LONDON_END;

		RL.PushLString(Result.c_str(), Result.size());
			
		return 1;
    }

	int RbxApi::encryptstring(DWORD rL)
	{
		syn::RbxLua RL(rL);

        VM_TIGER_WHITE_START;

		size_t DataSize;
		const auto DataCStr = RL.CheckLString(1, &DataSize);

		size_t KeySize;
		const auto KeyCStr = RL.CheckLString(2, &KeySize);

		CryptoPP::AutoSeededRandomPool Prng;
		byte IV[12];
		Prng.GenerateBlock(IV, 12);

		byte DerivedKey[32];
		CryptoPP::PKCS5_PBKDF2_HMAC<CryptoPP::SHA384> KDF;
		KDF.DeriveKey(DerivedKey, 32, 0, (byte*) KeyCStr, KeySize, NULL, 0, 10000);

		auto Encrypted = EncryptAuthenticatedWithAlgo<CryptoPP::GCM<CryptoPP::AES>::Encryption>(RL, 
			std::string(DataCStr, DataSize), 
			std::string((const char*) DerivedKey, 32), 
			std::string((const char*) IV, 12));

		Encrypted += "|" + Base64Encode(IV, 12);
		Encrypted = Base64Encode((byte*) Encrypted.data(), Encrypted.size());

		RL.PushLString(Encrypted.c_str(), Encrypted.size());

        VM_TIGER_WHITE_END;

		return 1;
	}

	template<typename T>
	__forceinline std::string DecryptWithAlgo(const syn::RbxLua RL, const std::string& Ciphertext, const std::string& Key, const std::string& IV)
	{
		try
		{
			std::string Decrypted;

			T Decryptor;
			Decryptor.SetKeyWithIV((byte*) Key.c_str(), Key.size(), (byte*) IV.c_str(), IV.length());

			const auto Base = Base64Decode(Ciphertext);

			CryptoPP::StringSource ss(Base, true,
				new CryptoPP::StreamTransformationFilter(Decryptor,
					new CryptoPP::StringSink(Decrypted)
				)
			);
			
			return Decrypted;
		}
		catch (CryptoPP::Exception& e)
		{
			UNUSED(RL.LError(e.what()));
			return "";
		}
	}

	template<typename T>
	__forceinline std::string DecryptAuthenticatedWithAlgo(const syn::RbxLua RL, const std::string& Ciphertext, const std::string& Key, const std::string& IV)
	{
		try
		{
			std::string Decrypted;

			T Decryptor;
			Decryptor.SetKeyWithIV((byte*) Key.c_str(), Key.size(), (byte*) IV.c_str(), IV.size());

			const auto Base = Base64Decode(Ciphertext);

			CryptoPP::AuthenticatedDecryptionFilter Adf(Decryptor,
				new CryptoPP::StringSink(Decrypted)
			);

			Adf.Put((const byte*)Base.data(), Base.size());
			Adf.MessageEnd();

			return Decrypted;
		}
		catch (CryptoPP::Exception& e)
		{
			UNUSED(RL.LError(e.what()));
			return "";
		}
	}
	
	int RbxApi::decryptstringcustom(DWORD rL)
	{
		syn::RbxLua RL(rL);

        VM_TIGER_LONDON_START;
    	
    	std::string Algo = RL.CheckString(1);

		size_t DataSize;
		const auto DataCStr = RL.CheckLString(2, &DataSize);
		const std::string Ciphertext(DataCStr, DataSize);

		size_t KeySize;
		const auto KeyCStr = RL.CheckLString(3, &KeySize);
		const std::string Key(KeyCStr, KeySize);

		size_t IvSize;
		const auto IvCStr = RL.CheckLString(4, &IvSize);
		const std::string Iv(IvCStr, IvSize);

		enum CryptModes
		{
			//AES
			AES_CBC,
			AES_CFB,
			AES_CTR,
			AES_OFB,
			AES_GCM,
			AES_EAX,

			//Blowfish
			BF_CBC,
			BF_CFB,
			BF_OFB
		};

		std::map<std::string, CryptModes> CryptTranslationMap =
		{
			//AES
			{ "aes-cbc", AES_CBC },
			{ "aes_cbc", AES_CBC },

			{ "aes-cfb", AES_CFB },
			{ "aes_cfb", AES_CFB },

			{ "aes-ctr", AES_CTR },
			{ "aes_ctr", AES_CTR },

			{ "aes-ofb", AES_OFB },
			{ "aes_ofb", AES_OFB },

			{ "aes-gcm", AES_GCM },
			{ "aes_gcm", AES_GCM },

			{ "aes-eax", AES_EAX },
			{ "aes_eax", AES_EAX },

			//Blowfish
			{ "blowfish-cbc", BF_CBC },
			{ "blowfish_cbc", BF_CBC },
			{ "bf-cbc", BF_CBC },
			{ "bf_cbc", BF_CBC },

			{ "blowfish-cfb", BF_CFB },
			{ "blowfish_cfb", BF_CFB },
			{ "bf-cfb", BF_CFB },
			{ "bf_cfb", BF_CFB },

			{ "blowfish-ofb", BF_OFB },
			{ "blowfish_ofb", BF_OFB },
			{ "bf-ofb", BF_OFB },
			{ "bf_ofb", BF_OFB },
		};

		std::transform(Algo.begin(), Algo.end(), Algo.begin(), tolower);

		if (!CryptTranslationMap.count(Algo))
			return RL.ArgError(1, "non-existant algorithm");

		const auto RAlgo = CryptTranslationMap[Algo];

		std::string Result;

		//This is intentional - blame Themida not supporting jump tables.
		if (RAlgo == AES_CBC)
		{
			Result = DecryptWithAlgo<CryptoPP::CBC_Mode<CryptoPP::AES>::Decryption>(RL, Ciphertext, Key, Iv);
		}
		else if (RAlgo == AES_CFB)
		{
			Result = DecryptWithAlgo<CryptoPP::CFB_Mode<CryptoPP::AES>::Decryption>(RL, Ciphertext, Key, Iv);
		}
		else if (RAlgo == AES_CTR)
		{
			Result = DecryptWithAlgo<CryptoPP::CTR_Mode<CryptoPP::AES>::Decryption>(RL, Ciphertext, Key, Iv);
		}
		else if (RAlgo == AES_OFB)
		{
			Result = DecryptWithAlgo<CryptoPP::OFB_Mode<CryptoPP::AES>::Decryption>(RL, Ciphertext, Key, Iv);
		}
		else if (RAlgo == AES_GCM)
		{
			Result = DecryptAuthenticatedWithAlgo<CryptoPP::GCM<CryptoPP::AES>::Decryption>(RL, Ciphertext, Key, Iv);
		}
		else if (RAlgo == AES_EAX)
		{
			Result = DecryptAuthenticatedWithAlgo<CryptoPP::EAX<CryptoPP::AES>::Decryption>(RL, Ciphertext, Key, Iv);
		}
		else if (RAlgo == BF_CBC)
		{
			Result = DecryptWithAlgo<CryptoPP::CBC_Mode<CryptoPP::Blowfish>::Decryption>(RL, Ciphertext, Key, Iv);
		}
		else if (RAlgo == BF_CFB)
		{
			Result = DecryptWithAlgo<CryptoPP::CFB_Mode<CryptoPP::Blowfish>::Decryption>(RL, Ciphertext, Key, Iv);
		}
		else if (RAlgo == BF_OFB)
		{
			Result = DecryptWithAlgo<CryptoPP::OFB_Mode<CryptoPP::Blowfish>::Decryption>(RL, Ciphertext, Key, Iv);
		}
		else
		{
			return RL.ArgError(1, "non-existent algorithm");
		}

        VM_TIGER_LONDON_END;
    	
    	RL.PushLString(Result.c_str(), Result.size());

		return 1;
	}

	int RbxApi::decryptstring(DWORD rL)
	{
		syn::RbxLua RL(rL);

        VM_TIGER_WHITE_START;

		size_t DataSize;
		const auto DataCStr = RL.CheckLString(1, &DataSize);

		size_t KeySize;
		const auto KeyCStr = RL.CheckLString(2, &KeySize);

		byte DerivedKey[32];
		CryptoPP::PKCS5_PBKDF2_HMAC<CryptoPP::SHA384> KDF;
		KDF.DeriveKey(DerivedKey, 32, 0, (byte*) KeyCStr, KeySize, NULL, 0, 10000);

		std::vector<std::string> Split;
		SplitString(Base64Decode(std::string(DataCStr, DataSize)), "|", Split);

		if (Split.size() != 2)
			return RL.ArgError(1, "Invalid encrypted string specified");

		auto Decrypted = DecryptAuthenticatedWithAlgo<CryptoPP::GCM<CryptoPP::AES>::Decryption>(RL,
			Split.at(0),
			std::string((const char*) DerivedKey, 32),
			Base64Decode(Split.at(1)));

		RL.PushLString(Decrypted.c_str(), Decrypted.size());

        VM_TIGER_WHITE_END;

		return 1;
	}

	char XProtectRepChar(const char Input)
	{
		switch (Input)
		{
			case 53:
				return 65;
			case 52:
				return 66;
			case 51:
				return 67;
			case 55:
				return 68;
			case 50:
				return 69;
			case 57:
				return 70;
			case 56:
				return 71;
			case 48:
				return 72;
			case 49:
				return 73;
			case 54:
				return 74;
			case 65:
				return 75;
			case 66:
				return 76;
			case 67:
				return 77;
			case 69:
				return 78;
			case 68:
				return 79;
			case 71:
				return 80;
			case 70:
				return 81;
			case 72:
				return 82;
			case 73:
				return 83;
			case 74:
				return 84;
			case 75:
				return 85;
			case 76:
				return 86;
			case 78:
				return 87;
			case 77:
				return 88;
			case 80:
				return 89;
			case 79:
				return 90;
			case 45:
				return 45;
			case 82:
				return 49;
			case 83:
				return 50;
			case 84:
				return 51;
			case 85:
				return 52;
			case 86:
				return 53;
			case 87:
				return 54;
			case 88:
				return 55;
			case 89:
				return 56;
			case 90:
				return 57;
			case 81:
				return 48;
			case 100:
				return 97;
			case 97:
				return 98;
			case 101:
				return 99;
			case 98:
				return 100;
			case 122:
				return 101;
			case 120:
				return 102;
			case 112:
				return 103;
			case 114:
				return 104;
			case 115:
				return 105;
			case 116:
				return 106;
			case 111:
				return 107;
			case 113:
				return 108;
			case 105:
				return 109;
			case 117:
				return 110;
			case 119:
				return 111;
			case 121:
				return 112;
			case 118:
				return 113;
			case 109:
				return 114;
			case 110:
				return 115;
			case 107:
				return 116;
			case 108:
				return 117;
			case 99:
				return 118;
			case 102:
				return 119;
			case 103:
				return 120;
			case 104:
				return 121;
			case 106:
				return 122;
			default:
				return Input;
		}
	}

	__forceinline std::string XProtectDecryptNew(syn::RbxLua RL, std::string Data)
	{
		std::string OutputReal;

		for (auto& I : Data)
		{
			I = XProtectRepChar(I);
		}

		const auto OutputAes = DecryptWithAlgo<CryptoPP::CFB_Mode<CryptoPP::AES>::Decryption>(RL, Data, OBFUSCATE_STR_TEA("gN1gFQXXoOB3DMy3DzJyECZ8LDAIPc3D"), OBFUSCATE_STR_TEA("ACUCcF3GDFffLC4f"));
		return DecryptWithAlgo<CryptoPP::CFB_Mode<CryptoPP::Blowfish>::Decryption>(RL, OutputAes, OBFUSCATE_STR_TEA("QXXogN1gFOB3DMy3xDAIPX4nzJynCZ8L"), OBFUSCATE_STR_TEA("3dCUGc2G"));
	}

	int RbxApi::xprotect(DWORD rL)
	{
		syn::RbxLua RL(rL);

        VM_TIGER_LONDON_START;
		
		const std::string Data = RL.CheckString(1);

		auto Output = XProtectDecryptNew(RL, Data);
		if (Output.find(OBFUSCATE_STR("2340@4$CdfHFSK(#)")) == std::string::npos)
		{
			//Decrypt with old XProtect
			Output = DecryptWithAlgo<CryptoPP::CFB_Mode<CryptoPP::Blowfish>::Decryption>(RL, Data, OBFUSCATE_STR_TEA("CtyqqJ9Ci4vDstSe43Dfs6DlldOhFcvu"), OBFUSCATE_STR_TEA("2R0G7s7k"));
		}

		ReplaceAll(Output, OBFUSCATE_STR("if (isreadonly(game) == false) then return end"), "");

		auto Schedule = syn::Scheduler::GetSingleton();
		Schedule->Push(Output);

        VM_TIGER_LONDON_END;
		
		return 0;
	}

	template<typename T>
	__forceinline std::string HashWithAlgo(const std::string& Input)
	{
		T Hash;
		std::string Digest;

		CryptoPP::StringSource SS(Input, true,
			new CryptoPP::HashFilter(Hash,
				new CryptoPP::HexEncoder(
					new CryptoPP::StringSink(Digest), false
				)));

		return Digest;
	}

	int RbxApi::hashstring(DWORD rL)
	{
		syn::RbxLua RL(rL);

        VM_TIGER_WHITE_START;

		size_t DataSize;
		const auto DataCStr = RL.CheckLString(1, &DataSize);

		auto Hash = HashWithAlgo<CryptoPP::SHA384>(std::string(DataCStr, DataSize));

		RL.PushLString(Hash.c_str(), Hash.size());

        VM_TIGER_WHITE_END;

		return 1;
	}

	int RbxApi::hashstringcustom(DWORD rL)
    {
		syn::RbxLua RL(rL);

        VM_TIGER_WHITE_START;
    	
    	std::string Algo = RL.CheckString(1);

    	size_t DataSize;
		const auto DataCStr = RL.CheckLString(2, &DataSize);
		const std::string Data(DataCStr, DataSize);

		enum HashModes
		{
			//MD5
			MD5,

			//SHA1
			SHA1,

			//SHA2
			SHA224,
			SHA256,
			SHA384,
			SHA512,

			//SHA3
			SHA3_256,
			SHA3_384,
			SHA3_512,
		};

		std::map<std::string, HashModes> HashTranslationMap =
		{
			//MD5
			{ "md5", MD5 },

			//SHA1
			{ "sha1", SHA1 },

			//SHA2
			{ "sha224", SHA224 },
			{ "sha256", SHA256 },
			{ "sha384", SHA384 },
			{ "sha512", SHA512 },

			//SHA3
			{ "sha3-256", SHA3_256 },
			{ "sha3_256", SHA3_256 },
			{ "sha3-384", SHA3_384 },
			{ "sha3_384", SHA3_384 },
			{ "sha3-512", SHA3_512 },
			{ "sha3_512", SHA3_512 },
		};

		std::transform(Algo.begin(), Algo.end(), Algo.begin(), tolower);

		if (!HashTranslationMap.count(Algo))
			return RL.ArgError(1, "non-existant hash algorithm");

		const auto RAlgo = HashTranslationMap[Algo];

		std::string Hash;
		
		//This is intentional - blame Themida not supporting jump tables.
		if (RAlgo == MD5)
		{
			Hash = HashWithAlgo<CryptoPP::Weak::MD5>(Data);
		}
		else if (RAlgo == SHA1)
		{
			Hash = HashWithAlgo<CryptoPP::SHA1>(Data);
		}
		else if (RAlgo == SHA224)
		{
			Hash = HashWithAlgo<CryptoPP::SHA224>(Data);
		}
		else if (RAlgo == SHA256)
		{
			Hash = HashWithAlgo<CryptoPP::SHA256>(Data);
		}
		else if (RAlgo == SHA384)
		{
			Hash = HashWithAlgo<CryptoPP::SHA384>(Data);
		}
		else if (RAlgo == SHA512)
		{
			Hash = HashWithAlgo<CryptoPP::SHA512>(Data);
		}
		else if (RAlgo == SHA3_256)
		{
			Hash = HashWithAlgo<CryptoPP::SHA3_256>(Data);
		}
		else if (RAlgo == SHA3_384)
		{
			Hash = HashWithAlgo<CryptoPP::SHA3_384>(Data);
		}
		else if (RAlgo == SHA3_512)
		{
			Hash = HashWithAlgo<CryptoPP::SHA3_512>(Data);
		}
		else
		{
			return RL.ArgError(1, "non-existant hash algorithm");
		}

		RL.PushLString(Hash.c_str(), Hash.size());

        VM_TIGER_WHITE_END;

		return 1;
    }

	int RbxApi::randomstring(DWORD rL)
    {
		syn::RbxLua RL(rL);

        VM_TIGER_WHITE_START;
    	
		const auto Size = RL.CheckInt(1);

		if (Size > 1024)
			return RL.ArgError(1, "exceeded maximum size (1024)");

		if (Size < 0)
			return RL.ArgError(1, "negative size specified");

		CryptoPP::AutoSeededRandomPool Prng;
		auto Alloc = (byte*) operator new(Size);
		Prng.GenerateBlock(Alloc, Size);

		RL.PushLString((const char*) Alloc, Size);

		delete Alloc;

        VM_TIGER_WHITE_END;
    	
    	return 1;
    }

	int RbxApi::derivestring(DWORD rL)
	{
		syn::RbxLua RL(rL);

        VM_TIGER_WHITE_START;
    	
    	size_t DataSize;
		const auto DataCStr = RL.CheckLString(1, &DataSize);
    	
    	const auto Size = RL.CheckInt(2);

		if (Size > 1024)
			return RL.ArgError(2, "exceeded maximum size (1024)");

		if (Size < 0)
			return RL.ArgError(2, "negative size specified");

		auto Alloc = (byte*) operator new(Size);
		CryptoPP::PKCS5_PBKDF2_HMAC<CryptoPP::SHA384> KDF;
		KDF.DeriveKey(Alloc, Size, 0, (byte*) DataCStr, DataSize, NULL, 0, 10000);

		RL.PushLString((const char*) Alloc, Size);

		delete Alloc;

        VM_TIGER_WHITE_END;
    	
    	return 1;
	}

	int RbxApi::base64encode(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t DataSize;
		const auto DataCStr = RL.CheckLString(1, &DataSize);

		auto Encoded = Base64Encode((unsigned char*) DataCStr, DataSize);

		RL.PushLString(Encoded.c_str(), Encoded.size());

		return 1;
	}

	int RbxApi::base64decode(DWORD rL)
	{
		syn::RbxLua RL(rL);

		size_t DataSize;
		const auto DataCStr = RL.CheckLString(1, &DataSize);

		auto Decoded = Base64Decode(std::string(DataCStr, DataSize));

		RL.PushLString(Decoded.c_str(), Decoded.size());

		return 1;
	}

	int RbxApi::securelua_gethwid(DWORD rL)
	{
		VM_TIGER_LONDON_START

		const syn::RbxLua RL(rL);
		RL.PushString(syn::HWID.c_str());

		VM_TIGER_LONDON_END

		return 1;
	}

	int RbxApi::securelua_randomstring(DWORD rL)
	{
		VM_TIGER_LONDON_START

		const syn::RbxLua RL(rL);
		RL.PushString(RandomString(RL.CheckInteger(1)).c_str());

		VM_TIGER_LONDON_END

		return 1;
	}

	int RbxApi::securelua_httpget(DWORD rL)
	{
		VM_TIGER_LONDON_START

		const syn::RbxLua RL(rL);
		const RbxYield RYield(RL);

		size_t CSize;
		const auto CStr = RL.CheckLString(1, &CSize);
		const auto Url = std::string(CStr, CSize);

		if (Url.find(OBFUSCATE_STR("http")))
			return RL.ArgError(1, OBFUSCATE_STR("Invalid protocol specified (expected 'http://' or 'https://')"));

		if (syn::AntiProxy::Check())
			CrashRoblox();
		
		VM_TIGER_LONDON_END

		return RYield.Execute([Url]()
		{
			const auto Result = cpr::Get(cpr::Url{ Url },
				cpr::Header{ { OBFUSCATE_STR("User-Agent"), std::string(OBFUSCATE_STR("synx/")) + OBFUSCATE_STR(SYNAPSE_VERSION) } });

			if (HttpStatus::isError(Result.status_code))
			{
				const auto Err = OBFUSCATE_STR("Http Error ") + std::to_string(Result.status_code) + " - " + HttpStatus::reasonPhrase(
					Result.status_code);

				throw std::exception(Err.c_str());
			}

			return [Result](RbxLua NRL) 
			{
				NRL.PushLString(Result.text.c_str(), Result.text.size());
				return 1;
			};
		});
	}

	int RbxApi::createrenderobject(DWORD rL)
	{
		syn::RbxLua RL(rL);

		const std::string Type = RL.CheckString(1);

		/* syn::D3D::Initialize checks if its already init'd, we don't need to add a check here */
		syn::D3D::Initialize();

		auto D3D = syn::D3D::GetSingleton();

		if (Type == "Line")
		{
			auto Line = new D3DLine();
			Line->Color = ImGui::GetColorU32(ImVec4(0, 0, 0, 255));
			Line->From = ImVec2(0, 0);
			Line->To = ImVec2(0, 0);
			Line->Thickness = 0;

			auto Obj = new D3DObject();
			Obj->Header.Type = D3_LINE;
			Obj->Header.Visible = FALSE;
			Obj->Ptr = (DWORD*)Line;

			D3D->AddToRenderList(Obj);

			RL.PushLightUserData(Obj);

			return 1;
		}

		if (Type == "Text")
		{
			auto Text = new D3DText();
			Text->Color = ImGui::GetColorU32(ImVec4(0, 0, 0, 255));
			Text->OutlineColor = ImGui::GetColorU32(ImVec4(0, 0, 0, 255));
			Text->Center = FALSE;
			Text->Outline = FALSE;
			Text->Pos = ImVec2(0, 0);
			Text->Size = 16;
			Text->Font = 0;
			Text->Text = new std::string("");

			auto Obj = new D3DObject();
			Obj->Header.Type = D3_TEXT;
			Obj->Header.Visible = FALSE;
			Obj->Ptr = (DWORD*)Text;

			D3D->AddToRenderList(Obj);

			RL.PushLightUserData(Obj);

			return 1;
		}

		if (Type == "Square")
		{
			auto Square = new D3DSquare();
			Square->Color = ImGui::GetColorU32(ImVec4(0, 0, 0, 255));
			Square->Filled = FALSE;
			Square->Thickness = 16;
			Square->Pos = ImVec2(0, 0);
			Square->Size = ImVec2(16, 16);

			auto Obj = new D3DObject();
			Obj->Header.Type = D3_SQUARE;
			Obj->Header.Visible = FALSE;
			Obj->Ptr = (DWORD*)Square;

			D3D->AddToRenderList(Obj);

			RL.PushLightUserData(Obj);

			return 1;
		}

		if (Type == "Circle")
		{
			auto Circle = new D3DCircle();
			Circle->Color = ImGui::GetColorU32(ImVec4(0, 0, 0, 255));
			Circle->Filled = FALSE;
			Circle->Thickness = 16;
			Circle->Pos = ImVec2(0, 0);
			Circle->Radius = 1;
			Circle->Sides = 100;

			auto Obj = new D3DObject();
			Obj->Header.Type = D3_CIRCLE;
			Obj->Header.Visible = FALSE;
			Obj->Ptr = (DWORD*)Circle;

			D3D->AddToRenderList(Obj);

			RL.PushLightUserData(Obj);

			return 1;
		}

		return RL.LError("type does not exist");
	}

	int RbxApi::setrenderproperty(DWORD rL)
	{
		syn::RbxLua RL(rL);

        /* TODO: Check if its an actual render object */
		if (!RL.IsUserData(1))
			return RL.ArgError(1, "render object expected");

        const std::string Property = RL.CheckString(2);
        RL.CheckAny(3);

		const auto Obj = (D3DObject*)RL.ToUserData(1);

		const std::function<ImVec2()> GetVec2 = [=]
		{
			RL.GetField(3, "X");
			const float X = (float)RL.ToNumber(-1);
			RL.Pop(1);
			RL.GetField(3, "Y");
			const float Y = (float)RL.ToNumber(-1);
			RL.Pop(1);

			return ImVec2(X, Y);
		};

		const std::function<ImU32(ImU32)> GetColor = [=](ImU32 Old)
		{
			RL.GetField(3, "r");
			const float R = (float)RL.ToNumber(-1) * 255;
			RL.Pop(1);
			RL.GetField(3, "g");
			const float G = (float)RL.ToNumber(-1) * 255;
			RL.Pop(1);
			RL.GetField(3, "b");
			const float B = (float)RL.ToNumber(-1) * 255;
			RL.Pop(1);

			return ImGui::GetColorU32(ImVec4(R / 255, G / 255, B / 255, ImGui::ColorConvertU32ToFloat4(Old).w));
		};

		if (Property == "Visible")
		{
			Obj->Header.Visible = RL.ToBoolean(3);

			return 0;
		}

		switch (Obj->Header.Type)
		{
			case D3_LINE:
			{
				auto Line = (D3DLine*)Obj->Ptr;

				if (Property == "From")
				{
					Line->From = GetVec2();
					return 0;
				}

				if (Property == "To")
				{
					Line->To = GetVec2();
					return 0;
				}

				if (Property == "Color")
				{
					Line->Color = GetColor(Line->Color);
					return 0;
				}

				if (Property == "Thickness")
				{
					Line->Thickness = (float)RL.ToNumber(3);
					return 0;
				}

				if (Property == "Transparency")
				{
					auto Col = ImGui::ColorConvertU32ToFloat4(Line->Color);
					Col.w = (float)RL.ToNumber(3);
					Line->Color = ImGui::GetColorU32(Col);
					return 0;
				}

				return RL.LError("invalid property for line");
			}

			case D3_TEXT:
			{
				auto Text = (D3DText*)Obj->Ptr;

				if (Property == "Text")
				{
					size_t TextSize;
					const auto TextCStr = RL.ToLString(3, &TextSize);

					delete Text->Text;
					Text->Text = new std::string(TextCStr, TextSize);
					return 0;
				}

				if (Property == "Position")
				{
					Text->Pos = GetVec2();
					return 0;
				}

				if (Property == "Size")
				{
					Text->Size = (float)RL.ToNumber(3);
					return 0;
				}

				if (Property == "Font")
				{
					Text->Font = RL.ToInteger(3);
					return 0;
				}

				if (Property == "Color")
				{
					Text->Color = GetColor(Text->Color);
					return 0;
				}

				if (Property == "Center")
				{
					Text->Center = RL.ToBoolean(3);
					return 0;
				}

				if (Property == "Outline")
				{
					Text->Outline = RL.ToBoolean(3);
					return 0;
				}

				if (Property == "OutlineColor")
				{
					Text->OutlineColor = GetColor(Text->OutlineColor);
					return 0;
				}

				if (Property == "Transparency")
				{
                    ImVec4 Col = ImGui::ColorConvertU32ToFloat4(Text->Color);
					ImVec4 ColOutline = ImGui::ColorConvertU32ToFloat4(Text->OutlineColor);
                    float Amt = (float)RL.ToNumber(3);

					Col.w = Amt;
					ColOutline.w = Amt;
					Text->Color = ImGui::GetColorU32(Col);
					Text->OutlineColor = ImGui::GetColorU32(ColOutline);

					return 0;
				}

				if (Property == "TextBounds")
				{
                    return RL.LError("TextBounds is a read only property for text");
				}

                return RL.LError("invalid property for text");
			}

			case D3_SQUARE:
			{
				auto Square = (D3DSquare*)Obj->Ptr;

				if (Property == "Position")
				{
					Square->Pos = GetVec2();
					return 0;
				}

				if (Property == "Size")
				{
					Square->Size = GetVec2();
					return 0;
				}

				if (Property == "Color")
				{
					Square->Color = GetColor(Square->Color);
					return 0;
				}

				if (Property == "Thickness")
				{
					Square->Thickness = (float)RL.ToNumber(3);
					return 0;
				}

				if (Property == "Filled")
				{
					Square->Filled = RL.ToBoolean(3);
					return 0;
				}

				if (Property == "Transparency")
				{
					auto Col = ImGui::ColorConvertU32ToFloat4(Square->Color);
					Col.w = (float)RL.ToNumber(3);
					Square->Color = ImGui::GetColorU32(Col);
					return 0;
				}

                return RL.LError("invalid property for square");
			}

			case D3_CIRCLE:
			{
				auto Circle = (D3DCircle*)Obj->Ptr;

				if (Property == "Position")
				{
					Circle->Pos = GetVec2();
					return 0;
				}

				if (Property == "Radius")
				{
					Circle->Radius = (float)RL.ToNumber(3);
					return 0;
				}

				if (Property == "Color")
				{
					Circle->Color = GetColor(Circle->Color);
					return 0;
				}

				if (Property == "Thickness")
				{
					Circle->Thickness = (float)RL.ToNumber(3);
					return 0;
				}

				if (Property == "Filled")
				{
					Circle->Filled = RL.ToBoolean(3);
					return 0;
				}

				if (Property == "Transparency")
				{
					auto Col = ImGui::ColorConvertU32ToFloat4(Circle->Color);
					Col.w = (float)RL.ToNumber(3);
					Circle->Color = ImGui::GetColorU32(Col);
					return 0;
				}

				if (Property == "NumSides")
				{
					Circle->Sides = RL.ToInteger(3);
					return 0;
				}

                return RL.LError("invalid property for circle");
			}
		}

        return RL.LError("can't find object");
	}

	int RbxApi::getrenderproperty(DWORD rL)
	{
		syn::RbxLua RL(rL);

        /* TODO: Check if its an actual render object */
        if (!RL.IsUserData(1))
            return RL.ArgError(1, "render object expected");
		
        const std::string Property = RL.CheckString(2);

		const auto Obj = (D3DObject*)RL.ToUserData(1);

		const std::function<int(ImVec2)> PushVec2 = [=](ImVec2 Vec)
		{
			RL.GetGlobal("Vector2");
			RL.GetField(-1, "new");
			RL.PushNumber(Vec.x);
			RL.PushNumber(Vec.y);
			RL.PCall(2, 1, 0);

			return 1;
		};

		const std::function<int(ImU32)> PushColor = [=](ImU32 Col)
		{
			const auto ColObj = ImGui::ColorConvertU32ToFloat4(Col);

			RL.GetGlobal("Color3");
			RL.GetField(-1, "fromRGB");
			RL.PushNumber(ColObj.x);
			RL.PushNumber(ColObj.y);
			RL.PushNumber(ColObj.z);
			RL.PCall(3, 1, 0);

			return 1;
		};

		if (Property == "Visible")
		{
			RL.PushBoolean(Obj->Header.Visible);
			return 1;
		}

		switch (Obj->Header.Type)
		{
			case D3_LINE:
			{
				const auto Line = (D3DLine*)Obj->Ptr;

				if (Property == "From")
				{
					return PushVec2(Line->From);
				}

				if (Property == "To")
				{
					return PushVec2(Line->To);
				}

				if (Property == "Color")
				{
					return PushColor(Line->Color);
				}

				if (Property == "Thickness")
				{
					RL.PushNumber(Line->Thickness);
					return 1;
				}

				if (Property == "Transparency")
				{
					const auto Col = ImGui::ColorConvertU32ToFloat4(Line->Color);
					RL.PushNumber(Col.w);
					return 1;
				}

				throw std::exception("invalid property for line");
			}

			case D3_TEXT:
			{
				const auto Text = (D3DText*)Obj->Ptr;

				if (Property == "Text")
				{
					RL.PushLString(Text->Text->c_str(), Text->Text->length());
					return 1;
				}

				if (Property == "Position")
				{
					return PushVec2(Text->Pos);
				}

				if (Property == "Size")
				{
					RL.PushNumber(Text->Size);
					return 1;
				}

				if (Property == "Font")
				{
					RL.PushNumber(Text->Font);
					return 1;
				}

				if (Property == "Color")
				{
					return PushColor(Text->Color);
				}

				if (Property == "Center")
				{
					RL.PushBoolean(Text->Center);
					return 1;
				}

				if (Property == "Outline")
				{
					RL.PushBoolean(Text->Outline);
					return 1;
				}

				if (Property == "OutlineColor")
				{
					return PushColor(Text->OutlineColor);
				}

				if (Property == "Transparency")
				{
					const auto Col = ImGui::ColorConvertU32ToFloat4(Text->Color);
					RL.PushNumber(Col.w);
					return 1;
				}

				if (Property == "TextBounds")
				{
					const auto TextSize = syn::D3D::GetSingleton()->GetFont(Text->Font)->CalcTextSizeA(
						Text->Size, FLT_MAX, 0.0f, Text->Text->c_str());
					return PushVec2(TextSize);
				}

                return RL.LError("invalid property for text");
			}

			case D3_SQUARE:
			{
				const auto Square = (D3DSquare*)Obj->Ptr;

				if (Property == "Position")
				{
					return PushVec2(Square->Pos);
				}

				if (Property == "Size")
				{
					return PushVec2(Square->Size);
				}

				if (Property == "Color")
				{
					return PushColor(Square->Color);
				}

				if (Property == "Thickness")
				{
					RL.PushNumber(Square->Thickness);
					return 1;
				}

				if (Property == "Filled")
				{
					RL.PushBoolean(Square->Filled);
					return 1;
				}

				if (Property == "Transparency")
				{
					const auto Col = ImGui::ColorConvertU32ToFloat4(Square->Color);
					RL.PushNumber(Col.w);
					return 1;
				}

                return RL.LError("invalid property for square");
			}

			case D3_CIRCLE:
			{
				const auto Circle = (D3DCircle*)Obj->Ptr;

				if (Property == "Position")
				{
					return PushVec2(Circle->Pos);
				}

				if (Property == "Radius")
				{
					RL.PushNumber(Circle->Radius);
					return 1;
				}

				if (Property == "Color")
				{
					return PushColor(Circle->Color);
				}

				if (Property == "Thickness")
				{
					RL.PushNumber(Circle->Thickness);
					return 1;
				}

				if (Property == "Filled")
				{
					RL.PushBoolean(Circle->Filled);
					return 1;
				}

				if (Property == "Transparency")
				{
					const auto Col = ImGui::ColorConvertU32ToFloat4(Circle->Color);
					RL.PushNumber(Col.w);
					return 1;
				}

				if (Property == "NumSides")
				{
					RL.PushNumber(Circle->Sides);
					return 1;
				}

                return RL.LError("invalid property for circle");
			}
		}

        return RL.LError("cant find object");
	}

	int RbxApi::destroyrenderobject(DWORD rL)
	{
		syn::RbxLua RL(rL);

        /* TODO: Check if its an actual render object */
        if (!RL.IsUserData(1))
            return RL.ArgError(1, "render object expected");

		const auto Obj = (D3DObject*)RL.ToUserData(1);

		auto D3D = syn::D3D::GetSingleton();

		D3D->RemoveFromRenderList(Obj);

		switch (Obj->Header.Type)
		{
			case D3_LINE:
			{
				delete (D3DLine*)Obj->Ptr;
				break;
			}
			case D3_TEXT:
			{
				const auto Text = (D3DText*)Obj->Ptr;
				delete Text->Text;
				delete Text;
				break;
			}
			case D3_CIRCLE:
			{
				delete (D3DCircle*)Obj->Ptr;
				break;
			}
			case D3_SQUARE:
			{
				delete (D3DSquare*)Obj->Ptr;
				break;
			}
		}

		delete Obj;

		return 0;
	}

	int RbxApi::isredirectionenabled(DWORD rL)
	{
		syn::RbxLua RL(rL);

		RL.PushBoolean(OutputRedirection);

		return 1;
	}

	int RbxApi::printconsole(DWORD rL)
	{
		syn::RbxLua RL(rL);

		if (RL.Type(1) != R_LUA_TSTRING)
			throw std::exception("expected string as argument #1");

		auto Color = 0xFFFFFFFF;

		if (RL.GetTop() >= 2)
		{
            float r = static_cast<float>(RL.CheckNumber(2));
            float g = static_cast<float>(RL.CheckNumber(3));
            float b = static_cast<float>(RL.CheckNumber(4));

			Color = ImGui::GetColorU32(ImVec4(r / 255, g / 255, b / 255, 1));
		}

		size_t TextSize;
		const auto TextCStr = RL.ToLString(1, &TextSize);

		ConsoleOutput.emplace_back(std::make_pair(std::string(TextCStr, TextSize), Color));

		return 1;
	}

    /* TODO: clean up this function */
	int RbxApi::getspecialinfo(DWORD rL)
	{
		syn::RbxLua RL(rL);

        if (!RL.IsUserData(1) || !checkinstance(RL, 1))
            return RL.ArgError(1, "Variant<userdata[MeshPart, UnionOperation, Terrain]> expected");

		const syn::Instance Inst = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(1));

		RL.NewTable();

		if (Inst.GetInstanceClassName() == "MeshPart")
		{
			auto PhyData = *(std::string*)(Inst + 0xE0);
			RL.PushLString(PhyData.c_str(), PhyData.size());
			RL.SetField(-2, "PhysicsData");
			const auto InitSize = *(RVector3*)(Inst + 0x100);
			RL.GetGlobal("Vector3");
			RL.GetField(-1, "new");
			RL.PushNumber(InitSize.x);
			RL.PushNumber(InitSize.y);
			RL.PushNumber(InitSize.z);
			RL.PCall(3, 1, 0);
			RL.SetField(-3, "InitialSize");
			RL.Pop(1);
		}
		else if (Inst.GetInstanceClassName() == "UnionOperation")
		{
			auto AssetId = *(std::string*)(Inst + 0x180);
			RL.PushLString(AssetId.c_str(), AssetId.size());
			RL.SetField(-2, "AssetId");
			auto ChildData = *(std::string*)(Inst + 0x138);
			RL.PushLString(ChildData.c_str(), ChildData.size());
			RL.SetField(-2, "ChildData");
			RL.PushNumber(*(DWORD*)(Inst + 0x178));
			RL.SetField(-2, "FormFactor");
			const auto InitSize = *(RVector3*)(Inst + 0x100);
			RL.GetGlobal("Vector3");
			RL.GetField(-1, "new");
			RL.PushNumber(InitSize.x);
			RL.PushNumber(InitSize.y);
			RL.PushNumber(InitSize.z);
			RL.PCall(3, 1, 0);
			RL.SetField(-3, "InitialSize");
			RL.Pop(1);
			auto MeshData = *(std::string*)(Inst + 0x150);
			RL.PushLString(MeshData.c_str(), MeshData.size());
			RL.SetField(-2, "MeshData");
			auto PhysicsData = *(std::string*)(Inst + 0xE0);
			RL.PushLString(PhysicsData.c_str(), PhysicsData.size());
			RL.SetField(-2, "PhysicsData");
		}
		else if (Inst.GetInstanceClassName() == "Terrain")
		{
			static DWORD ReadSmoothGrid = NULL;
			if (!ReadSmoothGrid) ReadSmoothGrid = RbxLua::GetBinValue(FNVA1_CONSTEXPR("readsmoothgrid"));

			static DWORD ReadMaterialColors = NULL;
			if (!ReadMaterialColors) ReadMaterialColors = RbxLua::GetBinValue(FNVA1_CONSTEXPR("readmaterialcolors"));

			const auto SmoothGrid = new std::string();
			const auto MaterialColors = new std::string();
			((int(__thiscall*)(int, std::string*))ReadSmoothGrid)(Inst, SmoothGrid);
			RL.PushLString(SmoothGrid->c_str(), SmoothGrid->size());
			RL.SetField(-2, "SmoothGrid");
			((int(__thiscall*)(int, std::string*))ReadMaterialColors)(Inst, MaterialColors);
			RL.PushLString(MaterialColors->c_str(), MaterialColors->size());
			RL.SetField(-2, "MaterialColors");
			delete SmoothGrid;
			delete MaterialColors;
		}

		return 1;
	}

	int RbxApi::cacheinvalidate(DWORD rL)
	{
		syn::RbxLua RL(rL);

        if (!RL.IsUserData(1) || !checkinstance(RL, 1))
            return RL.ArgError(1, "userdata<instance> expected");

		static DWORD PushF = NULL;
		if (!PushF) PushF = RbxLua::GetBinValue(FNVA1_CONSTEXPR("pushinstance"));

		DWORD Inst = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(1));
		RL.Pop(2);

		RL.PushLightUserData((void*)PushF);
		RL.GetTable(LUA_REGISTRYINDEX);
		RL.PushLightUserData((void*)Inst);
		RL.PushNil();
		RL.SetTable(-3);
		return 0;
	}

	int RbxApi::cachereplace(DWORD rL)
	{
		syn::RbxLua RL(rL);

        if (!RL.IsUserData(1) || !checkinstance(RL, 1))
            return RL.ArgError(1, "userdata<instance> expected");

        if (!RL.IsUserData(2) || !checkinstance(RL, 2))
            return RL.ArgError(2, "userdata<instance> expected");

		static DWORD PushF = NULL;
		if (!PushF) PushF = RbxLua::GetBinValue(FNVA1_CONSTEXPR("pushinstance"));

		DWORD Inst = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(1));

		RL.PushLightUserData((void*)PushF);
		RL.GetTable(LUA_REGISTRYINDEX);
		RL.PushLightUserData((void*)Inst);
		RL.PushValue(2);
		RL.SetTable(-3);
		return 0;
	}

	int RbxApi::iscache(DWORD rL)
	{
		syn::RbxLua RL(rL);

        if (!RL.IsUserData(1) || !checkinstance(RL, 1))
            return RL.ArgError(1, "userdata<instance> expected");

		static DWORD PushF = NULL;
		if (!PushF) PushF = RbxLua::GetBinValue(FNVA1_CONSTEXPR("pushinstance"));

		DWORD Inst = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(1));

		RL.PushLightUserData((void*)PushF);
		RL.GetTable(LUA_REGISTRYINDEX);
		RL.PushLightUserData((void*)Inst);
		RL.GetTable(-2);

        RL.PushBoolean(!RL.IsNil(-1));

		return 1;
	}

	int RbxApi::setndm(DWORD rL)
	{
		syn::RbxLua RL(rL);

        if (!RL.IsUserData(1) || !checkinstance(RL, 1))
            return RL.ArgError(1, "userdata<instance> expected");

        lua_Integer val = RL.CheckInteger(2);

		const DWORD Inst = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(1));
		*(DWORD*)(Inst + DM_NET) = val;

		return 0;
	}

	int RbxApi::getndm(DWORD rL)
	{
		syn::RbxLua RL(rL);

        if (!RL.IsUserData(1) || !checkinstance(RL, 1))
            return RL.ArgError(1, "userdata<instance> expected");

		const DWORD Inst = DereferenceSmartPointerInstance((DWORD) RL.ToUserData(1));
		RL.PushNumber(*(DWORD*)(Inst + DM_NET));

		return 1;
	}

	void RbxApi::reportkick_agent(const std::string& PlaceId, const std::string& Message)
	{
        VM_TIGER_LONDON_START;
		
		static auto Debounce = FALSE;
		if (Debounce)
			return;

		Debounce = TRUE;

		auto Encoded = Base64Encode((unsigned char*)Message.c_str(), Message.size());

		auto Id = RandomString(16);

		const auto Response = Post(cpr::Url{ OBFUSCATE_STR("https://synapse.to/whitelist/reportkick") },
			cpr::Header{
				{OBFUSCATE_STR("R"), sha512(CLIENT_REQ_KEY + HWID + Encoded + PlaceId + Id)}
			},
			cpr::Payload
			{
				{
					{"a", HWID},
					{"b", Encoded},
					{"c", PlaceId},
					{"d", Id}
				}
			});

        VM_TIGER_LONDON_END;
	}

	int RbxApi::reportkick(DWORD rL)
	{
		syn::RbxLua RL(rL);

        VM_TIGER_WHITE_START;

		static auto Debounce = FALSE;
		if (Debounce)
			return 0;

		if (!RL.IsString(1) || !RL.IsNumber(2))
			return 0;

		size_t KickSize;
		const auto KickCStr = RL.ToLString(1, &KickSize);
		auto Kick = std::string(KickCStr, KickSize);

		auto PlaceId = std::to_string((__int64)RL.ToNumber(2));

		std::thread(reportkick_agent, PlaceId, Kick).detach();

		Debounce = TRUE;

        VM_TIGER_WHITE_END;

		return 0;
	}

	void RbxApi::siprun_agent(const std::string& PlaceId)
	{
        VM_TIGER_WHITE_START;

		const auto Response = Get(cpr::Url{ OBFUSCATE_STR("https://synapse.to/whitelist/getsipscript") },
			cpr::Header{ {OBFUSCATE_STR("R"), sha512(CLIENT_REQ_KEY + HWID + PlaceId)} },
			cpr::Parameters
			{
				{
					{"a", HWID},
					{"b", PlaceId},
				}
			});

		if (Response.text == "NONE") return;

		auto Scheduler = syn::Scheduler::GetSingleton();
		Scheduler->Push(Response.text);

        VM_TIGER_WHITE_END;
	}

	int RbxApi::siprun(DWORD rL)
	{
		syn::RbxLua RL(rL);

        VM_TIGER_WHITE_START;

		if (RL.Type(1) != R_LUA_TNUMBER)
			return 0;

		auto PlaceId = std::to_string((__int64)RL.ToNumber(2));

		std::thread(siprun_agent, PlaceId).detach();

        VM_TIGER_WHITE_END;

		return 0;
	}

	int RbxApi::bitbswap(DWORD rL)
	{
		syn::RbxLua RL(rL);
		UBits b = barg(RL, 1);
		b = b >> 24 | b >> 8 & 0xff00 | (b & 0xff00) << 8 | b << 24;
		BRET(b)
	}

	int RbxApi::bittohex(DWORD rL)
	{
		syn::RbxLua RL(rL);
		UBits b = barg(RL, 1);
		SBits n = RL.Type(2) == LUA_TNONE ? 8 : (SBits)barg(RL, 2);
		const char* hexdigits = "0123456789abcdef";
		char buf[8];
		if (n < 0)
		{
			n = -n;
			hexdigits = "0123456789ABCDEF";
		}
		if (n > 8) n = 8;
		for (int i = (int)n; --i >= 0;)
		{
			buf[i] = hexdigits[b & 15];
			b >>= 4;
		}
		RL.PushLString(buf, (size_t)n);
		return 1;
	}

	volatile bool MBoxOpen = false;
	int RbxApi::messageboxasync(DWORD rL)
	{
		RbxLua RL(rL);
		RbxYield RYield(rL);

		const char* text = RL.CheckString(1);
		const char* caption = RL.CheckString(2);
		UINT type = RL.CheckInteger(3);

		return RYield.Execute([text, caption, type]()
		{
			while (MBoxOpen)
				Sleep(50);

			MBoxOpen = true;
			int result = MessageBoxA(NULL, text, caption, type);
			MBoxOpen = false;

			return [result](RbxLua NRL)
			{
				NRL.PushNumber(result);
				return 1;
			};
		});
	}

	int RbxApi::setconsolename(DWORD rL)
	{
		syn::Console::GetSingleton();

		syn::RbxLua RL(rL);

        const char* title = RL.CheckString(1);

		SetConsoleTitleA(title);

		return 0;
	}

	int RbxApi::consoleinputasync(DWORD rL)
	{
		syn::Console::GetSingleton();

		RbxYield RYield(rL);

		return RYield.Execute([]()
		{
			std::string out;
			std::getline(std::cin, out);

			return [out](RbxLua RL)
			{
				RL.PushString(out.c_str());
				return 1;
			};
		});
	}

	int RbxApi::consoleprint(DWORD rL)
	{
		syn::RbxLua RL(rL);

        const char* output = RL.CheckString(1);
		*syn::Console::GetSingleton() << output;

		return 0;
	}

	int RbxApi::consoleclear(DWORD rL)
	{
		system("cls");
		return 0;
	}

	int RbxApi::consoleinfo(DWORD rL)
	{
		syn::RbxLua RL(rL);

        const char* output = RL.CheckString(1);

		syn::Console::GetSingleton()->Info(output);
		return 0;
	}

	int RbxApi::consolewarn(DWORD rL)
	{
		syn::RbxLua RL(rL);

        const char* output = RL.CheckString(1);

		syn::Console::GetSingleton()->Warning(output);
		return 0;
	}

	int RbxApi::consoleerr(DWORD rL)
	{
		syn::RbxLua RL(rL);

        const char* output = RL.CheckString(1);

		syn::Console::GetSingleton()->Error(output);
		return 0;
	}

	int RbxApi::fireclickdetector(DWORD rL)
	{
		RbxLua RL(rL);

        if (!RL.IsUserData(1) || !checkinstance(RL, 1))
            return RL.ArgError(1, "userdata<ClickDetector> expected");

		syn::Instance CDectector = DereferenceSmartPointerInstance((DWORD)RL.ToUserData(1));

		if (CDectector.GetInstanceClassName() != "ClickDetector")
            return RL.ArgError(1, "userdata<ClickDetector> expected");

		if (!RL.IsNoneOrNil(2) && !RL.IsNumber(2))
            return RL.ArgError(2, "Variant<none, number> expected");

		float Dist = 0.0;
		if (RL.IsNumber(2))
			Dist = (float)RL.ToNumber(2);

		RL.GetGlobal("game");
		RL.GetField(-1, "GetService");
		RL.Insert(-2);
		RL.PushString("Players");
		RL.PCall(2, 1, 0);
		RL.GetField(-1, "LocalPlayer");

        VM_TIGER_WHITE_START;

		DWORD Plr = DereferenceSmartPointerInstance((DWORD)RL.ToUserData(-1));
		static DWORD FnFire = syn::RobloxBase(OBFUSCATED_NUM_UNCACHE(syn::Offsets::ClickDetector::FireClick));

		((void(__thiscall*)(DWORD, float, DWORD))FnFire)(CDectector, Dist, Plr);

        VM_TIGER_WHITE_END;

		return 0;
	}

	int RbxApi::crash(DWORD rL)
	{
		RbxLua RL(rL);
		if (RL.Type(1) == R_LUA_TSTRING)
			MessageBoxA(NULL, RL.ToString(1), "Synapse", MB_OK);

		((DWORD(__cdecl*)())nullptr)();
		return 0;
	}

	int RbxApi::firetouch(DWORD rL) 
	{
		RbxLua RL(rL);

        VM_TIGER_WHITE_START;

		if (!RL.IsUserData(1) || !checkinstance(RL, 1))
			return RL.ArgError(1, OBFUSCATE_STR("userdata<Instance> expected"));

		if (!RL.IsUserData(2) || !checkinstance(RL, 2))
			return RL.ArgError(2, OBFUSCATE_STR("userdata<Instance> expected"));

		if (!RL.IsNumber(3))
			return RL.ArgError(3, OBFUSCATE_STR("number expected"));

		if (RL.ToNumber(3) >= 2 || RL.ToNumber(3) < 0)
			return RL.ArgError(3, OBFUSCATE_STR("invalid number specified (0 or 1 expected)"));

		std::shared_ptr<DWORD> pi1 = *(std::shared_ptr<DWORD>*)((DWORD)RL.ToUserData(1) + 4);
		std::shared_ptr<DWORD> pi2 = *(std::shared_ptr<DWORD>*)((DWORD)RL.ToUserData(2) + 4);
						  
		typedef enum { Touch, Untouch } Type;

		Type typ = (Type) (DWORD) RL.ToNumber(3);

		struct TouchInfo
		{
			void* p1;
			void* p2;
			std::shared_ptr<DWORD> pi1;
			std::shared_ptr<DWORD> pi2;
			Type type;
			DWORD unk;
		};

		DWORD Workspace = syn::Instance(syn::DataModel).GetChildFromClassName("Workspace");
		DWORD World = *(DWORD*)(Workspace + OBFUSCATED_NUM_UNCACHE(272));
		DWORD Touches = World + OBFUSCATED_NUM_UNCACHE(32);

		TouchInfo Info1 = { nullptr, nullptr, pi1, pi2, typ, 1};
		TouchInfo Info2 = { nullptr, nullptr, pi2, pi1, typ, 1 };

		const auto Append = (void(__thiscall*)(DWORD, void*))syn::RobloxBase(OBFUSCATED_NUM_UNCACHE(Offsets::TouchInterestArray::AppendArray));

		Append(Touches, &Info1);
		Append(Touches, &Info2);
        
        VM_TIGER_WHITE_END;

		return 0;
	}

	int getcallstack(DWORD rL) 
	{
		RbxLua RL(rL);
		if (RL.Type(1) != R_LUA_TLIGHTUSERDATA)
			throw std::exception("expected thread as argument #1");

		DWORD UD = (DWORD)RL.ToUserData(1);
		if (*(BYTE*)(UD + GCO_TT) != R_LUA_TTHREAD)
			throw std::exception("expected thread as argument #1");

		DWORD CI = *(DWORD*)(UD + L_BCI);
		DWORD TCI = *(DWORD*)(UD + L_CI);

		RL.NewTable();
		
		DWORD Idx = 1;

		do
		{
			CI += 24;

			RL.PushNumber(Idx++);
			RL.PushRawObject(**(DWORD**)(CI + CI_FUNC), R_LUA_TFUNCTION);
			RL.SetTable(-3);
		} while (CI < TCI);

		return 1;
	}

#define WrapGlobal(func, name) \
        RL.PushCFunction(func); \
        RL.SetGlobal(OBFUSCATE_STR(name))

#define WrapMember(func, name) \
        RL.PushCFunction(func); \
        RL.SetField(-2, OBFUSCATE_STR(name))

#define WrapGlobalTable(name, f) \
        RL.NewTable(); \
        f \
        RL.SetGlobal(OBFUSCATE_STR(name))

#define WrapMemberTable(name, f) \
        RL.NewTable(); \
        f \
        RL.SetField(-2, OBFUSCATE_STR(name));

#define WrapExistingGlobal(name) \
        RL.GetGlobal(OBFUSCATE_STR(name)); \
        RL.SetField(-2, OBFUSCATE_STR(name))

#define WrapExistingMember(table, name) \
		RL.GetGlobal(table); \
		RL.GetField(-1, OBFUSCATE_STR(name)); \
		RL.Remove(-2); \
		RL.SetField(-2, OBFUSCATE_STR(name))

#define LockTable() *(BYTE*)(RL.ToPointer(-1) + RT_LOCKED) = TRUE

	bool RbxApi::PushLibraries(RbxLua RL, DWORD ORL)
	{
        VM_TIGER_WHITE_START;

        WrapGlobal(loadstring, "loadstring");

        WrapGlobal(getrawmetatable, "getrawmetatable");
        WrapGlobal(setrawmetatable, "setrawmetatable");

        WrapGlobal(setreadonly, "setreadonly");
        WrapGlobal(make_writeable, "make_writeable");
        WrapGlobal(make_readonly, "make_readonly");
        WrapGlobal(isreadonly, "isreadonly");

        WrapGlobal(checkcaller, "checkcaller");
        WrapGlobal(checkcaller, "is_protosmasher_caller");

        WrapGlobal(getreg, "getreg");

        WrapGlobal(getgenv, "getgenv");
        WrapGlobal(getrenv, "getrenv");

        WrapGlobal(isrbxactive, "isrbxactive");
        WrapGlobal(isrbxactive, "validfgwindow");

        WrapGlobal(getinfo, "getinfo");
        WrapGlobal(getstack, "getstack");
        WrapGlobal(setstack, "setstack");
        WrapGlobal(getupvalues, "getupvalues");
        WrapGlobal(getupvalue, "getupvalue");
        WrapGlobal(setupvalue, "setupvalue");
        WrapGlobal(getlocals, "getlocals");
        WrapGlobal(getlocal, "getlocal");
        WrapGlobal(setlocal, "setlocal");
        WrapGlobal(getconstants, "getconstants");
        WrapGlobal(getconstant, "getconstant");
        WrapGlobal(setconstant, "setconstant");

        WrapGlobal(decompile, "decompile");
        WrapGlobal(dumpstring, "dumpstring");

        WrapGlobal(newcclosure, "newcclosure");

        WrapGlobal(hookfunction, "hookfunction");

        WrapGlobal(setndm, "setndm");
        WrapGlobal(getndm, "getndm");

        WrapGlobal(isredirectionenabled, "is_redirection_enabled");

        WrapGlobal(printconsole, "printconsole");

        WrapGlobal(issynfunc, "is_synapse_function");
        WrapGlobal(issynfunc, "is_protosmasher_closure");

        WrapGlobal(getinstancelist, "getinstancelist");
        WrapGlobal(getstates, "getstates");
        WrapGlobal(getinstancefromstate, "getinstancefromstate");
		WrapGlobal(getpointerfromstate, "getpointerfromstate");
		WrapGlobal(getstateenv, "getstateenv");
		WrapGlobal(getcallstack, "getcallstack");

        WrapGlobal(getloadedmodules, "getloadedmodules");
        WrapGlobal(getloadedmodules, "get_loaded_modules");

        WrapGlobal(getcallingscript, "getcallingscript");
        WrapGlobal(getcallingscript, "get_calling_script");

        WrapGlobal(getconnections, "getconnections");
        WrapGlobal(getconnectionfunc, "getconnectionfunc");
        WrapGlobal(getconnectionstate, "getconnectionstate");

        WrapGlobal(firesignal, "firesignal");

        WrapGlobal(getspecialinfo, "getspecialinfo");

#ifdef EnableDebugOutput
            WrapGlobal(getnativefunc, "getnativefunc");

            WrapGlobal(crashrbx, "crashrbx");

            WrapGlobal(showinstance, "showinstance");

			WrapGlobal(decompilesala, "decompilesala");
#endif

#ifdef EnableLuaUTranslator
			WrapGlobal(luaudump, "luaudump");
#endif

		WrapGlobal(getpropvalue, "getpropvalue");

		WrapGlobal(setpropvalue, "setpropvalue");

        //WrapGlobal(firemessageout, "firemessageout");

        //WrapGlobal(createmsgoutstring, "createmsgoutstring");

        WrapGlobal(disableconnection, "disableconnection");
        WrapGlobal(enableconnection, "enableconnection");

        WrapGlobal(getgc, "getgc");

        WrapGlobal(getsenv, "getsenv");
        WrapGlobal(getsenv, "getmenv");

        WrapGlobal(islclosure, "islclosure");
        WrapGlobal(islclosure, "is_lclosure");

		WrapGlobal(isluau, "isluau");

		WrapGlobal(getnamecallmethod, "getnamecallmethod");
		WrapGlobal(setnamecallmethod, "setnamecallmethod");

        WrapGlobal(mouse1click, "mouse1click");
        WrapGlobal(mouse1press, "mouse1press");
        WrapGlobal(mouse1release, "mouse1release");
        WrapGlobal(mouse2click, "mouse2click");
        WrapGlobal(mouse2press, "mouse2press");
        WrapGlobal(mouse2release, "mouse2release");
        WrapGlobal(keypress, "keypress");
        WrapGlobal(keyrelease, "keyrelease");
        WrapGlobal(mousemoverel, "mousemoverel");
        WrapGlobal(mousemoveabs, "mousemoveabs");
        WrapGlobal(mousescroll, "mousescroll");

        WrapGlobal(readfile, "readfile");
        WrapGlobal(writefile, "writefile");
        WrapGlobal(listfiles, "listfiles");
        WrapGlobal(isfile, "isfile");
        WrapGlobal(isfolder, "isfolder");
        WrapGlobal(makefolder, "makefolder");
        WrapGlobal(delfolder, "delfolder");
        WrapGlobal(delfile, "delfile");
        WrapGlobal(loadfile, "loadfile");
        WrapGlobal(appendfile, "appendfile");

        WrapGlobal(checkrbxlocked, "checkrbxlocked");
        WrapGlobal(checkinst, "checkinst");
        WrapGlobal(checkparentchain, "checkparentchain");

        WrapGlobal(getclassname, "getclassname");

		WrapGlobal(xprotect, "XPROTECT");

        WrapGlobal(reportkick, "reportkick");

        WrapGlobal(createrenderobject, "createrenderobject");
        WrapGlobal(setrenderproperty, "setrenderproperty");
        WrapGlobal(getrenderproperty, "getrenderproperty");
        WrapGlobal(destroyrenderobject, "destroyrenderobject");

        WrapGlobal(messageboxasync, "messagebox");
        WrapGlobal(messageboxasync, "messageboxasync");

        WrapGlobal(setconsolename, "rconsolename");
        WrapGlobal(consoleinputasync, "rconsoleinput");
        WrapGlobal(consoleinputasync, "rconsoleinputasync");
        WrapGlobal(consoleprint, "rconsoleprint");
		WrapGlobal(consoleclear, "rconsoleclear");
        WrapGlobal(consoleinfo, "rconsoleinfo");
        WrapGlobal(consolewarn, "rconsolewarn");
        WrapGlobal(consoleerr, "rconsoleerr");

        WrapGlobal(fireclickdetector, "fireclickdetector");
		WrapGlobal(firetouch, "firetouchinterest");

        WrapGlobalTable("_G", (void));
        WrapGlobalTable("shared", (void));

		/* initialise syn.* library */
        WrapGlobalTable("syn",

            WrapMemberTable("crypt",
                WrapMember(encryptstring, "encrypt");
                WrapMember(decryptstring, "decrypt");
                WrapMember(hashstring, "hash");
				WrapMember(randomstring, "random");
				WrapMember(derivestring, "derive");

				WrapMemberTable("custom",
					WrapMember(encryptstringcustom, "encrypt");
					WrapMember(decryptstringcustom, "decrypt");
					WrapMember(hashstringcustom, "hash");
				);

                WrapMemberTable("base64",
                    WrapMember(base64encode, "encode");
                    WrapMember(base64decode, "decode");
                );
                   
                /* Copy legacy table */
                /* TODO: Add to initscript */
                RL.PushValue(-1);
                RL.SetField(-3, "crypto");
            );

            WrapGlobal(setclipboard, "setclipboard");
            WrapMember(setclipboard, "write_clipboard");

            WrapMember(getidentity, "get_thread_identity");
            WrapMember(setidentity, "set_thread_identity");

			WrapMember(httprequest, "request");

			WrapMember(isbeta, "is_beta");

            WrapMember(cachereplace, "cache_replace");
            WrapMember(cacheinvalidate, "cache_invalidate");
            WrapMember(iscache, "is_cached");

            LockTable();
        );

		/* initialise debug.* library */
        WrapGlobalTable("debug",

            WrapExistingMember("debug", "profilebegin");
            WrapExistingMember("debug", "profileend");
            WrapExistingMember("debug", "traceback");

            WrapExistingGlobal("getfenv");
            WrapMember(getinfo, "getinfo");
            WrapMember(getstack, "getstack");
            WrapMember(setstack, "setstack");
            WrapMember(getupvalue, "getupvalue");
            WrapMember(getupvalues, "getupvalues");
            WrapMember(setupvalue, "setupvalue");
            WrapMember(setupvaluename, "setupvaluename");
            WrapMember(getlocal, "getlocal");
            WrapMember(getlocals, "getlocals");
            WrapMember(setlocal, "setlocal");
            WrapMember(getconstants, "getconstants");
            WrapMember(getconstant, "getconstant");
            WrapMember(setconstant, "setconstant");
            WrapMember(getreg, "getregistry");
            WrapMember(getrawmetatable, "getmetatable");
            WrapMember(setrawmetatable, "setmetatable");

            LockTable();
        );

		/* initialise bit.* library */
        WrapGlobalTable("bit",
            WrapMember(bitbdiv, "bdiv");
            WrapMember(bitarshift, "arshift");
            WrapMember(bitrshift, "rshift");
            WrapMember(bitbswap, "bswap");
            WrapMember(bitbor, "bor");
            WrapMember(bitbnot, "bnot");
            WrapMember(bitbmul, "bmul");
            WrapMember(bitbsub, "bsub");
            WrapMember(bitbxor, "bxor");
            WrapMember(bittobit, "tobit");
            WrapMember(bitror, "ror");
            WrapMember(bitrol, "rol");
            WrapMember(bitlshift, "lshift");
            WrapMember(bittohex, "tohex");
            WrapMember(bitband, "band");
            WrapMember(bitbadd, "badd");

            LockTable();
        );

		InitRL = RL;
		RobloxGlobalRL = ORL;
        
        VM_TIGER_WHITE_END;

		return true;
	}

	void RbxApi::SetHWID(std::string HWID)
	{
		syn::HWID = std::move(HWID);
	}
}
