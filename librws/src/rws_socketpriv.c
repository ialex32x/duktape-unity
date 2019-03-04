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
#include "rws_socket.h"
#include "rws_memory.h"
#include "rws_string.h"

#define RWS_CONNECT_RETRY_DELAY 200
#define RWS_CONNECT_ATTEMPS 5

#ifndef  RWS_OS_WINDOWS 
#define	WSAEWOULDBLOCK  EAGAIN	
#define	WSAEINPROGRESS     EINPROGRESS	
#endif

unsigned int rws_socket_get_next_message_id(rws_socket s) {
	const unsigned int mess_id = ++s->next_message_id;
	if (mess_id > 9999999) {
		s->next_message_id = 0;
	}
	return mess_id;
}

void rws_socket_send_ping(rws_socket s) {
	char buff[16];
	size_t len = 0;
	_rws_frame * frame = rws_frame_create();

	len = rws_sprintf(buff, 16, "%u", rws_socket_get_next_message_id(s));

	frame->is_masked = rws_true;
	frame->opcode = rws_opcode_ping;
	rws_frame_fill_with_send_data(frame, buff, len);
	rws_socket_append_send_frames(s, frame);
}

void rws_socket_inform_recvd_frames(rws_socket s) {
	rws_bool is_all_finished = rws_true;
	_rws_frame * frame = NULL;
	_rws_node * cur = s->recvd_frames;
	while (cur) {
		frame = (_rws_frame *)cur->value.object;
		if (frame) {
			if (frame->is_finished) {
				switch (frame->opcode) {
					case rws_opcode_text_frame:
						if (s->on_recvd_text) {
							s->on_recvd_text(s, (const char *)frame->data, (unsigned int)frame->data_size);
						}
						break;
					case rws_opcode_binary_frame:
						if (s->on_recvd_bin) {
							s->on_recvd_bin(s, frame->data, (unsigned int)frame->data_size);
						}
						break;
					default: break;
				}
				rws_frame_delete(frame);
				cur->value.object = NULL;
			} else {
				is_all_finished = rws_false;
			}
		}
		cur = cur->next;
	}
	if (is_all_finished) {
		rws_list_delete_clean(&s->recvd_frames);
	}
}

void rws_socket_read_handshake_responce_value(const char * str, char ** value) {
	const char * s = NULL;
	size_t len = 0;

	while (*str == ':' || *str == ' ') {
		str++;
	}
	s = str;
	while (*s != '\r' && *s != '\n') {
		s++;
		len++;
	}
	if (len > 0) {
		*value = rws_string_copy_len(str, len);
	}
}

rws_bool rws_socket_process_handshake_responce(rws_socket s) {
	const char * str = (const char *)s->received;
	const char * sub = NULL;
	float http_ver = -1;
	int http_code = -1;

	rws_error_delete_clean(&s->error);
	sub = strstr(str, "HTTP/");
	if (!sub) {
		return rws_false;
	}
	
	sub += 5;
	if (rws_sscanf(sub, "%f %i", &http_ver, &http_code) != 2) {
		http_ver = -1;
		http_code = -1;
	}

	sub = strstr(str, k_rws_socket_sec_websocket_accept); // "Sec-WebSocket-Accept"
	if (sub) {
		sub += strlen(k_rws_socket_sec_websocket_accept);
		rws_socket_read_handshake_responce_value(sub, &s->sec_ws_accept);
	}

	if (http_code != 101 || !s->sec_ws_accept) {
		s->error = rws_error_new_code_descr(rws_error_code_parse_handshake,
											(http_code != 101) ? "HTPP code not found or non 101" : "Accept key not found");
		return rws_false;
	}
	return rws_true;
}

// need close socket on error
rws_bool rws_socket_send(rws_socket s, const void * data, const size_t data_size) {
	int sended = -1, error_number = -1;
	rws_error_delete_clean(&s->error);

	//errno = -1;
#if defined(RWS_OS_WINDOWS)
	sended = send(s->socket, (const char *)data, data_size, 0);
    error_number = WSAGetLastError();
#else
	sended = (int)send(s->socket, data, (int)data_size, 0);
    error_number = errno;
#endif


	if (sended > 0) {
		return rws_true;
	}

	rws_socket_check_write_error(s, error_number);
	if (s->error) {
		rws_socket_close(s);
		return rws_false;
	}
	return rws_true;
}

rws_bool rws_socket_recv(rws_socket s) {
	int is_reading = 1, error_number = -1, len = -1;
	char * received = NULL;
	size_t total_len = 0;
	char buff[8192];
	rws_error_delete_clean(&s->error);
	while (is_reading) {
		len = (int)recv(s->socket, buff, 8192, 0);
#if defined(RWS_OS_WINDOWS)
		error_number = WSAGetLastError();
#else
		error_number = errno;
#endif
		if (len > 0) {
			total_len += len;
			if (s->received_size - s->received_len < len) {
				rws_socket_resize_received(s, s->received_size + len);
			}
			received = (char *)s->received;
			if (s->received_len) {
				received += s->received_len;
			}
			memcpy(received, buff, len);
			s->received_len += len;
		} else {
			is_reading = 0;
		}
	}
	//if (error_number < 0) return rws_true;
	if (error_number != WSAEWOULDBLOCK && error_number != WSAEINPROGRESS) {
		s->error = rws_error_new_code_descr(rws_error_code_read_write_socket, "Failed read/write socket");
		rws_socket_close(s);
		return rws_false;
	}
	return rws_true;
}

_rws_frame * rws_socket_last_unfin_recvd_frame_by_opcode(rws_socket s, const rws_opcode opcode) {
	_rws_frame * last = NULL;
	_rws_frame * frame = NULL;
	_rws_node * cur = s->recvd_frames;
	while (cur) {
		frame = (_rws_frame *)cur->value.object;
		if (frame) {
            //  [FIN=0,opcode !=0 ],[FIN=0,opcode ==0 ],....[FIN=1,opcode ==0 ]
			if (!frame->is_finished /*&& frame->opcode == opcode*/) {
				last = frame;
			}
		}
		cur = cur->next;
	}
	return last;
}

void rws_socket_process_bin_or_text_frame(rws_socket s, _rws_frame * frame) {
	_rws_frame * last_unfin = rws_socket_last_unfin_recvd_frame_by_opcode(s, frame->opcode);
	if (last_unfin) {
		rws_frame_combine_datas(last_unfin, frame);
		last_unfin->is_finished = frame->is_finished;
		rws_frame_delete(frame);
	} else if (frame->data && frame->data_size) {
		rws_socket_append_recvd_frames(s, frame);
	} else {
		rws_frame_delete(frame);
	}
}

void rws_socket_process_ping_frame(rws_socket s, _rws_frame * frame) {
	_rws_frame * pong_frame = rws_frame_create();
	pong_frame->opcode = rws_opcode_pong;
	pong_frame->is_masked = rws_true;
	rws_frame_fill_with_send_data(pong_frame, frame->data, frame->data_size);
	rws_frame_delete(frame);
	rws_socket_append_send_frames(s, pong_frame);
}

void rws_socket_process_conn_close_frame(rws_socket s, _rws_frame * frame) {
	s->command = COMMAND_INFORM_DISCONNECTED;
	s->error = rws_error_new_code_descr(rws_error_code_connection_closed, "Connection was closed by endpoint");
	//rws_socket_close(s);
	rws_frame_delete(frame);
}

void rws_socket_process_received_frame(rws_socket s, _rws_frame * frame) {
	switch (frame->opcode) {
		case rws_opcode_ping: rws_socket_process_ping_frame(s, frame); break;
		case rws_opcode_text_frame:
		case rws_opcode_binary_frame:
		case rws_opcode_continuation:
			rws_socket_process_bin_or_text_frame(s, frame);
			break;
		case rws_opcode_connection_close: rws_socket_process_conn_close_frame(s, frame); break;
		default:
			// unprocessed => delete
			rws_frame_delete(frame);
			break;
	}
}

void rws_socket_idle_recv(rws_socket s) {
	_rws_frame * frame = NULL;

	if (!rws_socket_recv(s)) {
		// sock already closed
		if (s->error) {
			s->command = COMMAND_INFORM_DISCONNECTED;
		}
		return;
	}

   const size_t nframe_size = rws_check_recv_frame_size(s->received, s->received_len);
   if (nframe_size) {
       frame = rws_frame_create_with_recv_data(s->received, nframe_size);
       if (frame)  {
           rws_socket_process_received_frame(s, frame);
       }
	   
       if (nframe_size == s->received_len) {
           s->received_len = 0;
       } else if (s->received_len > nframe_size) {
           const size_t nLeftLen = s->received_len - nframe_size;
           memmove((char*)s->received, (char*)s->received + nframe_size, nLeftLen);
           s->received_len = nLeftLen;
       }
   }
}

void rws_socket_idle_send(rws_socket s) {
	_rws_node * cur = NULL;
	rws_bool sending = rws_true;
	_rws_frame * frame = NULL;

	rws_mutex_lock(s->send_mutex);
	cur = s->send_frames;
	if (cur) {
		while (cur && s->is_connected && sending) {
			frame = (_rws_frame *)cur->value.object;
			cur->value.object = NULL;
			if (frame) {
				sending = rws_socket_send(s, frame->data, frame->data_size);
			}
			rws_frame_delete(frame);
			cur = cur->next;
		}
		rws_list_delete_clean(&s->send_frames);
		if (s->error) {
			s->command = COMMAND_INFORM_DISCONNECTED;
		}
	}
	rws_mutex_unlock(s->send_mutex);
}

void rws_socket_wait_handshake_responce(rws_socket s) {
	if (!rws_socket_recv(s)) {
		// sock already closed
		if (s->error) {
			s->command = COMMAND_INFORM_DISCONNECTED;
		}
		return;
	}
	
	if (s->received_len == 0) {
		return;
	}

	if (rws_socket_process_handshake_responce(s)) {
        s->received_len = 0;
		s->is_connected = rws_true;
		s->command = COMMAND_INFORM_CONNECTED;
	} else {
		rws_socket_close(s);
		s->command = COMMAND_INFORM_DISCONNECTED;
	}
}

void rws_socket_send_disconnect(rws_socket s) {
	char buff[16];
	size_t len = 0;
	_rws_frame * frame = rws_frame_create();

	len = rws_sprintf(buff, 16, "%u", rws_socket_get_next_message_id(s));

	frame->is_masked = rws_true;
	frame->opcode = rws_opcode_connection_close;
	rws_frame_fill_with_send_data(frame, buff, len);
	rws_socket_send(s, frame->data, frame->data_size);
	rws_frame_delete(frame);
	s->command = COMMAND_END;
	rws_thread_sleep(RWS_CONNECT_RETRY_DELAY); // little bit wait after send message
}

void rws_socket_send_handshake(rws_socket s) {
	char buff[512];
	char * ptr = buff;
	size_t writed = 0;
	writed = rws_sprintf(ptr, 512, "GET %s HTTP/%s\r\n", s->path, k_rws_socket_min_http_ver);

	if (s->port == 80) {
		writed += rws_sprintf(ptr + writed, 512 - writed, "Host: %s\r\n", s->host);
	} else {
		writed += rws_sprintf(ptr + writed, 512 - writed, "Host: %s:%i\r\n", s->host, s->port);
	}

	writed += rws_sprintf(ptr + writed, 512 - writed,
						  "Upgrade: websocket\r\n"
						  "Connection: Upgrade\r\n"
						  "Origin: %s://%s\r\n",
						  s->scheme, s->host);

	writed += rws_sprintf(ptr + writed, 512 - writed,
						  "Sec-WebSocket-Key: %s\r\n"
						  "Sec-WebSocket-Protocol: chat, superchat\r\n"
						  "Sec-WebSocket-Version: 13\r\n"
						  "\r\n",
						  "dGhlIHNhbXBsZSBub25jZQ==");

	if (rws_socket_send(s, buff, writed)) {
		s->command = COMMAND_WAIT_HANDSHAKE_RESPONCE;
	} else {
		if (s->error) {
			s->error->code = rws_error_code_send_handshake;
		} else {
			s->error = rws_error_new_code_descr(rws_error_code_send_handshake, "Send handshake");
		}
		rws_socket_close(s);
		s->command = COMMAND_INFORM_DISCONNECTED;
	}
}

struct addrinfo * rws_socket_connect_getaddr_info(rws_socket s) {
	struct addrinfo hints;
	char portstr[16];
	struct addrinfo * result = NULL;
	int ret = 0, retry_number = 0, last_ret = 0;
#if defined(RWS_OS_WINDOWS)
	WSADATA wsa;
#endif

	rws_error_delete_clean(&s->error);

#if defined(RWS_OS_WINDOWS)
	memset(&wsa, 0, sizeof(WSADATA));
	if (WSAStartup(MAKEWORD(2,2), &wsa) != 0) {
		s->error = rws_error_new_code_descr(rws_error_code_connect_to_host, "Failed initialise winsock");
		s->command = COMMAND_INFORM_DISCONNECTED;
		return NULL;
	}
#endif

	rws_sprintf(portstr, 16, "%i", s->port);
	while (++retry_number < RWS_CONNECT_ATTEMPS) {
		result = NULL;
		memset(&hints, 0, sizeof(hints));
		hints.ai_family = AF_UNSPEC;
		hints.ai_socktype = SOCK_STREAM;

		ret = getaddrinfo(s->host, portstr, &hints, &result);
		if (ret == 0 && result) {
			return result;
		}

		if (ret != 0) {
			last_ret = ret;
		}
		
		if (result) {
			freeaddrinfo(result);
		}
		
		rws_thread_sleep(RWS_CONNECT_RETRY_DELAY);
	}

#if defined(RWS_OS_WINDOWS)
	WSACleanup();
#endif

	s->error = rws_error_new_code_descr(rws_error_code_connect_to_host,
										(last_ret > 0) ? gai_strerror(last_ret) : "Failed connect to host");
	s->command = COMMAND_INFORM_DISCONNECTED;
	return NULL;
}

void rws_socket_connect_to_host(rws_socket s) {
	struct addrinfo * result = NULL;
	struct addrinfo * p = NULL;
	rws_socket_t sock = RWS_INVALID_SOCKET;
	int retry_number = 0;
#if defined(RWS_OS_WINDOWS)
	unsigned long iMode = 0;
#endif

	result = rws_socket_connect_getaddr_info(s);
	if (!result) {
		return;
	}

	while ((++retry_number < RWS_CONNECT_ATTEMPS) && (sock == RWS_INVALID_SOCKET)) {
		for (p = result; p != NULL; p = p->ai_next) {
			sock = socket(p->ai_family, p->ai_socktype, p->ai_protocol);
			if (sock != RWS_INVALID_SOCKET) {
				rws_socket_set_option(sock, SO_ERROR, 1); // When an error occurs on a socket, set error variable so_error and notify process
				rws_socket_set_option(sock, SO_KEEPALIVE, 1); // Periodically test if connection is alive

				if (connect(sock, p->ai_addr, p->ai_addrlen) == 0) {
                    s->received_len = 0;
					s->socket = sock;
#if defined(RWS_OS_WINDOWS)
					// If iMode != 0, non-blocking mode is enabled.
					iMode = 1;
					ioctlsocket(s->socket, FIONBIO, &iMode);
#else
					fcntl(s->socket, F_SETFL, O_NONBLOCK);
#endif
					break;
				}
				RWS_SOCK_CLOSE(sock);
			}
		}
		if (sock == RWS_INVALID_SOCKET) {
			rws_thread_sleep(RWS_CONNECT_RETRY_DELAY);
		}
	}

	freeaddrinfo(result);

	if (s->socket == RWS_INVALID_SOCKET) {
#if defined(RWS_OS_WINDOWS)
		WSACleanup();
#endif
		s->error = rws_error_new_code_descr(rws_error_code_connect_to_host, "Failed connect to host");
		s->command = COMMAND_INFORM_DISCONNECTED;
	} else {
		s->command = COMMAND_SEND_HANDSHAKE;
	}
}

static void rws_socket_work_th_func(void * user_object) {
	rws_socket s = (rws_socket)user_object;
	size_t loop_number = 0;
	while (s->command < COMMAND_END) {
		loop_number++;
		rws_mutex_lock(s->work_mutex);
		switch (s->command) {
			case COMMAND_CONNECT_TO_HOST: rws_socket_connect_to_host(s); break;
			case COMMAND_SEND_HANDSHAKE: rws_socket_send_handshake(s); break;
			case COMMAND_WAIT_HANDSHAKE_RESPONCE: rws_socket_wait_handshake_responce(s); break;
			case COMMAND_DISCONNECT: rws_socket_send_disconnect(s); break;
			case COMMAND_IDLE:
				if (loop_number >= 400) {
					loop_number = 0;
					
					if (s->is_connected) {
						rws_socket_send_ping(s);
					}
				}
				
				if (s->is_connected) {
					rws_socket_idle_send(s);
				}
				
				if (s->is_connected) {
					rws_socket_idle_recv(s);
				}
				break;
			default: break;
		}
		
		rws_mutex_unlock(s->work_mutex);
		
		switch (s->command) {
			case COMMAND_INFORM_CONNECTED:
				s->command = COMMAND_IDLE;
				if (s->on_connected) {
					s->on_connected(s);
				}
				break;
			case COMMAND_INFORM_DISCONNECTED: {
                    s->command = COMMAND_END;
                    rws_socket_send_disconnect(s);
                    if (s->on_disconnected)  {
                        s->on_disconnected(s);
                    }
                }
				break;
			case COMMAND_IDLE:
				if (s->recvd_frames) {
					rws_socket_inform_recvd_frames(s);
				}
				break;
			default: break;
		}
		rws_thread_sleep(5);
	}

	rws_socket_close(s);
	s->work_thread = NULL;
	rws_socket_delete(s);
}

rws_bool rws_socket_create_start_work_thread(rws_socket s) {
	rws_error_delete_clean(&s->error);
	s->command = COMMAND_NONE;
	s->work_thread = rws_thread_create(&rws_socket_work_th_func, s);
	if (s->work_thread) {
		s->command = COMMAND_CONNECT_TO_HOST;
		return rws_true;
	}
	return rws_false;
}

void rws_socket_resize_received(rws_socket s, const size_t size) {
	void * res = NULL;
	size_t min = 0;
	if (size == s->received_size) {
		return;
	}

	res = rws_malloc(size);
	assert(res && (size > 0));

	min = (s->received_size < size) ? s->received_size : size;
	if (min > 0 && s->received) {
		memcpy(res, s->received, min);
	}
	rws_free_clean(&s->received);
	s->received = res;
	s->received_size = size;
}

void rws_socket_close(rws_socket s) {
    s->received_len = 0;
	if (s->socket != RWS_INVALID_SOCKET) {
		RWS_SOCK_CLOSE(s->socket);
		s->socket = RWS_INVALID_SOCKET;
#if defined(RWS_OS_WINDOWS)
		WSACleanup();
#endif
	}
	s->is_connected = rws_false;
}

void rws_socket_append_recvd_frames(rws_socket s, _rws_frame * frame) {
	_rws_node_value frame_list_var;
	frame_list_var.object = frame;
	if (s->recvd_frames) {
		rws_list_append(s->recvd_frames, frame_list_var);
	} else {
		s->recvd_frames = rws_list_create();
		s->recvd_frames->value = frame_list_var;
	}
}

void rws_socket_append_send_frames(rws_socket s, _rws_frame * frame) {
	_rws_node_value frame_list_var;
	frame_list_var.object = frame;
	if (s->send_frames) {
		rws_list_append(s->send_frames, frame_list_var);
	} else {
		s->send_frames = rws_list_create();
		s->send_frames->value = frame_list_var;
	}
}

rws_bool rws_socket_send_text_priv(rws_socket s, const char * text) {
	size_t len = text ? strlen(text) : 0;
	_rws_frame * frame = NULL;

	if (len <= 0) {
		return rws_false;
	}

	frame = rws_frame_create();
	frame->is_masked = rws_true;
	frame->opcode = rws_opcode_text_frame;
	rws_frame_fill_with_send_data(frame, text, len);
	rws_socket_append_send_frames(s, frame);

	return rws_true;
}

rws_bool rws_socket_send_bin_priv(rws_socket s, const void * data, size_t len) {
	_rws_frame * frame = NULL;

	if (len <= 0 || data == NULL) {
		return rws_false;
	}

	frame = rws_frame_create();
	frame->is_masked = rws_true;
	frame->opcode = rws_opcode_text_frame;
	rws_frame_fill_with_send_data(frame, data, len);
	rws_socket_append_send_frames(s, frame);

	return rws_true;
}

void rws_socket_delete_all_frames_in_list(_rws_list * list_with_frames) {
	_rws_frame * frame = NULL;
	_rws_node * cur = list_with_frames;
	while (cur) {
		frame = (_rws_frame *)cur->value.object;
		if (frame) {
			rws_frame_delete(frame);
		}
		cur->value.object = NULL;
	}
}

void rws_socket_set_option(rws_socket_t s, int option, int value) {
	setsockopt(s, SOL_SOCKET, option, (char *)&value, sizeof(int));
}

void rws_socket_check_write_error(rws_socket s, int error_num) {
#if defined(RWS_OS_WINDOWS)
	int socket_code = 0, code = 0;
	unsigned int socket_code_size = sizeof(int);
#else
	int socket_code = 0, code = 0;
	socklen_t socket_code_size = sizeof(socket_code);
#endif

	if (s->socket != RWS_INVALID_SOCKET) {
#if defined(RWS_OS_WINDOWS)
		if (getsockopt(s->socket, SOL_SOCKET, SO_ERROR, (char *)&socket_code, (int*)&socket_code_size) != 0) {
			socket_code = 0;
		}
#else
		if (getsockopt(s->socket, SOL_SOCKET, SO_ERROR, &socket_code, &socket_code_size) != 0) {
			socket_code = 0;
		}
#endif
	}

	code = (socket_code > 0) ? socket_code : error_num;
	if (code <= 0) {
		return;
	}
	
	switch (code) {
		// send errors
		case EACCES: //

//		case EAGAIN: // The socket is marked nonblocking and the requested operation would block
//		case EWOULDBLOCK: // The socket is marked nonblocking and the receive operation would block

		case EBADF: // An invalid descriptor was specified
		case ECONNRESET: // Connection reset by peer
		case EDESTADDRREQ: // The socket is not connection-mode, and no peer address is set
		case EFAULT: // An invalid user space address was specified for an argument
					// The receive buffer pointer(s) point outside the process's address space.
		case EINTR: // A signal occurred before any data was transmitted
					// The receive was interrupted by delivery of a signal before any data were available
		case EINVAL: // Invalid argument passed
		case EISCONN: // The connection-mode socket was connected already but a recipient was specified
		case EMSGSIZE: // The socket type requires that message be sent atomically, and the size of the message to be sent made this impossible
		case ENOBUFS: // The output queue for a network interface was full
		case ENOMEM: // No memory available
		case ENOTCONN: // The socket is not connected, and no target has been given
						// The socket is associated with a connection-oriented protocol and has not been connected
		case ENOTSOCK: // The argument sockfd is not a socket
					// The argument sockfd does not refer to a socket
		case EOPNOTSUPP: // Some bit in the flags argument is inappropriate for the socket type.
		case EPIPE: // The local end has been shut down on a connection oriented socket
		// recv errors
		case ECONNREFUSED: // A remote host refused to allow the network connection (typically because it is not running the requested service).

			s->error = rws_error_new_code_descr(rws_error_code_read_write_socket, rws_strerror(code));
			break;

		default:
			break;
	}
}

