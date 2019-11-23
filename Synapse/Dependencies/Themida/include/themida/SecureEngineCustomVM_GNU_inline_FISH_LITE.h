/******************************************************************************
 * Header: SecureEngineCustomVM_GNU_inline_FISH_LITE.h
 * Description: GNU C inline assembly macros definitions
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
asm (".byte 0xEB\n"\
     ".byte 0x10\n"\
     ".byte 0x57\n"\
     ".byte 0x4C\n"\
     ".byte 0x20\n"\
     ".byte 0x20\n"\
     ".byte 0x80\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x57\n"\
     ".byte 0x4C\n"\
     ".byte 0x20\n"\
     ".byte 0x20\n");
#endif

#ifndef VM_FISH_LITE_END
#define VM_FISH_LITE_END \
asm (".byte 0xEB\n"\
     ".byte 0x10\n"\
     ".byte 0x57\n"\
     ".byte 0x4C\n"\
     ".byte 0x20\n"\
     ".byte 0x20\n"\
     ".byte 0x10\n"\
     ".byte 0x02\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x57\n"\
     ".byte 0x4C\n"\
     ".byte 0x20\n"\
     ".byte 0x20\n");
#endif

#endif

#ifdef PLATFORM_X64

#ifndef VM_FISH_LITE_START
#define VM_FISH_LITE_START \
asm (".byte 0xEB\n"\
     ".byte 0x10\n"\
     ".byte 0x57\n"\
     ".byte 0x4C\n"\
     ".byte 0x20\n"\
     ".byte 0x20\n"\
     ".byte 0x81\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x57\n"\
     ".byte 0x4C\n"\
     ".byte 0x20\n"\
     ".byte 0x20\n");
#endif

#ifndef VM_FISH_LITE_END
#define VM_FISH_LITE_END \
asm (".byte 0xEB\n"\
     ".byte 0x10\n"\
     ".byte 0x57\n"\
     ".byte 0x4C\n"\
     ".byte 0x20\n"\
     ".byte 0x20\n"\
     ".byte 0x11\n"\
     ".byte 0x02\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x00\n"\
     ".byte 0x57\n"\
     ".byte 0x4C\n"\
     ".byte 0x20\n"\
     ".byte 0x20\n");
#endif

#endif

