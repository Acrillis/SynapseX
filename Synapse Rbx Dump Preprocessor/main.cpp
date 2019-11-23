/*
*
*	HeatSeeker
*	private tool for development (do not share)
*
*/
#include <Windows.h>
#include <DbgHelp.h>

#include <cstdlib>
#include <cstdio>
#include <cstring>
#pragma warning(disable: 4996)

#ifndef TRUE
#define TRUE 1
#endif

#ifndef FALSE
#define FALSE 0
#endif

const char*		rbx_obfuscated_ref_mask86 = "xx?????x??";
unsigned char	rbx_obfuscated_ref86[] =
{
	0xC7, 0x45, 0x00, 0x00, 0x00, 0x00, 0x00,	/* mov [A+B], K1 */
	0x8B, 0x00, 0x00,							/* mov D, [A+B] */
};

int chunk_compare(unsigned char* chunk, unsigned char* signature, const char* mask, unsigned long length)
{
	for (unsigned long i = 0; i < length; i++)
	{
		if (mask[i] == 'x' && signature[i] != chunk[i]) return FALSE;
	}
	return TRUE;
}

unsigned long chunk_search(unsigned char* chunk, unsigned long start, unsigned char* signature, const char* mask, unsigned long siglength, unsigned long chunklength)
{
	for (unsigned long i = start; i < chunklength - siglength; i++)
	{
		if (chunk_compare(chunk + i, signature, mask, siglength))
		{
			return i;
		}
	}
	return 0;
}

unsigned char* chunk_find_sub(unsigned char *chunk, unsigned long *size, unsigned long *constant)
{
	unsigned char last = 0;
	for (int i = 0; i < 16; i++)
	{
		unsigned char *sub = 0;
		if (chunk[i] == 0x2D)
		{
			/* sub eax, K2 = 2D <4 byte const> (5 bytes) */
			*size = 5;
			sub = chunk + i;
		}
		else if (last == 0x81)
		{
			/* sub reg, K2 = 81 <1 byte specifying register> <4 byte const> where reg != eax (6 bytes) */
			*size = 6;
			sub = chunk + i - 1;
		}

		if (sub)
		{
			*constant = *(unsigned long*)(chunk + i + 1);
			return sub;
		}
		last = chunk[i];
	}
	return 0;
}

int heatseeker86(unsigned char* chunk, unsigned long chunklength)
{
	unsigned long obfuscated_ref = 0;
	while (obfuscated_ref = chunk_search(chunk,
		obfuscated_ref,
		rbx_obfuscated_ref86,
		rbx_obfuscated_ref_mask86,
		sizeof(rbx_obfuscated_ref86),
		chunklength))
	{
		unsigned char* rs = chunk + obfuscated_ref;

		unsigned long size, constant;
		unsigned char *sub = chunk_find_sub(rs + sizeof(rbx_obfuscated_ref86), &size, &constant);

		if (sub)
		{
			/*
				Pattern & Goal:

				mov [reg1+O], K1	-> mov [reg1+O], K1 - K2
				mov reg2, [reg1+O]	-> no change
				...
				sub reg2, K2		-> nop
				...
				push reg2			-> no change
			*/

			*(unsigned long*)(rs + 3) -= constant;
			memset(sub, 0x90, size);

			printf("[-] patching ref at +%x (%x)\n", rs, *(unsigned long*)(rs + 3));

			obfuscated_ref = (unsigned long)(sub + size - chunk);
		}
		else
		{
			obfuscated_ref++;
		}
	}

	return TRUE;
}

int main(int argc, char **argv)
{
	if (argc > 1)
	{
		/* path was provided */
		unsigned long file_size_true = 0;
		unsigned long file_size = 0;
		unsigned char* file_content;
		FILE* file_handle = fopen(argv[1], "rb");
		fseek(file_handle, 0L, SEEK_END);
		file_size = ftell(file_handle);
		file_content = (unsigned char*)malloc(sizeof(char) * (file_size + 1));
		fseek(file_handle, 0L, SEEK_SET);
		file_size_true = fread(file_content, sizeof(char), file_size, file_handle);
		if (ferror(file_handle))
		{
			fputs("[-] failed to read input file", stderr);
			return 1;
		}
		else
			file_content[file_size_true++] = '\0';
		fclose(file_handle);

        if (file_content[0] != 'M' || file_content[1] != 'Z')
        {
            fputs("[-] not a valid executable", stderr);
            return 1;
        }

        IMAGE_NT_HEADERS* headers = ImageNtHeader(file_content);
        if (headers->FileHeader.Machine == IMAGE_FILE_MACHINE_I386)
        {
            printf("[-] determined file is x86\n");

            /* patch calls */
            heatseeker86(file_content, file_size_true);
        }
        else
        {
            printf("[-] determined file is x64\n");
            fputs("[-] not yet implemented", stderr);

            return 1;
        }

		/* save file */
		file_handle = fopen("RobloxPlayerBeta_PostPatch.exe", "wb");
		if (!file_handle)
		{
			fputs("[-] failed to open output file", stderr);
			return 1;
		}

		fwrite(file_content, sizeof(char), file_size_true, file_handle);
		fclose(file_handle);
		printf("[-] patch complete!\n");
	}

	return 0;
}