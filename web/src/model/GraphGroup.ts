// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { GraphPrincipal } from "./GraphPrincipal";

export interface GraphGroup extends GraphPrincipal {
	type: 'Group';
	mail?: string;
}