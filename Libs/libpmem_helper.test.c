#include "libpmem_helper.c"

#define PMEM_LEN 4096

void print_start_test(char* test_name);
void print_end_test();
void test_pmem_write_bytes();
void test_pmem_read_bytes();
void test_pmem_mem_set();

int main(int argc, char *argv[])
{
	//print_start_test("test_pmem_write_bytes");
	//test_pmem_write_bytes();
	//print_end_test();

	print_start_test("test_pmem_mem_set");
	test_pmem_mem_set();
	print_end_test();

	exit(0);
}

void print_start_test(char* test_name){
    printf("----------------- %s -----------\n", test_name);
}

void print_end_test(){
	printf("sucess\n");
    printf("-------------------------------\n");
}

void test_pmem_write_bytes(){
	char* data = "Hello World";
	pmem_write_bytes("filename_bytes", PMEM_LEN, data, 11);
}

void test_pmem_mem_set(){
	
	size_t mapped_len;
	int is_pmem;
	int* p = (int *)test_pmem_map_file("/pmem/WriteAndReadFromPmem2", 4096, 1, 0777, &mapped_len, &is_pmem);
}

// gcc -o libpmem_helper.test.o -fPIC libpmem_helper.test.c -lpmem
// ./libpmem_helper.test.o