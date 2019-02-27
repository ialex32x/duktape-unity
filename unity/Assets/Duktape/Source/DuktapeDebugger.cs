using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace Duktape
{
    using UnityEngine;

    public class DuktapeDebugger
    {
        private static DuktapeDebugger _instance;
        private static byte[] _buffer;
        private IntPtr _ctx;
        private IntPtr _debugger;
        private int _loop;
        private Socket _server;
        private Socket _client;
        private List<Socket> _pending = new List<Socket>();

        private DuktapeDebugger() { }

        public static DuktapeDebugger CreateDebugger(IntPtr ctx)
        {
            return CreateDebugger(ctx, 9091, 1024);
        }

        public static DuktapeDebugger CreateDebugger(IntPtr ctx, int port)
        {
            return CreateDebugger(ctx, port, 1024);
        }

        public static DuktapeDebugger CreateDebugger(IntPtr ctx, int port, int bufferSize)
        {
            if (_instance != null)
            {
                throw new Exception("debugger already exists");
            }
            _buffer = new byte[bufferSize];
            _instance = new DuktapeDebugger();
            _instance._ctx = ctx;
            _instance._debugger = IntPtr.Zero;
            _instance.Start(port);
            return _instance;
        }

        public static void Shutdown()
        {
            if (_instance != null)
            {
                _instance.Stop();
                _instance = null;
            }
        }

        private void Start(int port)
        {
            Stop();
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _server.Bind(new IPEndPoint(IPAddress.Any, port));
            _server.Listen(1);
            _server.BeginAccept(_Accept, _server);
            _loop = DuktapeRunner.SetLoop(OnUpdate);
        }

        private void _Accept(IAsyncResult ar)
        {
            try
            {
                var server = ar.AsyncState as Socket;
                var socket = server.EndAccept(ar);
                lock (_pending)
                {
                    _pending.Add(socket);
                }
                server.BeginAccept(_Accept, server);
            }
            catch (Exception)
            {
                // Debug.LogWarningFormat("debugger closed: {0}", exception);
            }
        }

        private void OnUpdate()
        {
            if (_pending.Count > 0)
            {
                lock (_pending)
                {
                    if (_client == null)
                    {
                        var selected = _pending.Count - 1;
                        if (selected >= 0)
                        {
                            _client = _pending[selected];
                            _pending.RemoveAt(selected);
                            Debug.LogFormat("debugger connected: {0}", _client.RemoteEndPoint);
                            _debugger = DuktapeDLL.duk_unity_attach_debugger(_ctx,
                                duk_unity_debug_read_function,
                                duk_unity_debug_write_function,
                                duk_unity_debug_peek_function,
                                duk_unity_debug_read_flush_function,
                                duk_unity_debug_write_flush_function,
                                duk_unity_debug_request_function,
                                duk_unity_debug_detached_function,
                                0);
                        }
                    }
                    for (int i = 0, size = _pending.Count; i < size; i++)
                    {
                        var socket = _pending[i];
                        try
                        {
                            socket.Close();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private void Stop()
        {
            lock (_pending)
            {
                DuktapeDLL.duk_unity_detach_debugger(_ctx, _debugger);
                _pending.Clear();
                if (_client != null)
                {
                    _client.Close();
                    _client = null;
                }
                if (_server != null)
                {
                    _server.Close();
                    _server = null;
                }
                _debugger = IntPtr.Zero;
                DuktapeRunner.Clear(_loop);
                _loop = 0;
            }
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_unity_debug_read_function))]
        private static uint duk_unity_debug_read_function(int udata, IntPtr buffer, uint length)
        {
            // Debug.LogWarning("duk_unity_debug_read_function");
            try
            {
                if (_instance != null && _instance._client != null)
                {
                    var bufferSize = _buffer.Length;
                    var size = bufferSize > length ? (int)length : bufferSize;
                    // Debug.LogWarningFormat("debugger read: {0} {1}", size, length);
                    var n = _instance._client.Receive(_buffer, size, SocketFlags.None);
                    // Debug.LogWarningFormat("debugger recv: {0}", n);
                    if (n > 0)
                    {
                        Marshal.Copy(_buffer, 0, buffer, n);
                        return (uint)n;
                    }
                    _instance._client.Close();
                    _instance._client = null;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarningFormat("debugger connection lost: {0}", exception);
                _instance._client = null;
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_unity_debug_write_function))]
        private static uint duk_unity_debug_write_function(int udata, IntPtr buffer, uint length)
        {
            // Debug.LogWarning("duk_unity_debug_write_function");
            try
            {
                if (_instance != null && _instance._client != null)
                {
                    var bufferSize = _buffer.Length;
                    var size = bufferSize > length ? (int)length : bufferSize;
                    Marshal.Copy(buffer, _buffer, 0, size);
                    // Debug.LogWarningFormat("debugger write: {0} {1}", size, length);
                    var n = _instance._client.Send(_buffer, size, SocketFlags.None);
                    // Debug.LogWarningFormat("debugger sent: {0}", n);
                    if (n > 0)
                    {
                        return (uint)n;
                    }
                    _instance._client.Close();
                    _instance._client = null;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarningFormat("debugger connection lost: {0}", exception);
                _instance._client = null;
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_unity_debug_peek_function))]
        private static uint duk_unity_debug_peek_function(int udata)
        {
            // Debug.LogWarning("duk_unity_debug_peek_function");
            try
            {
                if (_instance != null && _instance._client != null)
                {
                    if (_instance._client.Connected)
                    {
                        if (_instance._client.Poll(1000, SelectMode.SelectRead))
                        {
                            var n = _instance._client.Available;
                            // Debug.LogWarningFormat("Available {0}", n);
                            if (n > 0)
                            {
                                return (uint)n;
                            }
                            Debug.LogWarningFormat("debugger connection broken");
                        }
                        else if (_instance._client.Poll(1000, SelectMode.SelectError))
                        {
                            // Debug.LogWarningFormat("Error");
                        }
                        else
                        {
                            // Debug.LogWarningFormat("Not Readable");
                            return 0;
                        }
                    }
                    // Debug.LogWarningFormat("debugger closing");
                    _instance._client.Close();
                    _instance._client = null;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarningFormat("debugger connection lost: {0}", exception);
                _instance._client = null;
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_unity_debug_read_flush_function))]
        private static void duk_unity_debug_read_flush_function(int udata)
        {
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_unity_debug_write_flush_function))]
        private static void duk_unity_debug_write_flush_function(int udata)
        {
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_unity_debug_request_function))]
        private static int duk_unity_debug_request_function(IntPtr ctx, int udata, int nvalues)
        {
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_unity_debug_detached_function))]
        private static void duk_unity_debug_detached_function(IntPtr ctx, int udata)
        {
        }
    }
}