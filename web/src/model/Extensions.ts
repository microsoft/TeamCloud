// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/* eslint-disable no-extend-native */

export { };

declare global {
    interface Date {
        toTimeZoneString(): string;
        toTimeDisplayString(showTimezone: boolean): string;
        toDateTimeDisplayString(showTimezone: boolean): string;
    }
}

Date.prototype.toTimeZoneString = function () {
    return this.toTimeString().slice(19, -1) + ' (' + this.toTimeString().slice(9, 15).replace('GMT', 'UTC') + ':' + this.toTimeString().slice(15, 17) + ')';
}

Date.prototype.toTimeDisplayString = function (showTimezone: boolean = true) {
    return this.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit', timeZoneName: showTimezone ? 'short' : undefined })
}

Date.prototype.toDateTimeDisplayString = function (showTimezone: boolean = true) {
    return `${this.toLocaleDateString()} @ ${this.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit', timeZoneName: showTimezone ? 'short' : undefined })}`;
}
