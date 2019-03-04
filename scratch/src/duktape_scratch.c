/*
 *  Very simple example program
 */

#include "duktape.h"
#include <stdio.h>
#include <stdlib.h>

static duk_ret_t native_print(duk_context *ctx) {
	duk_push_string(ctx, " ");
	duk_insert(ctx, 0);
	duk_join(ctx, duk_get_top(ctx) - 1);
	printf("%s\n", duk_safe_to_string(ctx, -1));
	return 0;
}

static duk_ret_t native_get(duk_context *ctx) {
	duk_push_array(ctx);
	return 1;
}

static duk_ret_t native_change(duk_context *ctx) {
	duk_push_uint(ctx, 1);
	duk_put_prop_index(ctx, 0, 0);
	duk_push_uint(ctx, 2);
	duk_put_prop_index(ctx, 0, 1);
	duk_push_uint(ctx, 99);
	duk_put_prop_index(ctx, 0, 0);
	return 0;
}

int main(int argc, char *argv[]) {
	duk_context *ctx = duk_create_heap_default();

	(void) argc; (void) argv;  /* suppress warning */

	duk_unity_open(ctx);

	duk_push_c_function(ctx, native_print, DUK_VARARGS);
	duk_put_global_string(ctx, "print");
	duk_push_c_function(ctx, native_get, DUK_VARARGS);
	duk_put_global_string(ctx, "get");
	duk_push_c_function(ctx, native_change, DUK_VARARGS);
	duk_put_global_string(ctx, "change");

	FILE *fp = fopen("main.js", "r");
	if (fp) {
		fseek(fp, 0, SEEK_END);
		long length = ftell(fp);
		fseek(fp, 0, SEEK_SET);
		char *buf = malloc(length + 1);
		memset(buf, 0, length);
		fread(buf, length, 1, fp);
		fclose(fp);
		//printf("source(%d): %s\n", length, buf);
		if (duk_peval_string(ctx, buf) != 0) {
			duk_get_prop_string(ctx, -1, "stack");
			char *err = duk_safe_to_string(ctx, -1);
			printf("peval error: %s\n", err);
		}
		free(buf);
		duk_pop(ctx);  /* pop eval result */
	}
	else {
		printf("can not read file\n");
	}
	duk_destroy_heap(ctx);
	fflush(stdout);
	system("pause");
	return 0;
}
