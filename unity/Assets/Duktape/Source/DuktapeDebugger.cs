using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace Duktape
{
    using UnityEngine;

    public class DuktapeDebugger : MonoBehaviour
    {
        private static DuktapeDebugger _instance;
        private static byte[] _buffer;
        public static Action onAttached;

        private DuktapeVM _vm;
        private IntPtr _debugger = IntPtr.Zero;
        private Socket _server;
        private Socket _client;
        private List<Socket> _pending = new List<Socket>();

        public static bool connected
        {
            get { return _instance != null && _instance._client != null && _instance._client.Connected; }
        }

        public static DuktapeDebugger CreateDebugger(DuktapeVM vm)
        {
            return CreateDebugger(vm, 9091, 1024);
        }

        public static DuktapeDebugger CreateDebugger(DuktapeVM vm, int port)
        {
            return CreateDebugger(vm, port, 1024);
        }

        public static DuktapeDebugger CreateDebugger(DuktapeVM vm, int port, int bufferSize)
        {
            if (_instance != null)
            {
                throw new Exception("debugger already exists");
            }
            if (!Application.isPlaying)
            {
                throw new Exception();
            }
            var gameObject = new GameObject("_duktape_debugger");
            _instance = gameObject.AddComponent<DuktapeDebugger>();
            _buffer = new byte[bufferSize];
            _instance._vm = vm;
            _instance.Serve(port);
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null)
            {
                throw new Exception("debugger already exists");
            }
        }

        public static void Shutdown()
        {
            _instance?.Stop();
        }

        private void OnDestroy()
        {
            Stop();
        }

        private void Serve(int port)
        {
            Stop();
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _server.Bind(new IPEndPoint(IPAddress.Any, port));
            _server.Listen(8);
            _server.BeginAccept(_Accept, _server);
        }

        private void _Accept(IAsyncResult ar)
        {
            try
            {
                var socket = _server.EndAccept(ar);
                lock (_pending)
                {
                    if (_pending.Count == 0)
                    {
                        socket.NoDelay = true;
                        _pending.Add(socket);
                        Debug.LogWarningFormat("accept({0}): {1}", _pending.Count, socket.RemoteEndPoint);
                    }
                    else
                    {
                        socket.Close();
                    }
                }
                _server.BeginAccept(_Accept, _server);
            }
            catch (Exception)
            {
                // Debug.LogWarningFormat("debugger server closed: {0}", exception);
            }
        }

        private void Update()
        {
            if (_client == null)
            {
                lock (_pending)
                {
                    if (_pending.Count > 0)
                    {
                        var newClient = _pending[0];
                        _pending.RemoveAt(0);
                        if (newClient.Connected)
                        {
                            DetachCurrent();
                            Debug.LogFormat("debugger connected: {0}", newClient.RemoteEndPoint);
                            _client = newClient;
                            _debugger = DuktapeDLL.duk_unity_attach_debugger(_vm.ctx,
                                duk_unity_debug_read_function,
                                duk_unity_debug_write_function,
                                duk_unity_debug_peek_function,
                                duk_unity_debug_read_flush_function,
                                duk_unity_debug_write_flush_function,
                                duk_unity_debug_request_function,
                                duk_unity_debug_detached_function,
                                0);
                            try
                            {
                                onAttached?.Invoke();
                            }
                            catch (Exception exception)
                            {
                                Debug.LogError(exception);
                            }
                        }
                        else
                        {
                            Debug.LogWarningFormat("dead connection removed");
                        }
                    }
                }
            }
            else
            {
                if (!_client.Connected || (_client.Poll(1000, SelectMode.SelectRead) && _client.Available == 0))
                {
                    // Debug.LogError("dead");
                    _client.Close();
                    _client = null;
                }
            }
        }

        private void DetachCurrent()
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
            if (_vm != null && _vm.ctx != IntPtr.Zero)
            {
                DuktapeDLL.duk_unity_detach_debugger(_vm.ctx, _debugger);
            }
            _debugger = IntPtr.Zero;
        }

        private void Stop()
        {
            lock (_pending)
            {
                _pending.Clear();
            }
            DetachCurrent();
            if (_server != null)
            {
                _server.Close();
                _server = null;
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
                if (_instance != null && _instance._client != null)
                {
                    _instance._client.Close();
                    _instance._client = null;
                }
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
                            // Debug.LogWarningFormat("peek available {0}", n);
                            if (n > 0)
                            {
                                return (uint)n;
                            }
                            // Debug.LogWarningFormat("remote closed");
                        }
                        else if (_instance._client.Poll(1000, SelectMode.SelectError))
                        {
                            // Debug.LogWarningFormat("peek error");
                        }
                        else
                        {
                            // Debug.LogWarningFormat("no data");
                            return 0;
                        }
                    }
                    // Debug.LogWarningFormat("closing");
                    _instance._client.Close();
                    _instance._client = null;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarningFormat("debugger connection lost: {0}", exception);
                if (_instance != null && _instance._client != null)
                {
                    _instance._client.Close();
                    _instance._client = null;
                }
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
            // Debug.LogWarningFormat("duk_unity_debug_request_function: {0}", nvalues);
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_unity_debug_detached_function))]
        private static void duk_unity_debug_detached_function(IntPtr ctx, int udata)
        {
            // Debug.LogWarningFormat("duk_unity_debug_detached_function");
            if (_instance != null && _instance._client != null)
            {
                _instance._client.Close();
                _instance._client = null;
            }
        }
    }
}