"use strict";
// experimental
Object.defineProperty(exports, "__esModule", { value: true });
var _Profiling = typeof UnityEngine != "undefined" && UnityEngine["Profiling"];
var _CustomSampler = _Profiling && _Profiling["CustomSampler"];
var _Create = _CustomSampler && _CustomSampler["Create"];
var _samplers = {};
// let _enabled = !!_Create;
var _enabled = false;
var Profiler = /** @class */ (function () {
    function Profiler() {
    }
    Object.defineProperty(Profiler, "enabled", {
        get: function () {
            return _enabled;
        },
        set: function (value) {
            _enabled = _Create && value;
        },
        enumerable: true,
        configurable: true
    });
    return Profiler;
}());
exports.Profiler = Profiler;
function Profiling(target, name, descriptor) {
    if (_enabled) {
        var sampleName = "<JS> " + target.constructor.name + "." + name;
        var sampler_1 = _samplers[sampleName];
        if (!sampler_1) {
            sampler_1 = _samplers[sampleName] = _Create(sampleName);
        }
        if (sampler_1) {
            var origin_1 = descriptor.value;
            descriptor.value = function () {
                try {
                    sampler_1.Begin();
                    origin_1.apply(this, arguments);
                }
                finally {
                    sampler_1.End();
                }
            };
        }
        else {
            console.error("can not get sampler:", sampleName);
        }
    }
}
exports.Profiling = Profiling;
//# sourceMappingURL=profile.js.map