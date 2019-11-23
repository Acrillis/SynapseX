/******************************************************************************
 * Header: SecureEngineCustomVM_LCC_inline_FISH_LONDON.h
 * Description: LCC inline assembly macros definitions
 *
 * Author/s: Oreans Technologies 
 * (c) 2015 Oreans Technologies
 *
 * --- File generated automatically from Oreans VM Generator (27/1/2015) ---
 ******************************************************************************/

/***********************************************
 * Definition as inline assembly
 ***********************************************/

#ifdef PLATFORM_X32

#ifndef VM_FISH_LONDON_START
#define VM_FISH_LONDON_START                               __asm__ (" .byte\t0xEB, 0x10, 0x57, 0x4C, 0x20, 0x20, 0x82, 0x00, 0x00, 0x00, \
                                                                     0x00, 0x00, 0x00, 0x00, 0x57, 0x4C, 0x20, 0x20");
#endif

#ifndef VM_FISH_LONDON_END
#define VM_FISH_LONDON_END                                 __asm__ (" .byte\t0xEB, 0x10, 0x57, 0x4C, 0x20, 0x20, 0x12, 0x02, 0x00, 0x00, \
                                                                     0x00, 0x00, 0x00, 0x00, 0x57, 0x4C, 0x20, 0x20");
#endif

#endif

#ifdef PLATFORM_X64

#ifndef VM_FISH_LONDON_START
#define VM_FISH_LONDON_START                               __asm__ (" .byte\t0xEB, 0x10, 0x57, 0x4C, 0x20, 0x20, 0x83, 0x00, 0x00, 0x00, \
                                                                     0x00, 0x00, 0x00, 0x00, 0x57, 0x4C, 0x20, 0x20");
#endif

#ifndef VM_FISH_LONDON_END
#define VM_FISH_LONDON_END                                 __asm__ (" .byte\t0xEB, 0x10, 0x57, 0x4C, 0x20, 0x20, 0x13, 0x02, 0x00, 0x00, \
                                                                     0x00, 0x00, 0x00, 0x00, 0x57, 0x4C, 0x20, 0x20");
#endif

#endif

