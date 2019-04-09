
#include "duk_internal.h"

#include "libwebsockets.h"

#define LWS_BUF_SIZE 65536
#define LWS_PACKET_SIZE 65536

struct duk_websocket_t {
    struct lws_protocols *protocols;
    struct lws_context *context;
    duk_bool_t is_servicing;
    duk_bool_t is_polling;
    duk_bool_t is_context_destroying;
    duk_bool_t is_context_destroyed;
};

DUK_LOCAL int _lws_callback_function(struct lws *wsi, 
                                    enum lws_callback_reasons reason,
		                            void *user, 
                                    void *in, 
                                    size_t len) {
    websocket->is_servicing = TRUE;
	switch (reason) {
        case LWS_CALLBACK_OPENSSL_LOAD_EXTRA_CLIENT_VERIFY_CERTS: {
            break;
        } 
        case LWS_CALLBACK_CLIENT_ESTABLISHED: {
            break;
        } 
        case LWS_CALLBACK_CLIENT_CONNECTION_ERROR: {
            break;
        } 
        case LWS_CALLBACK_WS_PEER_INITIATED_CLOSE: {
            break;
        } 
        case LWS_CALLBACK_CLIENT_CLOSED: {
            break;
        } 
        case LWS_CALLBACK_CLIENT_RECEIVE: {
            break;
        } 
        case LWS_CALLBACK_CLIENT_WRITEABLE: {
            break;
        } 
        default:  {
            break;
        }
    }
    return 0;
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
    duk_push_pointer(ctx, websocket);
    duk_put_prop_literal(ctx, -2, DUK_HIDDEN_SYMBOL("websocket"));
    duk_pop(ctx); // pop this
    
    websocket->protocols = (struct lws_protocols *)duk_alloc(ctx, sizeof(struct lws_protocols) * (protocols_size + 2));
    if (websocket->protocols == NULL) {
        return duk_generic_error(ctx, "unable to alloc websocket protocols");
    }
    duk_memset(websocket->protocols, 0, sizeof(struct lws_protocols) * (protocols_size + 2));
	websocket->protocols[0].name = "default";
	websocket->protocols[0].callback = _lws_callback_function;
	websocket->protocols[0].per_session_data_size = 0;
	websocket->protocols[0].rx_buffer_size = LWS_BUF_SIZE;
	websocket->protocols[0].tx_packet_size = LWS_PACKET_SIZE;
    duk_push_literal(ctx, ",");
    for (duk_size_t i = 0; i < protocols_size; i++) {
        duk_get_prop_index(ctx, 0, i);
        const char *protocol_name = duk_get_string(ctx, -1);
        //TODO: copy protocol_name
        websocket->protocols[i + 1].name = protocol_name;
        websocket->protocols[i + 1].callback = _lws_callback_function;
        websocket->protocols[i + 1].per_session_data_size = 0;
        websocket->protocols[i + 1].rx_buffer_size = LWS_BUF_SIZE;
        websocket->protocols[i + 1].tx_packet_size = LWS_PACKET_SIZE;
    }
    duk_join(ctx, protocols_size);
    const char *protocol_names = duk_get_string(ctx, -1);
    //TODO: copy protocol_names
    duk_pop(ctx); // pop join result
	websocket->protocols[protocols_size + 1].name = NULL;
	websocket->protocols[protocols_size + 1].callback = NULL;
	websocket->protocols[protocols_size + 1].per_session_data_size = 0;
	websocket->protocols[protocols_size + 1].rx_buffer_size = 0;
    return 0;
}

DUK_LOCAL void _lws_destroy(struct duk_websocket_t *websocket) {
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
}

DUK_LOCAL duk_ret_t duk_WebSocket_finalizer(duk_context *ctx) {
    struct duk_websocket_t *websocket = duk_get_websocket(ctx, 0);
    if (websocket != 0) {
        _lws_destroy(websocket);
        duk_free(ctx, websocket->protocols);
        duk_free(ctx, websocket);
    }
    return 0;
}

DUK_LOCAL struct duk_websocket_t *duk_get_websocket(duk_context *ctx, duk_idx_t idx) {
    struct duk_websocket_t *websocket = 0;
    if (duk_get_prop_literal(ctx, idx, DUK_HIDDEN_SYMBOL("websocket"))) {
        websocket = duk_get_pointer(ctx, -1);
    }
    duk_pop(ctx);
    return websocket;
}

DUK_LOCAL duk_ret_t duk_WebSocket_connect(duk_context *ctx) {
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
	info.protocols = protocols;
	info.gid = -1;
	info.uid = -1;
	//info.ws_ping_pong_interval = 5;
	info.user = NULL; 
	struct lws_context *context = lws_create_context(&info);
    if (context == NULL) {
        return duk_generic_error(ctx, "lws_create_context failed");
    }
	i.context = context;
    i.protocol = "default";

	if (p_ssl) {
		i.ssl_connection = LCCSCF_USE_SSL;
		if (!verify_ssl)
			i.ssl_connection |= LCCSCF_ALLOW_SELFSIGNED;
	} else {
		i.ssl_connection = 0;
	}

	// These CharStrings needs to survive till we call lws_client_connect_via_info
	CharString addr_ch = ((String)addr).ascii();
	CharString host_ch = p_host.utf8();
	CharString path_ch = p_path.utf8();
	i.address = addr_ch.get_data();
	i.host = host_ch.get_data();
	i.path = path_ch.get_data();
	i.port = p_port;

	lws_client_connect_via_info(&i);

    return 0;
}

DUK_LOCAL duk_ret_t duk_WebSocket_send(duk_context *ctx) {
    duk_push_this(ctx);
    struct duk_websocket_t *websocket = duk_get_websocket(ctx, -1);
    duk_pop(ctx); // pop this
    return 0;
}

DUK_LOCAL duk_ret_t duk_WebSocket_close(duk_context *ctx) {
    duk_push_this(ctx);
    struct duk_websocket_t *websocket = duk_get_websocket(ctx, -1);
    duk_pop(ctx); // pop this
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
        _lws_destroy(websocket);
    }
    return 0;
}

DUK_INTERNAL duk_bool_t duk_websocket_open(duk_context *ctx) {
    duk_push_global_object(ctx);
    duk_unity_get_prop_object(ctx, -1, "DuktapeJS");
    duk_unity_begin_class(ctx, "WebSocket", DUK_UNITY_BUILTINS_WEBSOCKET, duk_WebSocket_constructor, duk_WebSocket_finalizer);
    {
        duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_EVENTDISPATCHER);
        duk_unity_inherit(ctx);
        duk_pop(ctx);
    }
    duk_unity_add_member(ctx, "send", duk_WebSocket_send, -1);
    duk_unity_add_member(ctx, "connect", duk_WebSocket_connect, -1);
    duk_unity_add_member(ctx, "close", duk_WebSocket_close, -1);
    duk_unity_add_member(ctx, "poll", duk_WebSocket_poll, -1);
    duk_unity_end_class(ctx);
    duk_pop_2(ctx); // pop DuktapeJS and global    
    return 1;
}
