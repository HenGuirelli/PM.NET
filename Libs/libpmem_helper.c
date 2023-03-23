// https://pmem.io/pmdk/libpmem/
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <stdio.h>
#include <errno.h>
#include <stdlib.h>
#ifndef _WIN32
#include <unistd.h>
#else
#include <io.h>
#endif
#include <string.h>
#include <libpmem.h>
#include <stdio.h>

void* test_pmem_map_file(
	const char *path, size_t len, int flags, mode_t mode,
	size_t *mapped_lenp, int *is_pmemp)
{
	return pmem_map_file(path, len, flags, mode, mapped_lenp, is_pmemp);
}

void pmem_write_string(char *path, int pmem_len, char *text)
{
	char *pmemaddr;
	size_t mapped_len;
	int is_pmem;

	/* create a pmem file and memory map it */
	if ((pmemaddr = (char *)pmem_map_file(path, pmem_len, PMEM_FILE_CREATE, 0666, &mapped_len, &is_pmem)) == NULL) {
		perror("pmem_map_file");
		exit(1);
	}

	/* store a string to the persistent memory */
	strcpy(pmemaddr, text);
	
	if (is_pmem)
		pmem_persist(pmemaddr, mapped_len);
	else
		pmem_msync(pmemaddr, mapped_len); // flush above strcpy to persistence 
}

void* pmem_write_bytes(char *path, int pmem_len, void *bytes, size_t array_len)
{
	void *pmemaddr;
	size_t mapped_len;
	int is_pmem;

	/* create a pmem file and memory map it */
	if ((pmemaddr = pmem_map_file(path, pmem_len, PMEM_FILE_CREATE, 0666, &mapped_len, &is_pmem)) == NULL) {
		perror("pmem_map_file");
		exit(1);
	}
	
	if (is_pmem)
		pmem_memcpy_persist(pmemaddr, bytes, array_len);
	else{
		memcpy(pmemaddr, bytes, array_len);
		pmem_msync(pmemaddr, mapped_len);
	}

	return pmemaddr;
}

void* pmem_read_bytes(char *pmem_file, int pmem_len)
{
	void *pmemaddr;
	size_t mapped_len;
	int is_pmem;

	/* open the pmem file to read back the data */
	if ((pmemaddr = pmem_map_file(pmem_file, pmem_len, PMEM_FILE_CREATE,
		0666, &mapped_len, &is_pmem)) == NULL) {
		perror("pmem_map_file");
		exit(1);
	}

	return pmemaddr;
}

char* pmem_read_string(char *pmem_file, int pmem_len)
{
	char *pmemaddr;
	size_t mapped_len;
	int is_pmem;

	/* open the pmem file to read back the data */
	if ((pmemaddr = (char *)pmem_map_file(pmem_file, pmem_len, PMEM_FILE_CREATE,
		0666, &mapped_len, &is_pmem)) == NULL) {
		perror("pmem_map_file");
		exit(1);
	}

	return pmemaddr;
}

void* pmemclr_pmem_memcpy_persist(void* addr, size_t size)
{
	// TODO: Change to result in arguments
	void* result = malloc(size);
	pmem_memcpy_persist(result, addr, size);
	return result;
}

// gcc -shared -o libpmem_helper.so -fPIC libpmem_helper.c -lpmem