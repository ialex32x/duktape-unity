/*===
*** test_1 (duk_safe_call)
0: 123
1: foo
2: 234
3: 345
final top: 4
==> rc=0, result='undefined'
*** test_2 (duk_safe_call)
insert at 3 ok
0: 123
1: 234
2: 345
3: foo
insert at -1 ok
0: 123
1: 234
2: 345
3: foo
==> rc=1, result='RangeError: invalid stack index 4'
*** test_3 (duk_safe_call)
insert at 0 ok
0: foo
1: 123
2: 234
3: 345
insert at -4 ok
0: 345
1: foo
2: 123
3: 234
==> rc=1, result='RangeError: invalid stack index -5'
*** test_4 (duk_safe_call)
==> rc=1, result='RangeError: invalid stack index -2147483648'
*** test_5 (duk_safe_call)
0: val-0
1: val-1
2: val-2
3: val-3
4: val-4
5: val-5
6: val-6
7: val-7
8: val-8
9: val-9
0: val-9
1: val-0
2: val-1
3: val-2
4: val-3
5: val-4
6: val-5
7: val-6
8: val-7
9: val-8
0: val-9
1: val-0
2: val-1
3: val-8
4: val-2
5: val-3
6: val-4
7: val-5
8: val-6
9: val-7
0: val-9
1: val-0
2: val-1
3: val-8
4: val-2
5: val-3
6: val-7
7: val-4
8: val-5
9: val-6
final top: 10
==> rc=0, result='undefined'
*** test_6 (duk_safe_call)
==> rc=1, result='RangeError: invalid stack index 0'
===*/

static void dump_stack(duk_context *ctx) {
	duk_idx_t i, n;

	n = duk_get_top(ctx);
	for (i = 0; i < n; i++) {
		printf("%ld: %s\n", (long) i, duk_to_string(ctx, i));
	}
}

static void prep(duk_context *ctx) {
	duk_set_top(ctx, 0);
	duk_push_int(ctx, 123);
	duk_push_int(ctx, 234);
	duk_push_int(ctx, 345);       /* -> [ 123 234 345 ] */
	duk_push_string(ctx, "foo");  /* -> [ 123 234 345 "foo" ] */
}

static duk_ret_t test_1(duk_context *ctx, void *udata) {
	(void) udata;

	prep(ctx);
	duk_insert(ctx, -3);          /* -> [ 123 "foo" 234 345 ] */

	dump_stack(ctx);

	printf("final top: %ld\n", (long) duk_get_top(ctx));
	return 0;
}

static duk_ret_t test_2(duk_context *ctx, void *udata) {
	(void) udata;

	prep(ctx);
	duk_insert(ctx, 3);           /* -> [ 123 234 345 "foo" ]  (legal, keep top) */
	printf("insert at 3 ok\n");
	dump_stack(ctx);
	duk_insert(ctx, -1);          /* -> [ 123 234 345 "foo" ]  (legal, keep top) */
	printf("insert at -1 ok\n");
	dump_stack(ctx);
	duk_insert(ctx, 4);           /* (illegal: index too high) */
	printf("insert at 4 ok\n");
	dump_stack(ctx);
	return 0;
}

static duk_ret_t test_3(duk_context *ctx, void *udata) {
	(void) udata;

	prep(ctx);
	duk_insert(ctx, 0);           /* -> [ "foo" 123 234 345 ]  (legal) */
	printf("insert at 0 ok\n");
	dump_stack(ctx);
	duk_insert(ctx, -4);          /* -> [ 345 "foo" 123 234 ]  (legal) */
	printf("insert at -4 ok\n");
	dump_stack(ctx);
	duk_insert(ctx, -5);          /* (illegal: index too low) */
	printf("insert at -5 ok\n");
	dump_stack(ctx);
	return 0;
}

static duk_ret_t test_4(duk_context *ctx, void *udata) {
	(void) udata;

	prep(ctx);
	duk_insert(ctx, DUK_INVALID_INDEX);  /* (illegal: invalid index) */
	printf("insert at DUK_INVALID_INDEX ok\n");
	dump_stack(ctx);
	return 0;
}

static duk_ret_t test_5(duk_context *ctx, void *udata) {
	duk_idx_t i;

	(void) udata;

	for (i = 0; i < 10; i++) {
		duk_push_sprintf(ctx, "val-%d", (int) i);
	}
	dump_stack(ctx);
	duk_insert(ctx, 0);
	dump_stack(ctx);
	duk_insert(ctx, 3);
	dump_stack(ctx);
	duk_insert(ctx, -4);
	dump_stack(ctx);

	printf("final top: %ld\n", (long) duk_get_top(ctx));

	return 0;
}

static duk_ret_t test_6(duk_context *ctx, void *udata) {
	(void) udata;

	duk_set_top(ctx, 0);
	duk_insert(ctx, 0);
	printf("insert on empty stack\n");
	dump_stack(ctx);
	return 0;
}

void test(duk_context *ctx) {
	TEST_SAFE_CALL(test_1);
	TEST_SAFE_CALL(test_2);
	TEST_SAFE_CALL(test_3);
	TEST_SAFE_CALL(test_4);
	TEST_SAFE_CALL(test_5);
	TEST_SAFE_CALL(test_6);
}
