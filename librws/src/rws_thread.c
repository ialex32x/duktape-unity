/*
 *   Copyright (c) 2014 - 2019 Oleh Kulykov <info@resident.name>
 *
 *   Permission is hereby granted, free of charge, to any person obtaining a copy
 *   of this software and associated documentation files (the "Software"), to deal
 *   in the Software without restriction, including without limitation the rights
 *   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *   copies of the Software, and to permit persons to whom the Software is
 *   furnished to do so, subject to the following conditions:
 *
 *   The above copyright notice and this permission notice shall be included in
 *   all copies or substantial portions of the Software.
 *
 *   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *   THE SOFTWARE.
 */


#include "../librws.h"
#include "rws_thread.h"
#include "rws_memory.h"
#include "rws_common.h"

#include <assert.h>

#if defined(RWS_OS_WINDOWS)
#include <windows.h>
#else
#include <pthread.h>
#include <unistd.h>
#endif

struct rws_thread_struct {
	rws_thread_funct thread_function;
	void * user_object;
#if defined(RWS_OS_WINDOWS)
	HANDLE thread;
#else
	pthread_t thread;
#endif
};

typedef struct _rws_threads_joiner_struct {
	rws_thread thread;
	rws_mutex mutex;
} _rws_threads_joiner;

static _rws_threads_joiner * _threads_joiner = NULL;
static void rws_threads_joiner_clean(void) { // private
	rws_thread t = _threads_joiner->thread;
#if defined(RWS_OS_WINDOWS)
	DWORD dwExitCode = 0;
#else
	void * r = NULL;
#endif

	if (!t) {
		return;
	}
	_threads_joiner->thread = NULL;

#if defined(RWS_OS_WINDOWS)
	do {
		if (GetExitCodeThread(t->thread, &dwExitCode) == 0) {
			break; // fail
		}
	} while (dwExitCode == STILL_ACTIVE);
	if (dwExitCode == STILL_ACTIVE) {
		TerminateThread(t->thread, 0);
	}
	if (CloseHandle(t->thread)) {
		t->thread = NULL;
	}
#else
	pthread_join(t->thread, &r);
	assert(r == NULL);
#endif
	rws_free(t);
}

static void rws_threads_joiner_add(rws_thread thread) { // public
	rws_mutex_lock(_threads_joiner->mutex);
	rws_threads_joiner_clean();
	_threads_joiner->thread = thread;
	rws_mutex_unlock(_threads_joiner->mutex);
}

static void rws_threads_joiner_create_ifneed(void) {
	if (_threads_joiner) {
		return;
	}
	_threads_joiner = (_rws_threads_joiner *)rws_malloc_zero(sizeof(_rws_threads_joiner));
	_threads_joiner->mutex = rws_mutex_create_recursive();
}

#if defined(RWS_OS_WINDOWS)
static DWORD WINAPI rws_thread_func_priv(LPVOID some_pointer) {
#else
static void * rws_thread_func_priv(void * some_pointer) {
#endif
	rws_thread t = (rws_thread)some_pointer;
	t->thread_function(t->user_object);
	rws_threads_joiner_add(t);

#if  defined(RWS_OS_WINDOWS)
	return 0;
#else
	return NULL;
#endif
}

rws_thread rws_thread_create(rws_thread_funct thread_function, void * user_object) {
	rws_thread t = NULL;
	int res = -1;
#if !defined(RWS_OS_WINDOWS)
	pthread_attr_t attr;
#endif

	if (!thread_function) {
		return NULL;
	}
	rws_threads_joiner_create_ifneed();
	t = (rws_thread)rws_malloc_zero(sizeof(struct rws_thread_struct));
	t->user_object = user_object;
	t->thread_function = thread_function;
#if defined(RWS_OS_WINDOWS)
	t->thread = CreateThread(NULL, 0, &rws_thread_func_priv, (LPVOID)t, 0, NULL);
	assert(t->thread);
#else
	if (pthread_attr_init(&attr) == 0) {
		if (pthread_attr_setscope(&attr, PTHREAD_SCOPE_SYSTEM) == 0) {
			if (pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_JOINABLE) == 0) {
				res = pthread_create(&t->thread, &attr, &rws_thread_func_priv, (void *)t);
			}
		}
		pthread_attr_destroy(&attr);
	}
	assert(res == 0);
#endif
	return t;
}

void rws_thread_sleep(const unsigned int millisec) {
#if defined(RWS_OS_WINDOWS)
	Sleep(millisec); // 1s = 1'000 millisec.
#else
	usleep(millisec * 1000); // 1s = 1'000'000 microsec.
#endif
}

rws_mutex rws_mutex_create_recursive(void) {
#if defined(RWS_OS_WINDOWS)
	CRITICAL_SECTION * mutex = (CRITICAL_SECTION *)rws_malloc_zero(sizeof(CRITICAL_SECTION));
	InitializeCriticalSection((LPCRITICAL_SECTION)mutex);
	return mutex;
#else
	pthread_mutex_t * mutex = (pthread_mutex_t *)rws_malloc_zero(sizeof(pthread_mutex_t));
	int res = -1;
	pthread_mutexattr_t attr;
	if (pthread_mutexattr_init(&attr) == 0) {
		if (pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE) == 0) {
			res = pthread_mutex_init(mutex, &attr);
		}
		pthread_mutexattr_destroy(&attr);
	}
	assert(res == 0);
	return mutex;
#endif
}

void rws_mutex_lock(rws_mutex mutex) {
	if (mutex) {
#if defined(RWS_OS_WINDOWS)
		EnterCriticalSection((LPCRITICAL_SECTION)mutex);
#else
		pthread_mutex_lock((pthread_mutex_t *)mutex);
#endif
	}
}

void rws_mutex_unlock(rws_mutex mutex) {
	if (mutex) {
#if defined(RWS_OS_WINDOWS)
		LeaveCriticalSection((LPCRITICAL_SECTION)mutex);
#else
		pthread_mutex_unlock((pthread_mutex_t *)mutex);
#endif
	}
}

void rws_mutex_delete(rws_mutex mutex) {
	if (mutex) {
#if defined(RWS_OS_WINDOWS)
		DeleteCriticalSection((LPCRITICAL_SECTION)mutex);
#else
		pthread_mutex_destroy((pthread_mutex_t *)mutex);
#endif
		rws_free(mutex);
	}
}

