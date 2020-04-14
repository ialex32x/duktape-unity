// experimental

let _Profiling = typeof UnityEngine != "undefined" && UnityEngine["Profiling"];
let _CustomSampler = _Profiling && _Profiling["CustomSampler"];
let _Create = _CustomSampler && _CustomSampler["Create"];
let _samplers: { [key: string]: any } = {};
// let _enabled = !!_Create;
let _enabled = false;

export class Profiler {
    static get enabled() {
        return _enabled;
    }

    static set enabled(value: boolean) {
        _enabled = _Create && value;
    }
}

export function Profiling(target, name, descriptor: PropertyDescriptor) {
    if (_enabled) {
        let sampleName = "<JS> " + target.constructor.name + "." + name;
        let sampler = _samplers[sampleName];
        if (!sampler) {
            sampler = _samplers[sampleName] = _Create(sampleName);
        }
        if (sampler) {
            let origin: Function = descriptor.value;
            descriptor.value = function () {
                try {
                    sampler.Begin();
                    origin.apply(this, arguments);
                } finally {
                    sampler.End();
                }
            }
        } else {
            console.error("can not get sampler:", sampleName);
        }
    }
}
