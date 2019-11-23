/******************************************************************************
 * Header: SecureEngineCustomVM_VC_inline_FISH_LITE.h
 * Description: VC inline assembly macros definitions
 *
 * Author/s: Oreans Technologies 
 * (c) 2014 Oreans Technologies
 *
 * --- File generated automatically from Oreans VM Generator (2/4/2014) ---
 ******************************************************************************/

/***********************************************
 * Definition as inline assembly
 ***********************************************/

#ifdef PLATFORM_X32

#ifndef VM_FISH_LITE_START
#define VM_FISH_LITE_START \
  __asm _emit 0xEB \
  __asm _emit 0x10 \
  __asm _emit 0x57\
  __asm _emit 0x4C\
  __asm _emit 0x20 \
  __asm _emit 0x20 \
  __asm _emit 0x80 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x57\
  __asm _emit 0x4C\
  __asm _emit 0x20 \
  __asm _emit 0x20 
#endif

#ifndef VM_FISH_LITE_END
#define VM_FISH_LITE_END \
  __asm _emit 0xEB \
  __asm _emit 0x10 \
  __asm _emit 0x57\
  __asm _emit 0x4C\
  __asm _emit 0x20 \
  __asm _emit 0x20 \
  __asm _emit 0x10 \
  __asm _emit 0x02 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x57\
  __asm _emit 0x4C\
  __asm _emit 0x20 \
  __asm _emit 0x20 
#endif

#endif

#ifdef PLATFORM_X64

#ifndef VM_FISH_LITE_START
#define VM_FISH_LITE_START \
  __asm _emit 0xEB \
  __asm _emit 0x10 \
  __asm _emit 0x57\
  __asm _emit 0x4C\
  __asm _emit 0x20 \
  __asm _emit 0x20 \
  __asm _emit 0x81 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x57\
  __asm _emit 0x4C\
  __asm _emit 0x20 \
  __asm _emit 0x20 
#endif

#ifndef VM_FISH_LITE_END
#define VM_FISH_LITE_END \
  __asm _emit 0xEB \
  __asm _emit 0x10 \
  __asm _emit 0x57\
  __asm _emit 0x4C\
  __asm _emit 0x20 \
  __asm _emit 0x20 \
  __asm _emit 0x11 \
  __asm _emit 0x02 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x00 \
  __asm _emit 0x57\
  __asm _emit 0x4C\
  __asm _emit 0x20 \
  __asm _emit 0x20 
#endif

#endif

