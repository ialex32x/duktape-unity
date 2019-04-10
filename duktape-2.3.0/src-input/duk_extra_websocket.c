
#include "duk_internal.h"

#include "libwebsockets.h"

#define LWS_BUF_SIZE 65536
#define LWS_PACKET_SIZE 65536
#define LWS_PAYLOAD_SIZE 4096

struct duk_websocket_payload_t {
    duk_bool_t is_binary;
    void *buf;
    duk_size_t len;

    struct duk_websocket_payload_t *next;
};

struct duk_websocket_t {
    duk_context *ctx;
    void *heapptr;

    struct duk_websocket_payload_t *pending_head;
    struct duk_websocket_payload_t *pending_tail;
    struct duk_websocket_payload_t *freelist;
    
    duk_bool_t is_binary;
    void *buf;
    duk_size_t len;

    struct lws_protocols *protocols;
    duk_uarridx_t protocols_size;
    const char *protocol_names;

    struct lws_context *context;
    struct lws *wsi;

    duk_bool_t is_closing;
    duk_bool_t is_servicing;
    duk_bool_t is_polling;
    duk_bool_t is_context_destroying;
    duk_bool_t is_context_destroyed;
};

DUK_LOCAL void _delete_payload(struct duk_websocket_t *websocket, struct duk_websocket_payload_t *payload) {
    payload->next = websocket->freelist;
    websocket->freelist = payload;
}

DUK_LOCAL struct duk_websocket_payload_t *_new_payload(struct duk_websocket_t *websocket) {
    struct duk_websocket_payload_t *payload = websocket->freelist;
    if (payload) {
        websocket->freelist = payload->next;
    } else {
        payload = (struct duk_websocket_payload_t *)duk_alloc(websocket->ctx, sizeof(struct duk_websocket_payload_t));
        duk_memzero(payload, sizeof(struct duk_websocket_payload_t));
        payload->buf = duk_alloc(websocket->ctx, LWS_PAYLOAD_SIZE + LWS_PRE);
        duk_memzero(payload->buf, LWS_PAYLOAD_SIZE + LWS_PRE);
    }
    payload->next = NULL;
    payload->len = 0;
    return payload;
}

DUK_LOCAL void _duk_lws_destroy(struct duk_websocket_t *websocket) {
    if (websocket == NULL || websocket->is_context_destroyed) {
        return;
    }
    if (websocket->is_polling) {
        websocket->is_context_destroying = TRUE;
        return;
    }
    websocket->is_context_destroyed = TRUE;
    if (websocket->context != NULL) {
        lws_context_destroy(websocket->context);
        websocket->context = NULL;
    }
    struct duk_websocket_payload_t *payload = websocket->pending_head;
    while (payload) {
        websocket->pending_head = payload->next;
        payload->next = websocket->freelist;
        websocket->freelist = payload;
        payload = websocket->pending_head;
    }
    websocket->pending_tail = NULL;
}

DUK_LOCAL void _on_connect(struct duk_websocket_t *websocket, const char *protocol) {
    duk_context *ctx = websocket->ctx;
    duk_push_heapptr(ctx, websocket->heapptr);
    duk_push_literal(ctx, "dispatch");
    duk_push_literal(ctx, "open");
    duk_push_string(ctx, protocol);
    if (duk_pcall_prop(ctx, -4, 2) != DUK_EXEC_SUCCESS) {
        lwsl_warn("unable to dispatch: %s", duk_to_string(ctx, -1));
    }
    duk_pop_2(ctx);
}

DUK_LOCAL void _on_error(struct duk_websocket_t *websocket) {
    duk_context *ctx = websocket->ctx;
    duk_push_heapptr(ctx, websocket->heapptr);
    duk_push_literal(ctx, "dispatch");
    duk_push_literal(ctx, "error");
    if (duk_pcall_prop(ctx, -3, 1) != DUK_EXEC_SUCCESS) {
        lwsl_warn("unable to dispatch: %s", duk_to_string(ctx, -1));
    }
    duk_pop_2(ctx);
}

DUK_LOCAL void _on_disconnect(struct duk_websocket_t *websocket) {
    duk_context *ctx = websocket->ctx;
    duk_push_heapptr(ctx, websocket->heapptr);
    duk_push_literal(ctx, "dispatch");
    duk_push_literal(ctx, "close");
    if (duk_pcall_prop(ctx, -3, 1) != DUK_EXEC_SUCCESS) {
    }
    duk_pop_2(ctx);
}

DUK_LOCAL void _on_close_request(struct duk_websocket_t *websocket, int code, const char *reason) {
    duk_context *ctx = websocket->ctx;
    duk_push_heapptr(ctx, websocket->heapptr);
    duk_push_literal(ctx, "dispatch");
    duk_push_literal(ctx, "close_request");
    duk_push_int(ctx, code);
    if (reason) {
        duk_push_string(ctx, reason);
    } else {
        duk_push_null(ctx);
    }
    if (duk_pcall_prop(ctx, -5, 3) != DUK_EXEC_SUCCESS) {
        lwsl_warn("unable to dispatch: %s", duk_to_string(ctx, -1));
    }
    duk_pop_2(ctx);
}

DUK_LOCAL void _on_received(struct duk_websocket_t *websocket) {
    duk_context *ctx = websocket->ctx;
    duk_push_heapptr(ctx, websocket->heapptr);
    duk_push_literal(ctx, "dispatch");
    duk_push_literal(ctx, "data");
    if (websocket->is_binary) {
        duk_push_fixed_buffer(ctx, websocket->len);
        void *buffer = duk_get_buffer_data(ctx, -1, NULL);
        duk_memcpy(buffer, websocket->buf, websocket->len);
    } else {
        duk_push_lstring(ctx, (const char *)(websocket->buf), websocket->len);
    }
    if (duk_pcall_prop(ctx, -4, 2) != DUK_EXEC_SUCCESS) {
        lwsl_warn("unable to dispatch: %s", duk_to_string(ctx, -1));
    }
    duk_pop_2(ctx);
}

DUK_LOCAL int _lws_receive(struct duk_websocket_t *websocket, struct lws *wsi, void *in, size_t len) {
    if (lws_is_first_fragment(wsi)) {
        websocket->len = 0;
    }
    if (websocket->len + len > LWS_PAYLOAD_SIZE) {
        lwsl_debug("receiving payload is too large");
        return -1;
    }
    duk_memcpy(&(((char *)(websocket->buf))[websocket->len]), in, len);
    websocket->len += len;
    if (lws_is_final_fragment(wsi)) {
        websocket->is_binary = lws_frame_is_binary(wsi);
        _on_received(websocket);
    }
    return 0;
}

DUK_LOCAL void _lws_send(struct duk_websocket_t *websocket, struct lws *wsi) {
    struct duk_websocket_payload_t *payload = websocket->pending_head;
    if (payload) {
        websocket->pending_head = payload->next;
        if (websocket->pending_head == NULL) {
            websocket->pending_tail = NULL;
        }
        payload->next = NULL;
        enum lws_write_protocol protocol = payload->is_binary ? LWS_WRITE_BINARY : LWS_WRITE_TEXT;
        lws_write(wsi, &(((char *)(payload->buf))[LWS_PRE]), payload->len, protocol);
        _delete_payload(websocket, payload);
        if (websocket->pending_head) {
            lws_callback_on_writable(websocket->wsi);
        }
    }
}

DUK_LOCAL void _duk_lws_close(struct duk_websocket_t *websocket) {
    if (websocket->wsi) {
        websocket->is_closing = TRUE;
        lws_callback_on_writable(websocket->wsi);
        websocket->wsi = NULL;
    }
}

DUK_LOCAL int _lws_callback_function(struct lws *wsi, 
                                    enum lws_callback_reasons reason,
		                            void *user, 
                                    void *in, 
                                    size_t len) {
    struct duk_websocket_t *websocket = (struct duk_websocket_t *)lws_context_user(lws_get_context(wsi));

    websocket->is_servicing = TRUE;
	switch (reason) {
        case LWS_CALLBACK_OPENSSL_LOAD_EXTRA_CLIENT_VERIFY_CERTS: {
            return 0;
        } 
        case LWS_CALLBACK_CLIENT_ESTABLISHED: {
            websocket->wsi = wsi;
			_on_connect(websocket, lws_get_protocol(wsi)->name);
            return 0;
        } 
        case LWS_CALLBACK_CLIENT_CONNECTION_ERROR: {
            _on_error(websocket);
			_duk_lws_destroy(websocket);
			return -1;
        } 
        case LWS_CALLBACK_WS_PEER_INITIATED_CLOSE: {
            const uint8_t *b = (const uint8_t *)in;
            int code = b[0] << 8 | b[1];
            const char *utf8 = NULL;
            if (len > 2) {
                utf8 = (const char *)&b[2];
            }
            _on_close_request(websocket, code, utf8);
			return 0;
        } 
        case LWS_CALLBACK_CLIENT_CLOSED: {
			_duk_lws_close(websocket);
			_duk_lws_destroy(websocket);
			_on_disconnect(websocket);
            return 0;
        } 
        case LWS_CALLBACK_CLIENT_RECEIVE: {
            return _lws_receive(websocket, wsi, in, len);
        } 
        case LWS_CALLBACK_CLIENT_WRITEABLE: {
            if (websocket->is_closing) {
                lws_close_reason(wsi, LWS_CLOSE_STATUS_NORMAL, "", 0);
                return -1;
            }
            _lws_send(websocket, wsi);
            return 0;
        } 
        default:  {
            return 0;
        }
    }
}

struct IP_Address {
	union {
		uint8_t field8[16];
		uint16_t field16[8];
		uint32_t field32[4];
	};
	duk_bool_t valid;
	duk_bool_t wildcard;
};

DUK_LOCAL void _IP_Address_clear(struct IP_Address *ip) {
	memset(&(ip->field8[0]), 0, sizeof(ip->field8));
	ip->valid = FALSE;
	ip->wildcard = FALSE;
}

DUK_LOCAL void _IP_Address_set_ipv4(struct IP_Address *ip, const uint8_t *p_ip) {
    _IP_Address_clear(ip);
    ip->valid = TRUE;
	ip->field16[5] = 0xffff;
	ip->field32[3] = *((const uint32_t *)p_ip);
}

DUK_LOCAL void _IP_Address_set_ipv6(struct IP_Address *ip, const uint8_t *p_ip) {
	_IP_Address_clear(ip);
	ip->valid = TRUE;
	for (int i = 0; i < 16; i++) {
		ip->field8[i] = p_ip[i];
    }
}

DUK_LOCAL duk_bool_t _resolve_hostname(const char *p_hostname, int p_type, struct IP_Address *ip) {
    if (!ip) {
        return FALSE;
    }
	struct addrinfo hints;
	struct addrinfo *result;

	duk_memzero(&hints, sizeof(struct addrinfo));
	if (p_type == AF_INET) {
		hints.ai_family = AF_INET;
	} else if (p_type == AF_INET6) {
		hints.ai_family = AF_INET6;
		hints.ai_flags = 0;
	} else {
		hints.ai_family = AF_UNSPEC;
		hints.ai_flags = AI_ADDRCONFIG;
	};
	hints.ai_flags &= ~AI_NUMERICHOST;

	int s = getaddrinfo(p_hostname, NULL, &hints, &result);
	if (s != 0) {
		return FALSE;
	};

	if (result == NULL || result->ai_addr == NULL) {
		if (result) {
            freeaddrinfo(result);
        }
		return FALSE;
	};

	if (result->ai_addr->sa_family == AF_INET) {
		struct sockaddr_in *addr = (struct sockaddr_in *)result->ai_addr;
		_IP_Address_set_ipv4(ip, (uint8_t *)&(addr->sin_addr));
	} else if (result->ai_addr->sa_family == AF_INET6) {
		struct sockaddr_in6 *addr6 = (struct sockaddr_in6 *)result->ai_addr;
		_IP_Address_set_ipv6(ip, addr6->sin6_addr.s6_addr);
	};

	freeaddrinfo(result);
	return TRUE;
}

DUK_LOCAL struct duk_websocket_t *duk_get_websocket(duk_context *ctx, duk_idx_t idx) {
    struct duk_websocket_t *websocket = NULL;
    if (duk_get_prop_literal(ctx, idx, DUK_HIDDEN_SYMBOL("websocket"))) {
        websocket = (struct duk_websocket_t *)duk_get_pointer(ctx, -1);
    }
    duk_pop(ctx);
    return websocket;
}

DUK_LOCAL duk_ret_t duk_WebSocket_constructor(duk_context *ctx) {
    duk_idx_t top = duk_get_top(ctx);
    if (top >= 1) {
        if (!duk_is_array(ctx, 0)) {
            return duk_generic_error(ctx, "invalid arg #0 (protocols)");
        }
    }
    duk_size_t protocols_size = duk_get_length(ctx, 0);
    struct duk_websocket_t *websocket = (struct duk_websocket_t *)duk_alloc(ctx, sizeof(struct duk_websocket_t));
    if (websocket == 0) {
        return duk_generic_error(ctx, "unable to alloc websocket");
    }
    duk_memset(websocket, 0, sizeof(struct duk_websocket_t));
    duk_push_this(ctx);
    duk_push_object(ctx);
    duk_put_prop_string(ctx, -2, "events");
    duk_push_pointer(ctx, websocket);
    duk_put_prop_literal(ctx, -2, DUK_HIDDEN_SYMBOL("websocket"));
    websocket->buf = duk_alloc(ctx, LWS_PAYLOAD_SIZE);
    websocket->len = 0;
    duk_memzero(websocket->buf, LWS_PAYLOAD_SIZE);
    websocket->heapptr = duk_get_heapptr(ctx, -1);
    duk_pop(ctx);
    websocket->ctx = ctx;
    websocket->wsi = NULL;
    websocket->protocols = (struct lws_protocols *)duk_alloc(ctx, sizeof(struct lws_protocols) * (protocols_size + 2));
    if (websocket->protocols == NULL) {
        return duk_generic_error(ctx, "unable to alloc websocket protocols");
    }
    duk_memset(websocket->protocols, 0, sizeof(struct lws_protocols) * (protocols_size + 2));
    websocket->protocols_size = 0;
	websocket->protocols[websocket->protocols_size].name = "default";
	websocket->protocols[websocket->protocols_size].callback = _lws_callback_function;
	websocket->protocols[websocket->protocols_size].per_session_data_size = 0;
	websocket->protocols[websocket->protocols_size].rx_buffer_size = LWS_BUF_SIZE;
	websocket->protocols[websocket->protocols_size].tx_packet_size = LWS_PACKET_SIZE;
    websocket->protocols_size = 1;
    for (duk_size_t i = 0; i < protocols_size; i++) {
        duk_get_prop_index(ctx, 0, i);
        duk_size_t protocol_name_length;
        const char *protocol_name_ptr = duk_get_lstring(ctx, -1, &protocol_name_length);
        if (protocol_name_ptr != NULL) {
            char *protocol_name = duk_alloc(ctx, protocol_name_length + 1);
            if (protocol_name != NULL) {
                duk_memcpy(protocol_name, protocol_name_ptr, protocol_name_length);
                protocol_name[protocol_name_length] = '\0';
                websocket->protocols[websocket->protocols_size].name = protocol_name;
                websocket->protocols[websocket->protocols_size].callback = _lws_callback_function;
                websocket->protocols[websocket->protocols_size].per_session_data_size = 0;
                websocket->protocols[websocket->protocols_size].rx_buffer_size = LWS_BUF_SIZE;
                websocket->protocols[websocket->protocols_size].tx_packet_size = LWS_PACKET_SIZE;
                websocket->protocols_size++;
            }
        }
        duk_pop(ctx);
    }
    duk_push_literal(ctx, ",");
    for (duk_size_t i = 0; i < websocket->protocols_size; i++) {
        duk_push_string(ctx, websocket->protocols[i].name);
    }
    duk_join(ctx, websocket->protocols_size);
    duk_size_t protocol_names_length;
    const char *protocol_names_ptr = duk_get_lstring(ctx, -1, &protocol_names_length);
    char *protocol_names = (char *)duk_alloc(ctx, protocol_names_length + 1);
    duk_memcpy(protocol_names, protocol_names_ptr, protocol_names_length);
    protocol_names[protocol_names_length] = '\0';
    websocket->protocol_names = protocol_names;
    duk_pop(ctx); // pop join result
	websocket->protocols[websocket->protocols_size].name = NULL;
	websocket->protocols[websocket->protocols_size].callback = NULL;
	websocket->protocols[websocket->protocols_size].per_session_data_size = 0;
	websocket->protocols[websocket->protocols_size].rx_buffer_size = 0;
    return 0;
}

DUK_LOCAL duk_ret_t duk_WebSocket_finalizer(duk_context *ctx) {
    struct duk_websocket_t *websocket = duk_get_websocket(ctx, 0);
    if (websocket != 0) {
        _duk_lws_destroy(websocket);
        for (int i = 1; i < websocket->protocols_size; i++) {
            struct lws_protocols *p = &(websocket->protocols[i]);
            if (p && p->name) {
                duk_free(ctx, p->name);
                p->name = NULL;
            }
        }
        duk_free(ctx, websocket->protocols);
        websocket->protocols = NULL;
        duk_free(ctx, websocket->protocol_names);
        websocket->protocol_names = NULL;
        struct duk_websocket_payload_t *payload = websocket->freelist;
        while (payload) {
            websocket->freelist = payload->next;
            duk_free(ctx, payload->buf);
            duk_free(ctx, payload);
            payload = websocket->freelist;
        }
        duk_free(ctx, websocket);
    }
    return 0;
}

DUK_LOCAL duk_ret_t duk_WebSocket_connect(duk_context *ctx) {
    char *p_address = duk_require_string(ctx, 0);
    char *p_host = duk_require_string(ctx, 1);
    char *p_path = duk_require_string(ctx, 2);
    duk_int_t p_port = duk_require_int(ctx, 3);
    duk_bool_t p_ssl = duk_require_boolean(ctx, 4);
    duk_bool_t p_allow_self_signed = duk_require_boolean(ctx, 5);

    duk_push_this(ctx);
    struct duk_websocket_t *websocket = duk_get_websocket(ctx, -1);
    duk_pop(ctx); // pop this
    if (websocket == NULL) {
        return duk_generic_error(ctx, "no websocket");
    }
	struct lws_context_creation_info info;
	struct lws_client_connect_info i;
    
	memset(&i, 0, sizeof(i));
	memset(&info, 0, sizeof(info));

	info.port = CONTEXT_PORT_NO_LISTEN;
	info.protocols = websocket->protocols;
	info.gid = -1;
	info.uid = -1;
	//info.ws_ping_pong_interval = 5;
	info.user = websocket; 
	struct lws_context *context = lws_create_context(&info);
    if (context == NULL) {
        return duk_generic_error(ctx, "lws_create_context failed");
    }
    websocket->context = context;
	i.context = context;
    i.protocol = websocket->protocol_names;
    if (p_ssl) {
		i.ssl_connection = LCCSCF_USE_SSL;
		if (p_allow_self_signed) {
            i.ssl_connection |= LCCSCF_ALLOW_SELFSIGNED;
        }
	} else {
		i.ssl_connection = 0;
	}
	i.address = p_address;
	i.host = p_host;
	i.path = p_path;
	i.port = p_port;

	lws_client_connect_via_info(&i);
    return 0;
}

DUK_LOCAL duk_ret_t duk_WebSocket_send(duk_context *ctx) {
    duk_size_t len = 0;
    duk_bool_t is_binary = TRUE;
    void *buf = NULL;
    duk_idx_t top = duk_get_top(ctx);
    if (top == 0) {
        return 0;
    }
    if (duk_is_string(ctx, 0)) {
        buf = duk_get_lstring(ctx, 0, &len);
        is_binary = FALSE;
    } else if (duk_is_buffer_data(ctx, 0)) {
        buf = duk_get_buffer_data(ctx, 0, &len);
    }
    if (len == 0) {
        lwsl_warn("invalid payload object");
        return 0;
    }
    if (len > LWS_PAYLOAD_SIZE) {
        return duk_generic_error(ctx, "payload is too large");
    }

    duk_push_this(ctx);
    struct duk_websocket_t *websocket = duk_get_websocket(ctx, -1);
    duk_pop(ctx); // pop this
    
    if (websocket) {
        if (websocket->is_context_destroyed || websocket->is_closing) {
            lwsl_warn("unable to send, websocket is closing");
            return 0;
        }
        struct duk_websocket_payload_t *payload = _new_payload(websocket);
        duk_memzero(payload->buf, LWS_PRE);
        duk_memcpy(&(((char *)(payload->buf))[LWS_PRE]), buf, len);
        payload->is_binary = is_binary;
        payload->len = len;
        struct duk_websocket_payload_t *tail = websocket->pending_tail;
        if (tail) {
            tail->next = payload;
            websocket->pending_tail = payload;
        } else {
            websocket->pending_head = payload;
            websocket->pending_tail = payload;
        }
        lws_callback_on_writable(websocket->wsi);
    } 
    return 0;
}

DUK_LOCAL duk_ret_t duk_WebSocket_close(duk_context *ctx) {
    duk_push_this(ctx);
    struct duk_websocket_t *websocket = duk_get_websocket(ctx, -1);
    duk_pop(ctx); // pop this
    _duk_lws_close(websocket);
    return 0;
}

DUK_LOCAL duk_ret_t duk_WebSocket_poll(duk_context *ctx) {
    duk_push_this(ctx);
    struct duk_websocket_t *websocket = duk_get_websocket(ctx, -1);
    duk_pop(ctx); // pop this
    if (websocket == NULL || websocket->context == NULL) {
        return 0;
    }
    websocket->is_polling = TRUE;
    do {
        websocket->is_servicing = FALSE;
        lws_service(websocket->context, 0);
    } while (websocket->is_servicing);
    websocket->is_polling = FALSE;

    if (websocket->is_context_destroying) {
        _duk_lws_destroy(websocket);
    }
    return 0;
}

DUK_INTERNAL duk_bool_t duk_websocket_open(duk_context *ctx) {
    duk_push_global_object(ctx);
    duk_unity_get_prop_object(ctx, -1, "DuktapeJS");
    duk_unity_begin_class(ctx, "WebSocket", DUK_UNITY_BUILTINS_WEBSOCKET, duk_WebSocket_constructor, duk_WebSocket_finalizer);
    
    duk_unity_add_member(ctx, "on", duk_events_eventdispatcher_on, -1);
    duk_unity_add_member(ctx, "off", duk_events_eventdispatcher_off, -1);
    duk_unity_add_member(ctx, "clear", duk_events_eventdispatcher_clear, -1);
    duk_unity_add_member(ctx, "dispatch", duk_events_eventdispatcher_dispatch, -1);

    duk_unity_add_member(ctx, "send", duk_WebSocket_send, -1);
    duk_unity_add_member(ctx, "connect", duk_WebSocket_connect, -1);
    duk_unity_add_member(ctx, "close", duk_WebSocket_close, -1);
    duk_unity_add_member(ctx, "poll", duk_WebSocket_poll, -1);
    duk_unity_end_class(ctx);
    duk_pop_2(ctx); // pop DuktapeJS and global    
    return 1;
}
