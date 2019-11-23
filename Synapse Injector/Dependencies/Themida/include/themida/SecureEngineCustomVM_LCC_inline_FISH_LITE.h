/******************************************************************************
 * Header: SecureEngineCustomVM_LCC_inline_FISH_LITE.h
 * Description: LCC inline assembly macros definitions
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
#define VM_FISH_LITE_START                                 __asm__ (" .byte\t0xEB, 0x10, 0x57, 0x4C, 0x20, 0x20, 0x80, 0x00, 0x00, 0x00, \
                                                                     0x00, 0x00, 0x00, 0x00, 0x57, 0x4C, 0x20, 0x20");
#endif

#ifndef VM_FISH_LITE_END
#define VM_FISH_LITE_END                                   __asm__ (" .byte\t0xEB, 0x10, 0x57, 0x4C, 0x20, 0x20, 0x10, 0x02, 0x00, 0x00, \
                                                                     0x00, 0x00, 0x00, 0x00, 0x57, 0x4C, 0x20, 0x20");
#endif

#endif

#ifdef PLATFORM_X64

#ifndef VM_FISH_LITE_START
#define VM_FISH_LITE_START                                 __asm__ (" .byte\t0xEB, 0x10, 0x57, 0x4C, 0x20, 0x20, 0x81, 0x00, 0x00, 0x00, \
                                                                     0x00, 0x00, 0x00, 0x00, 0x57, 0x4C, 0x20, 0x20");
#endif

#ifndef VM_FISH_LITE_END
#define VM_FISH_LITE_END                                   __asm__ (" .byte\t0xEB, 0x10, 0x57, 0x4C, 0x20, 0x20, 0x11, 0x02, 0x00, 0x00, \
                                                                     0x00, 0x00, 0x00, 0x00, 0x57, 0x4C, 0x20, 0x20");
#endif

#endif

