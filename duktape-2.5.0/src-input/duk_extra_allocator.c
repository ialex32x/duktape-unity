#include "duk_internal.h"
#include "tlsf.h"

/**************************
 * memory allocation
 **************************/

struct duk_unity_memory_state {
    duk_size_t malloc_count; 
    duk_size_t malloc_size;

    duk_size_t malloc_fail;
    duk_size_t large_alloc;

    //TODO: optimize, use small/large tlsf pool? use pool list?
    duk_size_t pool_size;
    duk_size_t large_size;
    char *pool_ptr;
    tlsf_t tlsf;
};

typedef struct {
	/* The double value in the union is there to ensure alignment is
	 * good for IEEE doubles too.  In many 32-bit environments 4 bytes
	 * would be sufficiently aligned and the double value is unnecessary.
	 */
	union {
		size_t sz;
		double d;
	} u;
} alloc_hdr;

// DUK_INTERNAL DUK_INLINE duk_size_t duk_unity_malloc_size(void *ptr) {
// #if defined(__APPLE__)
//     return malloc_size(ptr);
// #elif defined(_WIN32)
//     return _msize(ptr);
// #elif defined(__linux__)
//     return malloc_usable_size(ptr);
// #else
//     return 0;
// #endif
// }

DUK_LOCAL DUK_INLINE duk_bool_t duk_unity_mempool_contains(struct duk_unity_memory_state *state, const void *ptr) {
    return ptr >= state->pool_ptr && ptr < state->pool_ptr + state->pool_size;
}

DUK_INTERNAL struct duk_unity_memory_state *duk_unity_memory_state_create() {
    struct duk_unity_memory_state *state = (struct duk_unity_memory_state *) malloc(sizeof(struct duk_unity_memory_state));
    if (state) {
        // memset(state, 0, sizeof(struct duk_unity_memory_state));
        state->malloc_count = 0;
        state->malloc_size = 0;
        state->malloc_fail = 0;
        state->large_alloc = 0;

        duk_size_t pool_size = 4 * 1024 * 1024;
        void *pool_ptr = malloc(pool_size);
        if (pool_ptr) {
            state->pool_ptr = (char *) pool_ptr;
            state->pool_size = pool_size;
            state->large_size = pool_size / 8;
            state->tlsf = tlsf_create_with_pool(pool_ptr, pool_size);
        } else {
            state->pool_ptr = NULL;
            state->pool_size = 0;
            state->large_size = 0;
            state->tlsf = NULL;
        }
    }
    return state;
}

DUK_INTERNAL void duk_unity_memory_state_destroy(struct duk_unity_memory_state *state) {
    if (state) {
        if (state->pool_ptr) {
            tlsf_destroy(state->tlsf);
            free(state->pool_ptr);
            state->pool_ptr = NULL;
        }
        free(state);
    }
}

DUK_EXTERNAL void duk_unity_get_memory_state(duk_hthread *thr, duk_uint_t *malloc_count, duk_uint_t *malloc_size) {
    if (thr) {
        duk_heap *heap = thr->heap;
        if (heap) {
            struct duk_unity_memory_state *state = (struct duk_unity_memory_state *)(heap->heap_udata);
            if (state) {
                if (malloc_count) {
                    *malloc_count = (duk_uint_t)(state->malloc_count);
                }
                if (malloc_size) {
                    *malloc_size = (duk_uint_t)(state->malloc_size);
                }
            }
        }
    }
}

DUK_INTERNAL void *duk_unity_default_alloc_function(void *udata, duk_size_t size) {
    alloc_hdr *hdr;
	void *res;
    duk_size_t real_size = size + sizeof(alloc_hdr);
	struct duk_unity_memory_state *state = (struct duk_unity_memory_state *)udata;
    if (state->tlsf && size != 0 && size < state->large_size) {
        hdr = (alloc_hdr *)tlsf_malloc(state->tlsf, real_size);
    }
    if (hdr == NULL) {
        hdr = (alloc_hdr *)malloc(real_size);
    }
    if (!hdr) {
        return NULL;
    }
    state->malloc_count++;
    state->malloc_size += size;
    hdr->u.sz = size;
    res = (void*)(hdr + 1);
	return res;
}

DUK_INTERNAL void *duk_unity_default_realloc_function(void *udata, void *ptr, duk_size_t size) {
	alloc_hdr *hdr;
	size_t old_size;
	void *t;
	void *ret;

	struct duk_unity_memory_state *state = (struct duk_unity_memory_state *)udata;

	/* Handle the ptr-NULL vs. size-zero cases explicitly to minimize
	 * platform assumptions.  You can get away with much less in specific
	 * well-behaving environments.
	 */

	if (ptr) {
		hdr = (alloc_hdr *) (void *) ((unsigned char *) ptr - sizeof(alloc_hdr));
		old_size = hdr->u.sz;

        state->malloc_count--;
        state->malloc_size -= old_size;

		if (size == 0) {
            if (duk_unity_mempool_contains(state, (void *) hdr)) {
				tlsf_free(state->tlsf, (void *) hdr);
			} else {
			    free((void *) hdr);
            }
			return NULL;
		} else {
            duk_size_t real_size = size + sizeof(alloc_hdr);
            if (duk_unity_mempool_contains(state, (void *) hdr)) {
                if (real_size < state->large_size) {
                    t = tlsf_realloc(state->tlsf, (void *) hdr, real_size);
                } else {
                    tlsf_free(state->tlsf, (void *) hdr);
                    t = malloc(real_size);
                }
            } else {
                t = realloc((void *) hdr, real_size);
            }
			if (!t) {
				return NULL;
			}
			hdr = (alloc_hdr *) t;
			hdr->u.sz = size;
			ret = (void *) (hdr + 1);

            state->malloc_count++;
            state->malloc_size += size;

			return ret;
		}
	} else {
        if (size == 0) {
            return NULL;
        }
        return duk_unity_default_alloc_function(state, size);
	}
}

DUK_INTERNAL void duk_unity_default_free_function(void *udata, void *ptr) {
    alloc_hdr *hdr;
	struct duk_unity_memory_state *state = (struct duk_unity_memory_state *)udata;
	if (!ptr) {
		return;
	}
	hdr = (alloc_hdr *) (void *) ((unsigned char *) ptr - sizeof(alloc_hdr));
    state->malloc_count--;
    state->malloc_size -= hdr->u.sz;
    if (duk_unity_mempool_contains(state, hdr)) {
        tlsf_free(state->tlsf, (void *) hdr);
    } else {
        free((void *) hdr);
    }

}

DUK_EXTERNAL duk_hthread *duk_unity_create_heap() {
    struct duk_unity_memory_state *state = duk_unity_memory_state_create();
    if (state) {
        return duk_create_heap(
            duk_unity_default_alloc_function, 
            duk_unity_default_realloc_function, 
            duk_unity_default_free_function, 
            state, 
            NULL);
    }
    return NULL;
}

DUK_EXTERNAL void duk_unity_destroy_heap(duk_hthread *thr) {
    duk_heap *heap;
    if (!thr) {
        return;
    }
    heap = thr->heap;
    duk_unity_memory_state_destroy((struct duk_unity_memory_state *)heap->heap_udata);
    heap->heap_udata = NULL;
    duk_destroy_heap(thr);
}
