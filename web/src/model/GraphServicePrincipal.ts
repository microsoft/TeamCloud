// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { GraphPrincipal } from "./GraphPrincipal";

export interface GraphServicePrincipal extends GraphPrincipal {
	type: 'ServicePrincipal';
	appId?: string;
	appDisplayName: string;
}
