/*
 *  Very simple example program
 */

#include "duktape.h"
#include <stdio.h>
#include <stdlib.h>

#define RUN_AS_MODULE
#define duk_memcmp memcmp
#define duk_memcpy memcpy


static duk_ret_t native_sleep(duk_context* ctx) {
	duk_uint_t tval = duk_require_uint(ctx, 0);
	Sleep(tval);
	return 0;
}

static duk_ret_t native_print(duk_context* ctx) {
	duk_push_string(ctx, " ");
	duk_insert(ctx, 0);
	duk_join(ctx, duk_get_top(ctx) - 1);
	printf("%s\n", duk_safe_to_string(ctx, -1));
	return 0;
}

int main(int argc, char *argv[]) {
	duk_context *ctx = duk_create_heap_default();

	(void) argc; (void) argv;  /* suppress warning */

	duk_unity_open(ctx);

	duk_push_c_function(ctx, native_print, DUK_VARARGS);
	duk_put_global_string(ctx, "print");
	duk_push_c_function(ctx, native_sleep, 1);
	duk_put_global_string(ctx, "sleep");

	// duk_example_attach_debugger(ctx);
	const char* filename = "../../../scratch/scripts/main.js";
	FILE *fp = fopen(filename, "r");
	if (fp) {
		fseek(fp, 0, SEEK_END);
		long length = ftell(fp);
		fseek(fp, 0, SEEK_SET);
		char *buf = malloc(length + 1);
		memset(buf, 0, length + 1);
		fread(buf, length, 1, fp);
		fclose(fp);
		//printf("source(%d): %s\n", length, buf);
		duk_push_string(ctx, buf);

#if defined(RUN_AS_MODULE)
		if (duk_module_node_peval_main(ctx, filename) != 0) {
			duk_get_prop_string(ctx, -1, "stack");
			const char* err = duk_safe_to_string(ctx, -1);
			printf("peval error: %s\n", err);
			//printf("source: %s\n", buf);
		}
#else
		duk_push_string(ctx, filename);
		duk_compile(ctx, 0);
		if (duk_pcall(ctx, 0) != 0) {
			duk_get_prop_string(ctx, -1, "stack");
			const char *err = duk_safe_to_string(ctx, -1);
			printf("peval error: %s\n", err);
			//printf("source: %s\n", buf);
		}
#endif
		free(buf);
		duk_pop(ctx);  // pop eval result 
	} else {
		printf("can not read file\n");
	}

	duk_destroy_heap(ctx);
	fflush(stdout);
	system("pause");
	WSACleanup();
	return 0;
}
