#define DEFERRED_LOOP // 推迟到内部时间片循环外触发定时器 
using System;
using System.Threading;
using System.Collections.Generic;

namespace Duktape
{
    internal class TimeHandle
    {
        public uint id;
        public Action<uint> action;
        public int deadline;
        public bool deleted;
        public WheelSlot slot;
    }

    internal class WheelSlot
    {
        private List<TimeHandle> _timers = new List<TimeHandle>();

        public void Add(TimeHandle timer)
        {
            timer.slot = this;
            _timers.Add(timer);
        }

        public bool Remove(TimeHandle timer)
        {
            if (_timers.Remove(timer))
            {
                timer.slot = null;
                return true;
            }
            // var size = _timers.Count;
            // for (var i = 0; i < size; ++i)
            // {
            //     var it = _timers[i];
            //     if (it == timer)
            //     {
            //         timer.slot = null;
            //         _timers.RemoveAt(i);
            //         return true;
            //     }
            // }
            return false;
        }

        public void Collect(List<TimeHandle> cache)
        {
            var size = _timers.Count;
            for (var i = 0; i < size; ++i)
            {
                var it = _timers[i];
                it.slot = null;
                cache.Add(it);
            }
            _timers.Clear();
        }
    }

    internal class Wheel
    {
        private int _depth;
        private int _jiffies;
        private int _interval;
        private int _timerange;
        private int _index;
        private WheelSlot[] _slots;

        public int depth { get { return _depth; } }

        public int index { get { return _index; } }

        public int range { get { return _timerange; } }

        public Wheel(int depth, int jiffies, int interval, int slots)
        {
            _depth = depth;
            _index = 0;
            _jiffies = jiffies;
            _interval = interval;
            _timerange = _interval * slots;
            _slots = new WheelSlot[slots];
            for (var i = 0; i < slots; i++)
            {
                _slots[i] = new WheelSlot();
            }
            // var united = (float)_timerange / 1000f;
            // var repr = string.Empty;
            // if (united < 60)
            // {
            //     repr = united + "s";
            // }
            // else if (united < 60 * 60)
            // {
            //     repr = (united / 60) + "m";
            // }
            // else if (united < 60 * 60 * 24)
            // {
            //     repr = (united / (60 * 60)) + "h";
            // }
            // else
            // {
            //     repr = (united / (60 * 60 * 24)) + "d";
            // }
            // UnityEngine.Debug.Log($"[init] wheel#{_depth} scale: {_interval} range: {_timerange} ({repr})");
        }

        public int Add(int delay, TimeHandle timer)
        {
            var offset = Math.Max(1, (delay - _interval + _jiffies - 1) / _interval);
            var index = Math.Max((_index + offset) % _slots.Length, 0);
            _slots[index].Add(timer);
            // UnityEngine.Debug.LogWarning($"[wheel#{_depth}:{_index}<range:{_timerange} _interval:{_interval}>] add timer#{timer.id} delay:{delay} to index: {index} offset: {offset}");
            return index;
        }

        public void Collect(List<TimeHandle> cache)
        {
            _slots[_index].Collect(cache);
            // if (cache.Count > 0)
            // {
            //     UnityEngine.Debug.LogWarning($"[wheel#{_depth}:{_index}<range:{_timerange}>] collect timers {cache.Count}");
            // }
        }

        public bool Tick()
        {
            ++_index;
            if (_depth > 0)
            {
                // UnityEngine.Debug.Log($"[wheel#{_depth}:{_index}<range:{_timerange}>] tick...");
            }
            if (_index == _slots.Length)
            {
                _index = 0;
                return true;
            }
            return false;
        }
    }

    public class Timer
    {
        private static uint _idgen;
        private Scheduler _scheduler;
        private uint _handle;
        private uint _id;
        private bool _enabled;
        private int _repeat;
        private int _interval;
        private Invokable _fn;

        public string name;

        public uint id => _id;

        // 剩余重复次数
        public int repeat
        {
            get { return _repeat; }
            set { _repeat = value; }
        }

        /// 修改计时器时间 (将导致计时器重置)
        public int interval
        {
            get { return _interval; }
            set
            {
                if (_interval != value)
                {
                    _interval = value;
                    if (_enabled)
                    {
                        enabled = false;
                        enabled = true;
                    }
                }
            }
        }

        public Invokable callback
        {
            get { return _fn; }
            set { _fn = value; }
        }

        public Timer(Scheduler scheduler, int interval, Invokable fn, int repeat)
        {
            _id = ++_idgen;
            _scheduler = scheduler;
            _interval = interval;
            _fn = fn;
            _repeat = repeat;
            _enabled = false;
        }

        private void OnTick(uint id)
        {
            _handle = 0;
            --_repeat;
            _fn.Invoke();
            if (_repeat != 0 && _enabled)
            {
                _handle = _scheduler.Add(_interval, OnTick);
            }
            else
            {
                _enabled = false;
            }
            // UnityEngine.Debug.LogWarning($"Timer:{name}({_id}) interval:{_interval} OnTick, repeat: {_repeat} enabled: {_enabled}");
        }

        public bool enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    _scheduler.Remove(_handle);
                    _handle = 0;
                    if (value)
                    {
                        _handle = _scheduler.Add(_interval, OnTick);
                    }
                    // UnityEngine.Debug.LogWarning($"Timer:{name}({_id}) interval:{_interval} EnableChanged, repeat: {_repeat} enabled: {_enabled}");
                }
            }
        }
    }

    public class Scheduler
    {
        private int _threadId;
        private List<TimeHandle> _pool = new List<TimeHandle>();
        private Dictionary<uint, TimeHandle> _timers = new Dictionary<uint, TimeHandle>();
        private Wheel[] _wheels;
        private int _timeslice;
        private int _elapsed;
        private int _jiffies;
        private uint _idgen;
        private List<TimeHandle> _tcache1 = new List<TimeHandle>();
        private List<TimeHandle> _tcache2 = new List<TimeHandle>();

        public Scheduler(int jiffies = 8, int slots = 160, int depth = 4)
        {
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _jiffies = jiffies;
            _wheels = new Wheel[depth];
            for (int i = 0; i < depth; i++)
            {
                int interval = 1;
                for (var j = 0; j < i; j++)
                {
                    interval *= slots;
                }
                _wheels[i] = new Wheel(i, jiffies, jiffies * interval, slots);
            }
        }

        private void Rearrange(TimeHandle timer)
        {
            var delay = Math.Max(0, timer.deadline - _elapsed);
            var wheelCount = _wheels.Length;
            for (var i = 0; i < wheelCount; i++)
            {
                var wheel = _wheels[i];
                if (delay < wheel.range)
                {
                    wheel.Add(delay, timer);
                    // UnityEngine.Debug.Log($"[rearrange] {timer.id} wheel#{i}:{wheel.index}");
                    return;
                }
            }
            _wheels[wheelCount - 1].Add(delay, timer);
            // UnityEngine.Debug.Log($"[rearrange] {timer.id} wheel#{wheelCount - 1}:{_wheels[wheelCount - 1].index}");
        }

        private TimeHandle GetTimeHandle(uint id, int delay, Action<uint> fn)
        {
            var available = _pool.Count;
            TimeHandle timer;
            if (available > 0)
            {
                timer = _pool[available - 1];
                _pool.RemoveAt(available - 1);
            }
            else
            {
                timer = new TimeHandle();
            }
            timer.id = id;
            timer.deadline = delay + _elapsed;
            timer.action = fn;
            timer.deleted = false;
            timer.slot = null;
            return timer;
        }

        public int now
        {
            get { return _elapsed; }
        }

        public Timer CreateTimer(int interval, Invokable fn, int repeat)
        {
            if (_threadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new Exception("scheduler is only available in main thread");
            }
            var timer = new Timer(this, interval, fn, repeat);
            timer.enabled = true;
            return timer;
        }

        public uint Add(int delay, Action<uint> fn)
        {
            if (delay < 0)
            {
                delay = 0;
            }
            var id = ++_idgen;
            var timer = GetTimeHandle(id, delay, fn);
            _timers[id] = timer;
            Rearrange(timer);
            // UnityEngine.Debug.Log($"[Scheduler] Add timer#{timer.id} deadline: {timer.deadline}");
            return id;
        }

        public void Remove(uint id)
        {
            if (id > 0)
            {
                TimeHandle timer;
                if (_timers.TryGetValue(id, out timer))
                {
                    _timers.Remove(id);
                    timer.deleted = true;
                    if (timer.slot != null)
                    {
                        timer.slot.Remove(timer);
                        timer.slot = null;
                    }
                    _pool.Add(timer);
                }
            }
        }

        public void Update(int ms)
        {
            _elapsed += ms;
            _timeslice += ms;
            while (_timeslice >= _jiffies)
            {
                // UnityEngine.Debug.Log($"[schedule] dt:{ms} _elapsed:@{_elapsed} _jiffies:{_jiffies}");
                _timeslice -= _jiffies;
                var wheelIndex = 0;
                // console.log(`[schedule.wheel#${wheelIndex}] slot ${this._wheels[wheelIndex].index} @${this.elapsed}`)
                _wheels[wheelIndex].Collect(_tcache1);
                if (_wheels[wheelIndex].Tick())
                {
                    wheelIndex++;
                    while (wheelIndex < _wheels.Length)
                    {
                        // UnityEngine.Debug.Log($"[schedule.wheel#{wheelIndex}] slot {_wheels[wheelIndex].index} @{_elapsed}");
                        _wheels[wheelIndex].Collect(_tcache2);
                        for (var i = 0; i < _tcache2.Count; ++i)
                        {
                            var timer = _tcache2[i];
                            Rearrange(timer);
                        }
                        _tcache2.Clear();
                        if (!_wheels[wheelIndex].Tick())
                        {
                            break;
                        }
                        wheelIndex++;
                    }
                }
#if !DEFERRED_LOOP
                InvokeTimers();
#endif
            }
#if DEFERRED_LOOP
            InvokeTimers();
#endif
        }

        private void InvokeTimers()
        {
            for (int i = 0, cc = _tcache1.Count; i < cc; ++i)
            {
                var timer = _tcache1[i];
                var handler = timer.action;
                Remove(timer.id);
                // UnityEngine.Debug.LogError($"[timer#{timer.id}] active");
                handler(timer.id);
            }
            _tcache1.Clear();
        }
    }
}
