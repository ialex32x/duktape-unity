var $protobuf = window.protobuf;
$protobuf.roots.default=window;
// Common aliases
var $Reader = $protobuf.Reader, $Writer = $protobuf.Writer, $util = $protobuf.util;

// Exported root namespace
var $root = $protobuf.roots["default"] || ($protobuf.roots["default"] = {});

$root.protos = (function() {

    /**
     * Namespace protos.
     * @exports protos
     * @namespace
     */
    var protos = {};

    protos.Ping = (function() {

        /**
         * Properties of a Ping.
         * @memberof protos
         * @interface IPing
         * @property {number} time Ping time
         * @property {string} payload Ping payload
         */

        /**
         * Constructs a new Ping.
         * @memberof protos
         * @classdesc Represents a Ping.
         * @implements IPing
         * @constructor
         * @param {protos.IPing=} [properties] Properties to set
         */
        function Ping(properties) {
            if (properties)
                for (var keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null)
                        this[keys[i]] = properties[keys[i]];
        }

        /**
         * Ping time.
         * @member {number} time
         * @memberof protos.Ping
         * @instance
         */
        Ping.prototype.time = 0;

        /**
         * Ping payload.
         * @member {string} payload
         * @memberof protos.Ping
         * @instance
         */
        Ping.prototype.payload = "";

        /**
         * Creates a new Ping instance using the specified properties.
         * @function create
         * @memberof protos.Ping
         * @static
         * @param {protos.IPing=} [properties] Properties to set
         * @returns {protos.Ping} Ping instance
         */
        Ping.create = function create(properties) {
            return new Ping(properties);
        };

        /**
         * Encodes the specified Ping message. Does not implicitly {@link protos.Ping.verify|verify} messages.
         * @function encode
         * @memberof protos.Ping
         * @static
         * @param {protos.IPing} message Ping message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Ping.encode = function encode(message, writer) {
            if (!writer)
                writer = $Writer.create();
            writer.uint32(/* id 1, wireType 0 =*/8).int32(message.time);
            writer.uint32(/* id 2, wireType 2 =*/18).string(message.payload);
            return writer;
        };

        /**
         * Encodes the specified Ping message, length delimited. Does not implicitly {@link protos.Ping.verify|verify} messages.
         * @function encodeDelimited
         * @memberof protos.Ping
         * @static
         * @param {protos.IPing} message Ping message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Ping.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim();
        };

        /**
         * Decodes a Ping message from the specified reader or buffer.
         * @function decode
         * @memberof protos.Ping
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {protos.Ping} Ping
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Ping.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader))
                reader = $Reader.create(reader);
            var end = length === undefined ? reader.len : reader.pos + length, message = new $root.protos.Ping();
            while (reader.pos < end) {
                var tag = reader.uint32();
                switch (tag >>> 3) {
                case 1:
                    message.time = reader.int32();
                    break;
                case 2:
                    message.payload = reader.string();
                    break;
                default:
                    reader.skipType(tag & 7);
                    break;
                }
            }
            if (!message.hasOwnProperty("time"))
                throw $util.ProtocolError("missing required 'time'", { instance: message });
            if (!message.hasOwnProperty("payload"))
                throw $util.ProtocolError("missing required 'payload'", { instance: message });
            return message;
        };

        /**
         * Decodes a Ping message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof protos.Ping
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {protos.Ping} Ping
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Ping.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader))
                reader = new $Reader(reader);
            return this.decode(reader, reader.uint32());
        };

        /**
         * Verifies a Ping message.
         * @function verify
         * @memberof protos.Ping
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Ping.verify = function verify(message) {
            if (typeof message !== "object" || message === null)
                return "object expected";
            if (!$util.isInteger(message.time))
                return "time: integer expected";
            if (!$util.isString(message.payload))
                return "payload: string expected";
            return null;
        };

        return Ping;
    })();

    protos.Pong = (function() {

        /**
         * Properties of a Pong.
         * @memberof protos
         * @interface IPong
         * @property {number} time Pong time
         * @property {string} payload Pong payload
         */

        /**
         * Constructs a new Pong.
         * @memberof protos
         * @classdesc Represents a Pong.
         * @implements IPong
         * @constructor
         * @param {protos.IPong=} [properties] Properties to set
         */
        function Pong(properties) {
            if (properties)
                for (var keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null)
                        this[keys[i]] = properties[keys[i]];
        }

        /**
         * Pong time.
         * @member {number} time
         * @memberof protos.Pong
         * @instance
         */
        Pong.prototype.time = 0;

        /**
         * Pong payload.
         * @member {string} payload
         * @memberof protos.Pong
         * @instance
         */
        Pong.prototype.payload = "";

        /**
         * Creates a new Pong instance using the specified properties.
         * @function create
         * @memberof protos.Pong
         * @static
         * @param {protos.IPong=} [properties] Properties to set
         * @returns {protos.Pong} Pong instance
         */
        Pong.create = function create(properties) {
            return new Pong(properties);
        };

        /**
         * Encodes the specified Pong message. Does not implicitly {@link protos.Pong.verify|verify} messages.
         * @function encode
         * @memberof protos.Pong
         * @static
         * @param {protos.IPong} message Pong message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Pong.encode = function encode(message, writer) {
            if (!writer)
                writer = $Writer.create();
            writer.uint32(/* id 1, wireType 0 =*/8).int32(message.time);
            writer.uint32(/* id 2, wireType 2 =*/18).string(message.payload);
            return writer;
        };

        /**
         * Encodes the specified Pong message, length delimited. Does not implicitly {@link protos.Pong.verify|verify} messages.
         * @function encodeDelimited
         * @memberof protos.Pong
         * @static
         * @param {protos.IPong} message Pong message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Pong.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim();
        };

        /**
         * Decodes a Pong message from the specified reader or buffer.
         * @function decode
         * @memberof protos.Pong
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {protos.Pong} Pong
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Pong.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader))
                reader = $Reader.create(reader);
            var end = length === undefined ? reader.len : reader.pos + length, message = new $root.protos.Pong();
            while (reader.pos < end) {
                var tag = reader.uint32();
                switch (tag >>> 3) {
                case 1:
                    message.time = reader.int32();
                    break;
                case 2:
                    message.payload = reader.string();
                    break;
                default:
                    reader.skipType(tag & 7);
                    break;
                }
            }
            if (!message.hasOwnProperty("time"))
                throw $util.ProtocolError("missing required 'time'", { instance: message });
            if (!message.hasOwnProperty("payload"))
                throw $util.ProtocolError("missing required 'payload'", { instance: message });
            return message;
        };

        /**
         * Decodes a Pong message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof protos.Pong
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {protos.Pong} Pong
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Pong.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader))
                reader = new $Reader(reader);
            return this.decode(reader, reader.uint32());
        };

        /**
         * Verifies a Pong message.
         * @function verify
         * @memberof protos.Pong
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Pong.verify = function verify(message) {
            if (typeof message !== "object" || message === null)
                return "object expected";
            if (!$util.isInteger(message.time))
                return "time: integer expected";
            if (!$util.isString(message.payload))
                return "payload: string expected";
            return null;
        };

        return Pong;
    })();

    return protos;
})();