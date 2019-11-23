/******************************************************************************
 * Header: SecureEngineCustomVM_TIGER_LONDON.h
 * Description:  Definitions for Private VM in SecureEngine
 *
 * Author/s: Oreans Technologies 
 * (c) 2015 Oreans Technologies
 *
 * --- File generated automatically from Oreans VM Generator (27/1/2015) ---
 ******************************************************************************/

// ***********************************************
// Definition of macros as function names
// ***********************************************

 #ifdef __cplusplus
  extern "C" {
 #endif

DLL_IMPORT void STDCALL_CONVENTION CustomVM00000132_Start(void);

DLL_IMPORT void STDCALL_CONVENTION CustomVM00000132_End(void);

DLL_IMPORT void STDCALL_CONVENTION CustomVM00000133_Start(void);

DLL_IMPORT void STDCALL_CONVENTION CustomVM00000133_End(void);

#ifdef __cplusplus
}
#endif


// ***********************************************
// x64 definition as function names
// ***********************************************

#if defined(PLATFORM_X64) && !defined(CV_X64_INSERT_VIA_INLINE)

#define VM_TIGER_LONDON_START CustomVM00000133_Start();
#define VM_TIGER_LONDON_END CustomVM00000133_End();

#define CV_CUSTOM_VMS_DEFINED

#endif 


// ***********************************************
// x32 definition as function names
// ***********************************************

#if defined(PLATFORM_X32) && !defined(CV_X32_INSERT_VIA_INLINE)

#define VM_TIGER_LONDON_START CustomVM00000132_Start();
#define VM_TIGER_LONDON_END CustomVM00000132_End();

#define CV_CUSTOM_VMS_DEFINED

#endif 


// ***********************************************
// x32/x64 definition as inline assembly
// ***********************************************

#ifndef CV_CUSTOM_VMS_DEFINED

#ifdef __BORLANDC__
  #include "SecureEngineCustomVM_BorlandC_inline_TIGER_LONDON.h"
#endif

#ifdef __GNUC__
  #include "SecureEngineCustomVM_GNU_inline_TIGER_LONDON.h"
#endif

#ifdef __ICL
  #include "SecureEngineCustomVM_ICL_inline_TIGER_LONDON.h"
#endif

#ifdef __LCC__
  #include "SecureEngineCustomVM_LCC_inline_TIGER_LONDON.h"
#endif

#if defined(_MSC_VER) || defined(__INTEL_COMPILER)
  #include "SecureEngineCustomVM_VC_inline_TIGER_LONDON.h"
#endif

#endif
